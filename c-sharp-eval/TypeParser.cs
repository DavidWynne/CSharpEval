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
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Kamimu
{
  public class TypeParserException : Exception { public TypeParserException(string msg) : base(msg) { } }

  public static class ParserExtensions
  {

    public static bool HasAttribute<T>(this Type type)
    {
      return type.GetCustomAttributes(typeof(T), false).Length > 0;
    }
    public static bool HasAttribute<T>(this FieldInfo fi)
    {
      return fi.GetCustomAttributes(typeof(T), false).Length > 0;
    }
    public static bool HasAttribute<T>(this MemberInfo fi)
    {
      return fi.GetCustomAttributes(typeof(T), false).Length > 0;
    }
    public static bool HasAttribute<T>(this PropertyInfo fi)
    {
      return fi.GetCustomAttributes(typeof(T), false).Length > 0;
    }


    public static T GetAttribute<T>(this Type type)
    {
      object[] t = type.GetCustomAttributes(typeof(T), false);
      if (t.Length == 0) return default(T) ;
      return (T)t[0];
    }
    public static T GetAttribute<T>(this FieldInfo fi)
    {
      object[] t = fi.GetCustomAttributes(typeof(T), false);
      if (t.Length == 0) return default(T);
      return (T)t[0];
    }
    public static T GetAttribute<T>(this MemberInfo fi)
    {
      object[] t = fi.GetCustomAttributes(typeof(T), false);
      if (t.Length == 0) return default(T);
      return (T)t[0];
    }
    public static T GetAttribute<T>(this PropertyInfo fi)
    {
      object[] t = fi.GetCustomAttributes(typeof(T), false);
      if (t.Length == 0) return default(T);
      return (T)t[0];
    }
  }

  public class ExpState
  {
    public readonly LexToken Name;

    public readonly FieldInfo Field;
    public readonly Type ResultType;
    public readonly bool AsStatic;
    public readonly bool IsNull;

    public ExpState()
    {
      IsNull = true;
    }

    public ExpState(Type i)
    {
      if (i == null) {
        IsNull = true;
      } else {
        ResultType = i;
      }
    }
    public ExpState(Type i, bool asStatic)
    {
      ResultType = i;
      AsStatic = asStatic;
    }
    public ExpState(FieldInfo fi)
    {
      Field = fi;
      ResultType = fi.FieldType;
    }
    public ExpState(Type one, Type two) : this(NumericCombinedType(one, two)) { }

    public static Type NumericCombinedType(Type one, Type two)
    {
      if (two == typeof(char) && (one == typeof(int) || one == typeof(long) || one == typeof(float) || one == typeof(double))) return one; 
      if (one == typeof(int)) return two;
      if (one == typeof(long)) {
        if (two == typeof(int) || two == typeof(long)) return typeof(long);
        return typeof(double);
      }
      if (one == typeof(float)) {
        if (two == typeof(double) || two == typeof(long)) return typeof(double);
        return typeof(float);
      }
      return typeof(double);
    }

    public void CheckTypeIs(LexList list, Type type, string msg)
    {
      if (ResultType != type) list.ThrowException(msg);
    }

    public void CheckEqualityTypes(LexList InputList, Type type)
    {
      if (ResultType == null && type == null) return; 
      if (ResultType == null && !type.IsValueType) return; 
      if (!ResultType.IsValueType && (type == null)) return;
      if (ResultType.IsEnum && type == Enum.GetUnderlyingType(ResultType)) return;
      if (type.IsEnum && ResultType == Enum.GetUnderlyingType(type)) return; 
      if ((ResultType == type) || ((IsIntType(ResultType) || IsRealType(ResultType)) && (IsIntType(type) || IsRealType(type)))) return;
      InputList.ThrowException("The left hand operand type '" + ResultType.ToString() + "'\n is not Equality compatible with the right hand operand type '" + type.ToString() + "'.");
    }

    public bool IsIntType(Type type)
    {
      return (type == typeof(int) || type == typeof(long));
    }

    public bool IsRealType(Type type)
    {
      return (type == typeof(float) || type == typeof(double));
    }

    public void CheckComparisonTypes(LexList InputList, Type type)
    {
      if ((ResultType == typeof(string) || ResultType == typeof(bool) || ResultType == typeof(char)) && (ResultType == type)) return;
      if ((IsIntType(ResultType) || IsRealType(ResultType)) && (IsIntType(type) || IsRealType(type))) return;
      if (ResultType.IsEnum && type.IsEnum && ResultType == type) return;
      InputList.ThrowException("The left hand operand type '" + ResultType.ToString() + "'\n is not Comparison compatible with the right hand operand type '" + type.ToString() + "'.");
    }

    public void CheckIsNumberType(LexList InputList, string p)
    {
      if (IsIntType(ResultType) || IsRealType(ResultType)) return;
      InputList.ThrowException(p);
    }

    public void CheckIsIntType(LexList InputList, string p)
    {
      if (IsIntType(ResultType)) return;
      InputList.ThrowException(p);
    }

    public void CheckIsArrayType(LexList InputList, string p)
    {
      if (ResultType.IsArray) return;
      InputList.ThrowException(p);
    }


    public void CheckIsBoolType(LexList InputList, string msg)
    {
      if (ResultType == typeof(bool)) return;
      InputList.ThrowException(msg);
    }
  }

  public class TypeParser
  {
    public static TypeParser DefaultParser = null; 

    public static List<Assembly> GetAllAssemblies(Assembly start)
    {
      List<Assembly> list = new List<Assembly>();
      if (start == null) start = Assembly.GetExecutingAssembly();
      GetReferencedAssemblies(list, start);
      return list;
    }

    public string TypeToString(Type type)
    {
      if (type == null) return "NullType";
      StringBuilder sb = new StringBuilder(300);
      StringBuilder id = new StringBuilder(100);
      bool ignoreMode = false;
      foreach (char ch in type.ToString()) {
        if (ignoreMode && ch >= '0' && ch <= '9') {
        } else {
          ignoreMode = false;
          if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '_' || ch == '.') {
            id.Append(ch);
          } else {
            sb.Append(GetReducedDottedId(id.ToString()));
            id.Length = 0;
            switch (ch) {
            case '[': sb.Append('<'); break;
            case '+': sb.Append('.'); break;
            case ']': sb.Append('>'); break;
            case '`': ignoreMode = true; break;
            default: sb.Append(ch); break;
            }
          }
        }
      }
      sb.Append(GetReducedDottedId(id.ToString()));
      return sb.ToString();
    }

    public static Type GetMethodDelegateType(LexList list, Type[] argumentTypes, bool lastIsReturnType)
    {
      if (lastIsReturnType) {
        switch (argumentTypes.Length) {
        case 1: return typeof(Func<>).MakeGenericType(argumentTypes);
        case 2: return typeof(Func<,>).MakeGenericType(argumentTypes);
        case 3: return typeof(Func<,,>).MakeGenericType(argumentTypes);
        case 4: return typeof(Func<,,,>).MakeGenericType(argumentTypes);
        case 5: return typeof(Func<,,,,>).MakeGenericType(argumentTypes);
        case 6: return typeof(Func<,,,,,>).MakeGenericType(argumentTypes);
        case 7: return typeof(Func<,,,,,,>).MakeGenericType(argumentTypes);
        }
      } else {
        switch (argumentTypes.Length) {
        case 0: return typeof(Action);
        case 1: return typeof(Action<>).MakeGenericType(argumentTypes);
        case 2: return typeof(Action<,>).MakeGenericType(argumentTypes);
        case 3: return typeof(Action<,,>).MakeGenericType(argumentTypes);
        case 4: return typeof(Action<,,,>).MakeGenericType(argumentTypes);
        case 5: return typeof(Action<,,,,>).MakeGenericType(argumentTypes);
        case 6: return typeof(Action<,,,,,>).MakeGenericType(argumentTypes);
        case 7: return typeof(Action<,,,,,,>).MakeGenericType(argumentTypes);
        }
      }
      list.ThrowException("GetMethodDelegateType program error " + argumentTypes.Length + " " + lastIsReturnType);
      return null;
    }

    public static MethodInfo GetDelegateInfo(LexList list, Type type, out Type returnType, out Type[] paras)
    {
      if (type.BaseType != typeof(MulticastDelegate)) list.ThrowException("Type '" + type.Name + " is not a multicast delegate type.");
      MethodInfo invoke = type.GetMethod("Invoke");
      if (invoke == null) list.ThrowException("Type '" + type.Name + " is not a delegate type.");
      paras = (from p in invoke.GetParameters() select p.ParameterType).ToArray();
      returnType = invoke.ReturnType;
      return invoke;
    }

    public TypeParser(Assembly startAssembly, List<string> listUsings)
    {
      Assemblies = TypeParser.GetAllAssemblies(startAssembly) ;
      Usings = listUsings;

      ShortcutType("short", typeof(short));
      ShortcutType("ushort", typeof(ushort));
      ShortcutType("uint", typeof(uint));
      ShortcutType("ulong", typeof(ulong));
      ShortcutType("sbyte", typeof(sbyte));
      ShortcutType("int", typeof(int));
      ShortcutType("bool", typeof(bool));
      ShortcutType("long", typeof(long));
      ShortcutType("float", typeof(float));
      ShortcutType("double", typeof(double));
      ShortcutType("string", typeof(string));
      ShortcutType("char", typeof(char));
      ShortcutType("byte", typeof(byte));
      ShortcutType("object", typeof(object));

      PopulateExtensionMethods();
    }

    public MethodInfo GetExtensionMethod(string name, Type dispatchType, Type[] parameterTypes, LexList lexList)
    {
      List<MethodInfo> newListExact = new List<MethodInfo>();
      List<MethodInfo> newListSubclass = new List<MethodInfo>();
      List<MethodInfo> list = null;
      ExtensionMethods.TryGetValue(name, out list);
      if (list != null) {
        foreach (var m in list) {
          ParameterInfo[] pai = m.GetParameters();
          if (pai.Length == 0 && parameterTypes.Length == 0) {
            newListExact.Add(m); 
          } else {
            if (MatchingParameters(pai, parameterTypes)) {
              if (pai[0].ParameterType == dispatchType) {
                newListExact.Add(m);
              } else if (pai[0].ParameterType.IsSubclassOf(dispatchType)) {
                newListSubclass.Add(m);
              }
            }
          }
        }
      }
      if (newListExact.Count == 1) return newListExact[0];
      if (newListExact.Count == 0 && newListSubclass.Count == 1) return newListSubclass[0];

      string msg = "Method '" + name + "' " ;   
      if (newListExact.Count == 0 && newListSubclass.Count == 0) {
        msg += "was not found";
      } else {
        msg += "is ambigious";
      }
      msg += " in class '" + TypeToString(dispatchType) + "'.";

      if (lexList != null) lexList.ThrowException(msg); else throw new TypeParserException(msg);
      return null;
    }

    // Syntax:
    // Member  = '.' StaticMember .
    public ExpState ParseMember(Type theClass, LexList list, BindingFlags flags, bool implicitDot)
    {
      if (!implicitDot) list.CheckStrAndAdvance(".", "Expected a dot followed by a static member name here.");
      LexToken token = list.CurrentToken();
      string name = list.GetIdentifier("Expected the name of a static member here.");

      FieldInfo fi = theClass.GetField(name, flags);
      if (fi != null) return new ExpState(fi);

      list.Prev();
      return null;
    }

    public Type ParseType(LexList list, bool typeOnly, bool arrayIndexActualsAllowed, BindingFlags visibility)
    {
      bool pendingCloseAngle;
      Type type = Parse(list, typeOnly, arrayIndexActualsAllowed, out pendingCloseAngle, visibility);
      if (pendingCloseAngle) list.ThrowException("Unexpected > here");
      return type;
    }

    public Type ParseType(LexList list, bool typeOnly, bool arrayIndexActualsAllowed, out bool pendingCloseAngle, BindingFlags visibility)
    {
      Type type = Parse(list, typeOnly, arrayIndexActualsAllowed, out pendingCloseAngle, visibility);
      if (pendingCloseAngle) list.ThrowException("Unexpected > here");
      return type;
    }

    // Syntax: 
    // Type               =  TypeNonArray [ TypeArrayPart ] .               
    // TypeNonArray       =  TypeIdAndGenerics *( '.' TypeIdAndGenerics ) . 
    // TypeIdAndGenerics  =  TypeIdentifier [ Generics ] .                  
    // Generics           =  '<' Type *( ',' Type ) '>' .                   
    // TypeArrayPart      =  '[' ']' *( '[' ']' ) .                         

    public Type Parse(LexList list, bool typeOnly, bool arrayIndexActualsAllowed, out bool pendingCloseAngle, BindingFlags visibility)
    {
      string id = "";
      Type returnType = null;
      int checkPoint = list.Index;
      pendingCloseAngle = false;

      while (true) {
        if (list.Kind == LexKind.Type) {
          returnType = list.CurrentToken().ActualObject as Type;
          list.Next();
          return returnType;
        }
        if (list.Kind != LexKind.Identifier) {
          if (typeOnly) list.ThrowException("Unable to convert this to a type.");
          return returnType;
        }
        if (id != "") id += ".";
        id += list.GetIdentifier();
        Type ty = null ; 
        if (list.Str != "<" || !IsGenericSpec ( list ) ) ty = FindTypeFromDottedIds(returnType, id, visibility);
        if (ty != null) {
          id = "";
          returnType = ty;
          checkPoint = list.Index;
          if (list.Kind == LexKind.End) return returnType;
          if (list.Str != ".") return ParseArrayTypeModifier(list, returnType, arrayIndexActualsAllowed);
        } else if (ty == null && list.Str == "<" && IsGenericSpec(list)) {
          // The test for the GenericSpec is needed to separate out comparision '<' or shift '<' from generic brackets.
          int save = list.Index;
          Type[] types = ParseGenericTypeModifier(list, out pendingCloseAngle, visibility);
          id += "`" + types.Length.ToString();
          ty = FindTypeFromDottedIds(returnType, id, visibility);
          if (ty != null) ty = ty.MakeGenericType(types.ToArray());
          if (ty == null) {
            if (typeOnly) list[save].ThrowException("Unable to resolve generic type.", list);
            list.Index = checkPoint;
            return returnType;
          }
          return ParseArrayTypeModifier(list, ty, arrayIndexActualsAllowed);
        } else {
          if (list.Str != ".") {
            if (typeOnly) { list.Prev(); list.ThrowException("Unable to convert this to a type."); }
            list.Index = checkPoint;
            return returnType;
          }
        }
        list.Next(); // skips over the '.'
      }
    }

    // Non-Public after this line.

    private List<Assembly> Assemblies;
    private List<string> Usings;
    private Dictionary<string, string> Shortcuts = new Dictionary<string, string>();
    private Dictionary<string, List<MethodInfo>> ExtensionMethods = new Dictionary<string, List<MethodInfo>>();

    private static void GetReferencedAssemblies(List<Assembly> list, Assembly asm)
    {
      if (!list.Exists(a => a == asm)) {
        list.Add(asm);
        foreach (var an in asm.GetReferencedAssemblies()) {
          Assembly a = Assembly.Load(an);
          list.Add(a);
          GetReferencedAssemblies(list, a);
        }
      }
    }

    private string GetReducedDottedId(string s)
    {
      if (s == "") return "";
      int longest = -1;
      string shortened = "";
      if (Shortcuts.TryGetValue(s, out shortened)) return shortened;
      foreach (var use in Usings) {
        if (use.Length < s.Length - 1 && s[use.Length] == '.' && s.StartsWith(use) && use.Length > longest) {
          // If this Usings name is shorter than the given name, and 
          // the given name starts with this Usings name, and 
          // the first character after the Usings name as it appears in the given name is a dot, and
          // this Usings name is longer than any previous found, then we will use it.
          longest = use.Length;
          shortened = s.Substring(use.Length + 1); // Extract what remains after the Usings name and the dot from the given name.
        }
      }
      if (longest > -1) return shortened;
      return s;
    }
    private Type FindTypeFromDottedIds(Type parentType, string dottedIds, BindingFlags visibility)
    {
      Type type = null;
      if (parentType == null) {
        //type = FindGlobalTypeFromDottedIds(dottedIds);
        type = GlobalTypeCache.Get(dottedIds, this); 
      } else {
        type = parentType.GetNestedType(dottedIds, visibility | BindingFlags.Static);
      }

      return type; // May be null.
    }

    private Memoizer<string, TypeParser, Type> GlobalTypeCache = new Memoizer<string, TypeParser, Type>((dottedIds,parser) =>
    {
      for (int i = parser.Usings.Count - 1; i >= -1; i--) { // Scan backwards through the Usings array, ending up at -1.
        string finalName;
        if (i == -1) finalName = dottedIds; else finalName = parser.Usings[i] + "." + dottedIds;
        foreach (Assembly asm in parser.Assemblies) {
          Type type = asm.GetType(finalName);
          if (type != null) return type; 
        }
      }
      return null; 
    });

    private void ShortcutType(string name, Type type)
    {
      // Allow our code to recognise the same shortcut type names that the C# compiler provides us. (ie 'int' instead of 'Int32' and so on.)
      GlobalTypeCache.Add(name, type);
      Shortcuts.Add(type.ToString(), name);
    }
    private void PopulateExtensionMethods()
    {
      foreach (var a in Assemblies) {
        foreach (var t in a.GetTypes()) {
          foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Static)) {
            bool isExtension = m.IsDefined(typeof(ExtensionAttribute), true);
            List<MethodInfo> list;
            if (ExtensionMethods.TryGetValue(m.Name, out list)) {
              list.Add(m);
            } else {
              ExtensionMethods.Add(m.Name, new List<MethodInfo>() { m });
            }
          }
        }
      }
    }

    private bool MatchingParameters(ParameterInfo[] extensionParameters, Type[] callingParameters)
    {
      if ((extensionParameters.Length - 1) != callingParameters.Length) return false;
      for (int i = 1; i < extensionParameters.Length; i++) {
        if (extensionParameters[i].ParameterType != callingParameters[i - 1] && callingParameters[i - 1].IsSubclassOf(extensionParameters[i].ParameterType)) return false;
      }
      return true;
    }

    // The syntax for a Generic specification is:
    //
    //   GenericSpec = '<' Type *( ',' Type ) '>' .   // The '>' may be in a double delimiter format as '>>' when terminating two nested generic brackets. 
    //   Type        = Id [GenericSpec] *( '.' Id [GenericSpec] ) [ArrayPart] .
    //   ArrayPart   = '[' *',' ']' .
    // 
    // This method only wants to know if the syntax is correct, it does not care about the actual identifiers. 
    // So the syntax can be reduced to the rules:
    // 
    //   Token   Can-be-followed-by
    //    <       Id
    //    >       >  >>  . [ ,
    //    >>      >  >>  . [ ,
    //    Id      <  .  [  ,  > >>
    //    [       ]
    //    ]       ,  >  >> [
    //    ,       Id (if not in ArrayPart) ] (if in ArrayPart)
    private bool IsGenericSpec(LexList list)
    {
      int nesting = 0;
      bool inArrayPart = false;

      for (int i = list.Index; i < list.Count; i++) {
        string next = list[i + 1].Str;
        bool nextIsId = ( list[i + 1].Kind == LexKind.Identifier) || ( list[i+1].Kind == LexKind.Type ) ;
        switch (list[i].Str) {
        case "<":
          nesting++;
          if (!nextIsId) return false;
          break;
        case ">>":
          nesting--;
          goto case ">";
        case ">":
          nesting--;
          if (nesting <= 0) return true;
          if (next == ">" || next == ">>" || next == "." || next == "[" || next == ".") break;
          return false;
        case "[":
          if (inArrayPart) return false;
          inArrayPart = true;
          if (next == "]") break;
          return false;
        case "]":
          if (!inArrayPart) return false;
          inArrayPart = false;
          if (next == "," || next == ">" || next == ">>" || next == "[") break;
          return false;
        case ",":
          if (inArrayPart) {
            if (next == "]") break;
          } else {
            if (nextIsId) break;
          }
          return false;
        default:
          if (list[i].Kind != LexKind.Identifier && list[i].Kind != LexKind.Type) return false;
          if (next == "<" || next == "." || next == "[" || next == "," || next == ">" || next == ">>") break;
          return false;
        }
      }
      return false;
    }

    private Type[] ParseGenericTypeModifier(LexList list, out bool pendingCloseAngle, BindingFlags visibility)
    {
      list.Next();
      pendingCloseAngle = false;
      List<Type> types = new List<Type>();
      bool pending = false;
      while (true) {
        types.Add(Parse(list, true, false, out pending, visibility));
        if (pending) {
          break;
        } else if (list.Kind == LexKind.End) {
          list.ThrowException("Unexpected end of input.");
        } else if (list.Str == ",") {
          list.Next();
        } else if (list.Str == ">") {
          list.Next();
          break;
        } else if (list.Str == ">>") {
          list.Next();
          pendingCloseAngle = true;
          break;
        }
      }
      return types.ToArray();
    }

    private Type ParseArrayTypeModifier(LexList list, Type returnType, bool arrayIndexActualsAllowed)
    {
      if (list.Str == "[") {
        list.Next();
        if (list.Kind == LexKind.End) list.ThrowException("Unexpected end of input.");
        if (!arrayIndexActualsAllowed) {
          if (list.Str != "]") list.ThrowException("Expected a ']' here.");
          list.Next();
        }
        Type arrayType = returnType.MakeArrayType () ;
        while (list.Str == "[") {
          list.Next(); 
          arrayType = arrayType.MakeArrayType();
          if (list.Str != "]") list.ThrowException("Expected a ']' here");
          list.Next(); 
        }
        if (arrayIndexActualsAllowed) list.Prev();
        return arrayType; 
      } else {
        return returnType;
      }
    }

    private static int CountCommas(LexList list, int rank, bool arrayIndexActualsAllowed)
    {
      Stack<string> stack = new Stack<string>(); 
      if (arrayIndexActualsAllowed) {
        int saveIndex = list.Index - 1; 
        bool finished = false; 
        while (!list.AtEnd() && !finished ) {
          switch (list.Str) {
          case ",": if (stack.Count == 0) rank++; break;
          case "(": stack.Push(")"); break;
          case "{": stack.Push("}"); break;
          case "[": stack.Push("]"); break;
          case ")":
          case "}": if (stack.Pop() != list.Str) list.ThrowException("Unbalanced brackets in array initialiser");
            break;
          case "]": 
            if (stack.Count == 0) {
              list.Prev(); 
              finished = true;
            } else {
              if (stack.Pop() != list.Str) list.ThrowException("Unbalanced brackets in array initialiser");
            }
            break;
          }
          list.Next(); 
        }
        list.Index = saveIndex; 
      } else {
        while (list.Str == ",") {
          list.Next();
          rank++;
        }
      }
      return rank;
    }
  }
}
