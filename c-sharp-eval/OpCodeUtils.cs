//Copyright (C) 2009-2010 David Wynne.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
//files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use,
//copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
//Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
//OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
//HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
//FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Text;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
namespace Kamimu
{
  public class KamimuEmitException : Exception { public KamimuEmitException(string msg) : base(msg) { } }

  public class Emit
  {
    class VarInfo
    {
      public int NestingLevel; 
      public Type VarType;  
      bool IsArg;
      public int Ix;
      public LocalBuilder LocBuilder;
      public VarInfo(Type varType, bool isArg, int ix, int nestingLevel, LocalBuilder lb) { LocBuilder = lb; VarType = varType; IsArg = isArg; Ix = ix; NestingLevel = nestingLevel; }
      public VarInfo(Type varType, bool isArg, int ix, int nestingLevel) { VarType = varType; IsArg = isArg; Ix = ix; NestingLevel = nestingLevel; }
      public OpCode Op(bool isLoad)
      {
        if (isLoad) return IsArg ? OpCodes.Ldarg : OpCodes.Ldloc;
        return IsArg ? OpCodes.Starg : OpCodes.Stloc;
      }
    }
    private DynamicMethod Dyno ;
    private Type ReturnTypeValue ;
    private List<Type> ArgTypes = new List<Type>();
    private List<string> ArgNames = new List<string>();
    private List<Type> LocalTypes = new List<Type>();
    private List<string> LocalNames = new List<string>();
    private Dictionary<string, VarInfo> VarAccess = new Dictionary<string, VarInfo>();
    private LexList Comment ; 

    public Emit ( LexList comment ) { Comment = comment ; }
    public Emit() { }

    public void DeclareArg(Type type, LexToken name, LexList theList)
    {
      VarInfo found;
      if (VarAccess.TryGetValue(name.Str, out found)) name.ThrowException("Argument name '" + name.Str + "' is already used.", theList);
      VarAccess.Add(name.Str, new VarInfo(type, true, ArgNames.Count,-1));  
      ArgNames.Add(name.Str);
      ArgTypes.Add(type);
    }
    public void HideNestedLocals(int nestingLevel)
    {
      List<string> toDelete = new List<string>();
      foreach (var v in VarAccess) if (v.Value.NestingLevel > nestingLevel) toDelete.Add(v.Key);
      foreach (var key in toDelete) VarAccess.Remove(key); 
    }

    private int AnonymousLocal = 0; 
    public LocalBuilder DeclareAnonymousLocal(Type type, LexList theList)
    {
      string name = "-AnonymousLocal" + AnonymousLocal++;
      return DeclareLocal(type, new LexToken (name , LexKind.Identifier , theList ), -1, theList);
    }

    public LocalBuilder DeclareLocal(Type type, LexToken name, int nestingLevel, LexList theList) 
    {
      if (!IL.Active) return null; 
      VarInfo found;
      if (VarAccess.TryGetValue(name.Str, out found)) name.ThrowException("Local name '" + name.Str + "' is already used.", theList ); 
      LocalBuilder lb = IL.DeclareLocal_opCodes(type, name.Str);
      VarAccess.Add(name.Str, new VarInfo(type, false , LocalNames.Count, nestingLevel , lb ));
      LocalNames.Add(name.Str);
      LocalTypes.Add(type);
      return lb; 
    }
    public ILDebugGenerator IL;
    public void MakeGenerator (string methodName, Module module )
    {
      Dyno = new DynamicMethod(methodName, ReturnTypeValue, ArgTypes.ToArray() , module);
      IL = new ILDebugGenerator(Dyno.GetILGenerator()) { Args = ArgNames, Locals = LocalNames };
      if (Comment != null) IL.Text.AppendLine(Comment.CodeFormat).AppendLine(); 
      IL.Text.Append( (ReturnTypeValue==null?"void":ReturnTypeValue.ToString())).Append(" ").Append(methodName).Append("("); 
      for (int i = 0; i < ArgTypes.Count; i++) { 
        IL.Text.Append(ArgTypes[i].ToString()).Append(" ").Append(ArgNames[i]);
        if (i<ArgTypes.Count-1) IL.Text.Append(",") ; 
      }
      IL.Text.AppendLine(")") ; 
    }

    public LocalBuilder DeclareIfNotAlready(string name, Type theType)
    {
      VarInfo vi = null;
      VarAccess.TryGetValue(name, out vi);
      if (vi != null) return vi.LocBuilder;
      LocalBuilder lb = IL.DeclareLocal_opCodes(theType, name);
      VarAccess.Add(name, new VarInfo(theType, false, LocalNames.Count, -1, lb));
      LocalNames.Add(name);
      LocalTypes.Add(theType);
      return lb;
    }

    public bool IsLocalOrArg(LexToken token, ref Type theType)
    {
      VarInfo vi = null;
      VarAccess.TryGetValue(token.Str, out vi);
      if (vi == null) return false;
      theType = vi.VarType;
      return true; 
    }

    public Type LoadFromVar(string name, BindingFlags visibility) { return DoLoadOrStore(name, true, visibility); }
    public Type StoreToVar(string name, BindingFlags visibility) { return DoLoadOrStore(name, false, visibility); }

    private Type DoLoadOrStore(string str, bool isLoad, BindingFlags visibility)
    { 
      int pos = str.IndexOf(":");
      if (pos == -1) {
        VarInfo vi = GetVarInfo(str);
        IL.Emit(vi.Op(isLoad), vi.Ix);
        return vi.VarType; 
      } else {
        FieldInfo fi = GetFieldInfo(str.Substring(0, pos), str.Substring(pos + 1), visibility);
        IL.Emit(isLoad ? OpCodes.Ldfld : OpCodes.Stfld, fi);
        return fi.FieldType; 
      }
    }

    private FieldInfo GetFieldInfo(string owner, string field, BindingFlags visibility)
    {
      VarInfo vi = GetVarInfo(owner);
      FieldInfo fi = vi.VarType.GetField(field, visibility | BindingFlags.Instance);
      if (fi == null) throw new KamimuEmitException("Unknown field '" + field + "' in variable '" + owner + "'.");
      return fi; 
    }

    private VarInfo GetVarInfo(string str)
    {
      VarInfo vi;
      if (!VarAccess.TryGetValue(str, out vi)) throw new KamimuEmitException("Unknown variable '" + str + "'.");
      return vi;
    }

    public Label DefineLabel()
    {
      return IL.DefineLabel();
    }
    public void MarkLabel(Label L)
    {
      IL.MarkLabel(L);
    }
    public void ReturnType ( Type type ) { ReturnTypeValue = type ; }
    public Delegate CreateDelegate(Type type) 
    {
      return Dyno.CreateDelegate(type);
    }
  }

  public class ILDebugGenerator
  {
    public ILGenerator IL;
    public StringBuilder Text = new StringBuilder();
    public ILDebugGenerator(ILGenerator il) { IL = il; }
    public List<string> Args ;
    public List<string> Locals ;
    public bool Active = true; 

    private string LoadArg(int i)
    {
      if (i < 0 || i >= Args.Count) return "Load Unknown Arg " + i;
      return "Load " + Args[i]; 
    }
    private string LoadLocal(int i)
    {
      if (i < 0 || i >= Locals.Count) return "Load Unknown Local " + i;
      return "Load " + Locals[i];
    }
    private string StoreArg(int i)
    {
      if (i < 0 || i >= Args.Count) return "Store Unknown Arg " + i;
      return "Store " + Args[i];
    }
    private string StoreLocal(int i)
    {
      if (i < 0 || i >= Locals.Count) return "Store Unknown Local " + i;
      return "Store " + Locals[i];
    }

    private string ConvertOpCode(OpCode op, int i)
    {
      string s = ConvertLoadStores(op, i); 
      if ( s == "" ) return op.ToString() + " " + i ;
      return s + " (" + op.ToString() + " " + i + ")"; 
    }

    private string ConvertLoadStores(OpCode op, int i)
    {
      switch (op.ToString()) {
      case "ldarg.s":
      case "ldarg": return LoadArg(i);

      case "ldloc.s":
      case "ldloc": return LoadLocal(i);

      case "stloc.s":
      case "stloc": return StoreLocal(i);

      case "starg.s":
      case "starg": return StoreArg(i);
      default: return "";
      }
    }

    private string ConvertOpCode(OpCode op)
    {
      string s = ConvertLoadStores(op);
      if (s == "") return op.ToString();
      return s + " (" + op.ToString() + ")"; 
    }

    private string ConvertLoadStores(OpCode op)
    {
      switch (op.ToString()) {
      case "ldarg.0": return LoadArg(0);
      case "ldarg.1": return LoadArg(1);
      case "ldarg.2": return LoadArg(2);
      case "ldarg.3": return LoadArg(3);

      case "ldloc.0": return LoadLocal(0);
      case "ldloc.1": return LoadLocal(1);
      case "ldloc.2": return LoadLocal(2);
      case "ldloc.3": return LoadLocal(3);

      case "stloc.0": return StoreLocal(0);
      case "stloc.1": return StoreLocal(1);
      case "stloc.2": return StoreLocal(2);
      case "stloc.3": return StoreLocal(3);

      default: return "" ;
      }
    }

    private static FieldInfo FieldInfo_LabelNumber = typeof(Label).GetField("m_label", BindingFlags.Instance | BindingFlags.NonPublic);
    public static int GetLabelNumber(Label lab) { return (int)(FieldInfo_LabelNumber.GetValue(lab)); }

    public void Emit(OpCode op) { if (!Active) return;  IL.Emit(op); Text.Append("  ").Append(ConvertOpCode(op)).AppendLine(); }
    public void Emit(OpCode op, string s) { if (!Active) return; IL.Emit(op, s); Text.Append("  ").Append(op.ToString()).Append(' ').Append('"').Append(s).Append('"').AppendLine(); }
    public void Emit(OpCode op, int i) { if (!Active) return; IL.Emit(op, i); Text.Append("  ").Append(ConvertOpCode(op, i)).AppendLine(); }
    public void Emit(OpCode op, long i) { if (!Active) return; IL.Emit(op, i); Text.Append("  ").Append(op.ToString()).AppendLine(); }
    public void Emit(OpCode op, float i) { if (!Active) return; IL.Emit(op, i); Text.Append("  ").Append(op.ToString()).AppendLine(); }
    public void Emit(OpCode op, double i) { if (!Active) return; IL.Emit(op, i); Text.Append("  ").Append(op.ToString()).AppendLine(); }
    public void Emit(OpCode op, FieldInfo fi) { if (!Active) return; IL.Emit(op, fi); Text.Append("  ").Append(op.ToString()).Append(' ').Append(fi.ToString()).AppendLine(); }
    public void Emit(OpCode op, MethodInfo mi) { if (!Active) return; IL.Emit(op, mi); Text.Append("  ").Append(op.ToString()).Append(' ').Append(mi.ToString()).AppendLine(); }
    public void Emit(OpCode op, Type ty) { if (!Active) return; IL.Emit(op, ty); Text.Append("  ").Append(op.ToString()).Append(' ').Append(ty.ToString()).AppendLine(); }
    public void Emit(OpCode op, ConstructorInfo ci) { if (!Active) return; IL.Emit(op, ci); Text.Append("  ").Append(op.ToString()).Append(' ').Append(ci.DeclaringType.ToString()).AppendLine(); }
    public void Emit(OpCode op, Label lab) { if (!Active) return; IL.Emit(op, lab); Text.Append("  ").Append(op.ToString()).Append(' ').Append(" L").Append(GetLabelNumber(lab)).AppendLine(); }
    public void Emit(OpCode op, LocalBuilder lb) { if (!Active) return; IL.Emit(op, lb); Text.Append("  ").Append(op.ToString()).Append(' ').Append(lb.ToString()).AppendLine(); }
    public Label DefineLabel() { if (!Active) return new Label();  Label lab = IL.DefineLabel(); return lab; }
    public void MarkLabel(Label lab) { if (!Active) return; IL.MarkLabel(lab); Text.Append("L").Append(GetLabelNumber(lab)).Append(":").AppendLine(); }
    public void WriteLine(string msg) { if (!Active) return; IL.EmitWriteLine(msg); Text.Append("  WriteLine(" + msg + ")").AppendLine(); }
    public void WriteField(FieldInfo fi) { if (!Active) return; IL.EmitWriteLine(fi); Text.Append("  WriteField(" + fi.ToString() + ")").AppendLine(); }
    public void WriteLocal(LocalBuilder lb) { if (!Active) return; IL.EmitWriteLine(lb); Text.Append("  WriteLocal(" + lb.ToString() + ")").AppendLine(); }

    public void BeginExceptionBlock() { if (!Active) return; IL.BeginExceptionBlock(); Text.Append("Try").AppendLine(); }
    public void BeginCatchBlock(Type type) { if (!Active) return; IL.BeginCatchBlock(type); Text.Append("Catch (").Append(type.ToString()).AppendLine(); }
    public void BeginFinallyBlock() { if (!Active) return; IL.BeginFinallyBlock(); Text.Append("Finally").AppendLine(); }
    public void EndExceptionBlock() { if (!Active) return; IL.EndExceptionBlock(); Text.Append("EndTry").AppendLine(); }

    public void Emit(OpCode op, Label[] labs)
    {
      if (!Active) return; 
      IL.Emit(op, labs);
      Text.Append("  ").Append(op.ToString());
      foreach (var lab in labs) Text.Append(" L").Append(GetLabelNumber(lab));
      Text.AppendLine();
    }
    public LocalBuilder DeclareLocal_opCodes(Type ty, string name) 
    {
      if (!Active) return null; 
      Text.Append( "  Local ").Append(ty.ToString()).Append(" ").Append(name).AppendLine() ; return IL.DeclareLocal(ty); 
    }
    public LocalBuilder DeclareLocal_opCodes(Type ty)
    {
      if (!Active) return null; 
      Text.Append("  Local temp").Append(ty.ToString()).AppendLine(); return IL.DeclareLocal(ty);
    }
  }
}
