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

#if Class

using System.Text;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace Kamimu
{

  public partial class MakeClass
  {
    public static readonly string DoExpressionName = "_____axkjdiusswa__DoExpression";

    public class MadeField 
    {
      public string VarName = "" ; 
      public Type VarType ;
      public int Index; 
    }

    public static string DebugListing = "";
    protected List<Delegate> MethodTable;
    internal  Dictionary<string,CompileMethod> CompileMethodsDict = new Dictionary<string,CompileMethod>();
    protected Dictionary<string, MadeField> MadeFieldsDict = new Dictionary<string, MadeField>(); 
    public string MostRecentNameAdded = "";

    public string GetCodes()
    {
      StringBuilder sb = new StringBuilder(1000);
      foreach (var cm in CompileMethodsDict) cm.Value.GetCodes(sb);
      return sb.ToString(); 
    }

    public MakeClass(TypeParser parser)
    {
      Parser = parser;
    }

    public MakeClass(TypeParser parser, LexList list)
    {
      Parser = parser ;
      AddMethodsAndFields(list, false ); 
    }

    private void Crosslink()
    {
      try {
        InputList.CrosslinkBrackets();
      } catch (Exception ex) {
        InputList.ThrowException(ex.Message);
      }
    }

    public string AllMethods()
    {
      StringBuilder sb = new StringBuilder();
      foreach (var c in CompileMethodsDict) sb.AppendLine(c.Value.Signature);
      return sb.ToString(); 
    }

    public string AllFields()
    {
      StringBuilder sb = new StringBuilder();
      foreach (var c in MadeFieldsDict) sb.Append(Parser.TypeToString( c.Value.VarType)).Append ( ' ' ).AppendLine ( c.Value.VarName ) ;
      return sb.ToString(); 
    }

    public string MethodIL(string methodName)
    {
      CompileMethod cm; 
      if (!CompileMethodsDict.TryGetValue (methodName , out cm)) return "Method " + methodName + " not found" ;
      return cm.MethodIL; 
      
    }

    private Type IsVarKind()
    {
      return  Parser.ParseType(InputList, false, false, BindingFlags.Public) ;
    }

    public R DoExpression<T, R>(T instance, ReadOnlyCollection<string> input) { return DoExpression<T, R>(instance, input, false); }
    public R DoExpression<T, R>(T instance, string input, bool promoteQuotes) { return DoExpression<T, R>(instance, new List<string>() { input }.AsReadOnly(), promoteQuotes); }
    public R DoExpression<T, R>(T instance, string input) { return DoExpression<T, R>(instance , new List<string>() { input }.AsReadOnly(), false ); }
    public R DoExpression<T, R>(T instance, ReadOnlyCollection<string> input, bool promoteQuotes)
    {
      LexListBuilder ll = new LexListBuilder();
      Func<T, R> fn;
      ll.AddAndPromoteQuotes(@" 
          partial class `Type
          {
            public `ReturnType `MethodName() { return ", "Type", typeof(T), "ReturnType" , typeof(R), "MethodName" , DoExpressionName );
      foreach (string line in input) if (promoteQuotes) ll.AddAndPromoteQuotes(line); else ll.Add ( line ) ; 
      ll.AddAndPromoteQuotes(" ;  }}");
      AddMethodsAndFields(ll.ToLexList(), true).GetFunc<T, R>(DoExpressionName, out fn);
      try {
        return fn(instance);
      } catch (Exception ex) {
        ll.ToLexList().ThrowException("This did not run: " + ex.Message);
        return default(R);
      }
    }

    public void DoStatement<T>(T instance, string input) { DoStatement<T>(instance, new List<string>() { input }.AsReadOnly(), false); }
    public void DoStatement<T>(T instance, string input, bool promoteQuotes) { DoStatement<T>(instance, new List<string>() { input }.AsReadOnly(), promoteQuotes); }
    public void DoStatement<T>(T instance, ReadOnlyCollection<string> input) { DoStatement<T>(instance, input, false); }
    public void DoStatement<T>(T instance, ReadOnlyCollection<string> input, bool promoteQuotes)
    {
      LexListBuilder ll = new LexListBuilder();
      Action<T> act;
      ll.AddAndPromoteQuotes(@"
          partial class `Type 
          {
            public void DoStatement () { ", "Type", typeof(T));
      foreach (string line in input) if (promoteQuotes) ll.AddAndPromoteQuotes(line); else ll.Add(line); 
      ll.AddAndPromoteQuotes("}}");
      AddMethodsAndFields(ll.ToLexList(), true).GetAction<T>("DoStatement", out act);
      try {
        act(instance);
      } catch (Exception ex) {
        ll.ToLexList().ThrowException("This did not run: " + ex.Message);
      }
    }

    public void Clear()
    {
      MethodTable = null ;
      CompileMethodsDict.Clear() ; 
      MadeFieldsDict.Clear () ; 
    }

    public MakeClass AddMethodsAndFields(LexList list, bool overwriteAllowed)
    {
      if (InputList != null) PreviousInputLists.Add(InputList);
      InputList = list;
      Type varType = null; 
      var currentListOfMethods = new List<CompileMethod>(); 
      try {
        Crosslink();
        DoClassVisibility();
        DoClassType() ; 
        InputList.CheckStrAndAdvance("{", "Expected an { after the class header.");
        while (true) {
          if (InputList.Str == "}" && InputList.NextIsAtEnd() ) {
            break ;                                  
          } else if ((InputList.Str == "public" || InputList.Str == "private" || InputList.Str == "[" ) && !IsAFieldDefinition()) {
            CompileNextMethodHeader(InputList, overwriteAllowed, currentListOfMethods);
          } else if ((InputList.Str == "public" || InputList.Str == "private") && IsAFieldDefinition()) {
            InputList.Next();
            varType = IsVarKind();
            if (varType == null) InputList.ThrowException("Unknown field type here");
            CompileNextFieldDefinition(InputList, varType); 
          } else if ((varType = IsVarKind()) != null ) {
            CompileNextFieldDefinition(InputList,varType);
          } else {
            InputList.ThrowException("Expected either a 'public' or 'private' keyword (to start a method) or a type (to start a field declaration) here or the closing } of the class.");
          }
        }
        InputList.CheckStrAndAdvance("}" , "Expected a closing } here." ) ;
        if (!InputList.AtEnd() ) InputList.ThrowException("Expected the end of the source text here." ) ; 
        if (MadeFieldsDict.Count > 0) MakeFieldInitialiser(currentListOfMethods);
        FinishClass(currentListOfMethods); 

      } catch (LexListException lle) {
        throw lle;
      } catch (Exception ex) {
        list.CurrentToken().ThrowException(ex.Message, list); 
      }
      return this; 
    }

    private bool IsAFieldDefinition()
    {
      for (int i = InputList.Index; i < InputList.Count; i++) {
        if (InputList[i].Str == "(") return false;
        if (InputList[i].Str == ";") return true;
      }
      InputList.ThrowException("Expected a field definition (with no initialisation) or a method here.");
      return false; 
    }

    private void CompileNextFieldDefinition(LexList theLexList, Type varType)
    {
      string varId = theLexList.GetIdentifier("Expected the name of the field here.");
      MostRecentNameAdded = varId;
      theLexList.CheckStr(";", "Expected a ; here");
      if (MadeFieldsDict.ContainsKey(varId)) theLexList.ThrowException("This field already defined.");
      if (CompileMethodsDict.ContainsKey(varId)) theLexList.ThrowException("This field name already used as a method name.");
      MadeFieldsDict.Add(varId, new MadeField() { VarName = varId, VarType = varType, Index = MadeFieldsDict.Count });
      FieldInfo fi = ClassType.GetField("Fields", BindingFlags.Public | BindingFlags.Instance);
      if (fi == null) theLexList.ThrowException("Can only add fields to a class that has a 'List<Object> Fields' field.");
      if (fi.FieldType != typeof(List<object>)) theLexList.ThrowException("Can only add fields to a class where its Fields field is of type List<object>.");
      theLexList.Next();
    }

    private void CompileNextMethodHeader(LexList theLexList, bool overwriteAllowed, List<CompileMethod> currentListOfMethods)
    {
      CompileMethod cm = DoOneMethod(theLexList) ; 
      currentListOfMethods.Add(cm);
      AddMethodToDictionary(cm, overwriteAllowed);
      if (cm.MethodName != "FieldsInitialiser" ) MostRecentNameAdded = cm.MethodName;
      if (MadeFieldsDict.ContainsKey(cm.MethodName)) theLexList.ThrowException("This method name already used for a field.");
    }

    private void MakeFieldInitialiser(List<CompileMethod> currentListOfMethods )
    {
      LexListBuilder lb = new LexListBuilder() ;
      lb.AddAndPromoteQuotes("public void FieldsInitialiser () { ");
      lb.AddAndPromoteQuotes("if (Fields == null) Fields = new List<object> () ;");
      foreach (var f in ( from f in MadeFieldsDict orderby f.Value.Index select f ) ) {
        Type theType = f.Value.VarType;
        lb.AddAndPromoteQuotes ( "`type `name ;  if (Fields.Count == `index) Fields.Add ( (object)`name ) ; " , "type" , theType, "name" , f.Value.VarName , "index" , f.Value.Index ) ;
        // The conditional 'if (Fields.Count == f.Value.Index)' bit means that this initialisation function can be called repeatedly, and only the new ones will be initialised.
      }
      lb.AddAndPromoteQuotes("}");
      LexList ll = lb.ToLexList();
      ll.CrosslinkBrackets();
      CompileNextMethodHeader(ll, true, currentListOfMethods);
    }

    private void AddMethodToDictionary(CompileMethod cm, bool overwriteAllowed)
    {
      CompileMethod existing = null;
      if (CompileMethodsDict.TryGetValue(cm.MethodName, out existing)) {
        if (!overwriteAllowed) InputList.ThrowException("The method '" + cm.MethodName + "' already exists.");
        if (cm.MethodName != DoExpressionName && existing.MethodDelegateType != cm.MethodDelegateType) InputList.ThrowException("Trying to replace the method '" + cm.MethodName + "', however its signature does not exactly match the existing method.");
        cm.Index = existing.Index;
      } else {
        cm.Index = CompileMethodsDict.Count;
      }
      CompileMethodsDict[cm.MethodName] = cm;
    }

    // Everything beyond this point is not externally accessible. 

    private int MethodIndex = 0 ; 
    private Type ClassType = null ;
    private TypeParser Parser;
    private LexList InputList;
    private List<LexList> PreviousInputLists = new List<LexList>(); 
    private ThisEnv This; 

    private CompileMethod FindCompiledMethod(string methodName, Type delegateType)
    {
      CompileMethod cm;
      if (!CompileMethodsDict.TryGetValue(methodName, out cm)) throw new LexListException ( "Method '" + methodName + "' not found.");
      cm.CompareExpectedAndCompiledDelegateTypes(true, delegateType);
      if (cm.MethodVisibility != BindingFlags.Public) cm.InputList.ThrowException("Method '" + methodName + " is not public.");  
      return cm;
    }

    public bool HasMethod(string name)
    {
      return CompileMethodsDict.ContainsKey(name); 
    }

    private void FinishClass(List<CompileMethod> list)
    {
      This = new ThisEnv(ClassType,CompileMethodsDict, MadeFieldsDict );
      if (MethodTable == null) MethodTable = new List<Delegate> (CompileMethodsDict.Count) ;
      foreach (var cm in list) { 
        while (MethodTable.Count <= cm.Index) MethodTable.Add(null); 
        MethodTable[cm.Index] = cm.Finish(This);
      }
    }

    // ClassVisibility = ( 'partial' 'class' | 'class' 'partial' ) .
    private void DoClassVisibility()
    {   
      if (InputList.AtEnd()) InputList.ThrowException("Unexpected end of text.");
      if (InputList.Str == "partial") {
        InputList.Next();
        InputList.CheckStrAndAdvance("class", "Expected a 'class' after the 'partial'.");
      } else if (InputList.Str == "class") {
        InputList.Next();
        InputList.CheckStrAndAdvance("partial", "Expected a 'partial' after the 'class'.");
      } else {
        InputList.ThrowException("Expected either a 'partial class' or a 'class partial' here.");
      }
    }

    // ClassType 
    private void DoClassType()
    {
      LexToken firstClassToken = InputList.CurrentToken();
      Type thisClassType = null; 
      try {
        thisClassType = Parser.ParseType(InputList, true, false, BindingFlags.Public);
      } catch (Exception ex) {
        InputList.ThrowException("The type of this class needs to be the same as a real C# class in the hosting program."); 
      }
      if (ClassType != null && thisClassType != ClassType) firstClassToken.ThrowException("The class type must be the same as the previous one '" + ClassType.Name + "'", InputList);
      ClassType = thisClassType; 

      if (ClassType.IsValueType) firstClassToken.ThrowException("Cannot use a value type for the class.",InputList);
      if (ClassType.IsAbstract && ClassType.IsSealed) firstClassToken.ThrowException("Cannot use a static class here.", InputList);
      if (ClassType.IsEnum) firstClassToken.ThrowException("Cannot use a Enum type here.", InputList);
      if (!ClassType.IsClass) firstClassToken.ThrowException("Must use a class type here.", InputList);
    }

    // Each method declaration starts with a 'private' or a 'public' and then continues as per a method processed in MakeMethod.cs
    private CompileMethod DoOneMethod( LexList theLexList ) 
    {
      BindingFlags vis = BindingFlags.Public ; 
      if (theLexList.Str == "public") {
        vis = BindingFlags.Public;
      } else if (theLexList.Str == "private") {
        vis = BindingFlags.NonPublic;
      } else {
        theLexList.ThrowException("Expected this method to be marked either public or private.");
      }
      theLexList.Next(); 
      CompileMethod cm = new CompileMethod(Parser, theLexList, ClassType, vis );
      theLexList.CheckStrAndAdvance("}", "Expected a closing } at the end of the method.");
      return cm; 
    }

  }
}

#endif
