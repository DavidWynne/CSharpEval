//Copyright (c) 2009-2010 David Wynne.
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
using System.Diagnostics;
using System.Collections.ObjectModel;
namespace Kamimu
{
  // Method                   =  MethodReturnType MethodName MethodParameters ! StatementBody . 
  // MethodReturnType         =  ( 'void' | OneType ) .                                   
  // MethodParameters         =  '(' MethodParameter *( ',' MethodParameter ) ')' .             
  // MethodParameter          =  OneType ParameterName .                                        

  // StatementBody            =  '{' *Statement '}' .
  // Statement                =  ( IfStatement      |
  //                               WhileStatement   |
  //                               ReturnStatement  |
  //                               VarDeclaration   |
  //                               StatementBody    |
  //                               CallExpression   |
  //                               VarAssignment    |
  //                               DotIdAssignment  |
  //                               ForStatement     |
  //                               IncrementingStatement |
  //                               ArrayElementAssignment ) .

  // IfStatement              =  'if' '(' Expression ')' StatementBody *( 'else' 'if' Conditional StatementBody ) [ 'else' StatementBody ] .
  // WhileStatement           =  'while' '(' Expression ')' StatementBody .
  // ReturnStatement          =  'return' Expression ';' .

  // CallStatement            =  Expression ';' .
  // ForStatement             = 'for' '(' OneType ForLoopIndex '=' DoExpression ';' DoExpression ';' VarAssignment ')' StatementBody .

  // DotIdAssignment          =  Expression '.' LastId '=' Expression ';' .
  // ArrayAssignment          =  Expression ArrayPara '=' Expression ';' . 
  // VarDeclaration           =  ( 'var' | OneType ) VarName [ '=' Expression ] ';' .
  // VarAssignment            =  VarId '=' Expression ';' .

  public class TestTimer
  {
    public static Stopwatch ExpressionSW = new Stopwatch();
    public static long Counter; 
  }

  public class ThisEnv
  {

    public readonly Type ClassType;
    private readonly Dictionary<string, CompileMethod> MethodDictionary;
#if Class
    private readonly Dictionary<string, MakeClass.MadeField> FieldsDictionary; 
#endif

#if Class
    internal ThisEnv(Type classType, Dictionary<string, CompileMethod> methodsDict, Dictionary<string, MakeClass.MadeField> fieldsDict)
    {
      ClassType = classType;
      MethodDictionary = methodsDict;
      FieldsDictionary = fieldsDict;
    }

    internal int GetMadeFieldIndex(LexToken fieldName, out Type fieldType)
    {
      fieldType = null;
      MakeClass.MadeField mf = null;
      if (FieldsDictionary.TryGetValue(fieldName.Str, out mf)) {
        fieldType = mf.VarType;
        return mf.Index;
      } else {
        return -1;
      }
    }

    internal bool IsMadeField(LexToken token)
    {
      return FieldsDictionary.ContainsKey(token.Str); 
    }
#else
    public ThisEnv(Type classType, Dictionary<string, CompileMethod> methodsDict, object notUsed)
    {
      ClassType = classType;
      MethodDictionary = methodsDict;
    }

    internal int GetMadeFieldIndex(LexToken fieldName, out Type fieldType)
    {
      fieldType = null;
      return -1;
    }

    internal bool IsMadeField(LexToken token) { return false; }

#endif
    internal bool IsMember(LexToken token)
    {
      return ClassType.GetMember(token.Str).Length != 0; 
    }

    internal int GetMethodIndex(LexToken methodName, out Type delegateType , out Type returnType )
    {
      delegateType = null;
      returnType = null; 
      CompileMethod cm;
      if (MethodDictionary.TryGetValue(methodName.Str, out cm)) {
        delegateType = cm.MethodDelegateType;
        returnType = cm.MethodReturnType; 
        return cm.Index; 
      } else {
        return -1;
      }
    }


    internal bool IsCompiledMethod(LexToken token)
    {
      return MethodDictionary.ContainsKey(token.Str); 
    }
  }

  /// <summary>
  /// MakeMethod provides a type checking wrapper around the CompileMethod class. The generic type parameter D is the 
  /// expected function or action type of the resulting compiled delegate.
  /// 
  /// For example if the source code to be compiled is 
  /// 
  ///   int IncFn ( float f ) { ... } 
  ///   
  /// then the expected delegate type is Func<float,int>, which is what is used for the D generic type parameter.
  /// 
  /// </summary>
  public class MakeMethod<D> : CompileOneMethod 
  {
    public static D Compile(TypeParser theParser, LexList list)
    {
      list.CrosslinkBrackets(); 
      MakeMethod<D> mm = new MakeMethod<D>(theParser, list);
      mm.CompareExpectedAndCompiledDelegateTypes(typeof(D)); 
      D d = (D)(object)(mm.E).CreateDelegate(typeof(D)); 
      return d; 
    }

    private MakeMethod(TypeParser theParser, LexList list)
      : base(theParser, list, null , BindingFlags.Public )
    {
      FinishMethod(null); 
    }
  }

  public class MakeMethod 
  {

    public static R DoExpression<R>(string input) { return DoExpression<R>(new List<string>() { input }.AsReadOnly(), false); }
    public static R DoExpression<R>(string input, bool promoteQuotes) { return DoExpression<R>(new List<string>() { input }.AsReadOnly(), promoteQuotes); }
    public static R DoExpression<R>(ReadOnlyCollection<string> input) { return DoExpression<R>(input, false); } 

    public static R DoExpression<R>(ReadOnlyCollection<string> input, bool promoteQuotes )
    {
      LexListBuilder ll = new LexListBuilder();
      Func<R> fn;
      ll.AddAndPromoteQuotes(@"public `ReturnType DoExpression() { return  " , "ReturnType" , typeof(R) );
      foreach (string line in input) if (promoteQuotes) ll.AddAndPromoteQuotes(line);  else ll.Add(line);
      ll.AddAndPromoteQuotes(" ;  }");
      fn = MakeMethod<Func<R>>.Compile( TypeParser.DefaultParser , ll.ToLexList () ) ;  
      try {
        return fn();
      } catch (Exception ex) {
        ll.ToLexList().ThrowException("This did not run: " + ex.Message);
        return default(R);
      }
    }

    public static void DoStatement(string input, bool promoteQuotes) { DoStatement(new List<string>() { input }.AsReadOnly(), promoteQuotes); }
    public static void DoStatement(string input) { DoStatement(new List<string>() { input }.AsReadOnly(), false); }
    public static void DoStatement(ReadOnlyCollection<string> input) { DoStatement(input, false); }

    public static void DoStatement(ReadOnlyCollection<string> input, bool promoteQuotes)
    {
      LexListBuilder ll = new LexListBuilder();
      Action act;
      ll.AddAndPromoteQuotes(@"public void DoStatement() { ");
      foreach (string line in input) if (promoteQuotes) ll.AddAndPromoteQuotes(line); else ll.Add(line); 
      ll.AddAndPromoteQuotes("}");
      act = MakeMethod<Action>.Compile(TypeParser.DefaultParser, ll.ToLexList());
      try {
        act ();
      } catch (Exception ex) {
        ll.ToLexList().ThrowException("This did not run: " + ex.Message);
      }
    }
  }

  /// <summary>
  /// Allows a method to be compiled in two halves. The first half is to compile the method header, which is the method name and the 
  /// parameters. Then at some later time the Finish() method is called which will compile the method body and return a delegate to the code.
  /// </summary>
  public class CompileMethod : CompileOneMethod
  {
    public CompileMethod(TypeParser theParser, LexList list)
      : base(theParser, list, null, BindingFlags.Public ) {   }
    public CompileMethod(TypeParser theParser, LexList list, Type classType, BindingFlags vis)
      : base(theParser, list, classType, vis) { }

    public new Type MethodDelegateType { get { return base.MethodDelegateType; } }
    public Type MethodReturnType { get { return base.ReturnType; } }
    public new string MethodName { get { return base.MethodName; } }
    public int Index;

    public Delegate Finish() { return Finish(null); }

    public Delegate Finish(ThisEnv thisEnv)
    {
      FinishMethod(thisEnv);
      Delegate returnDelegate = (E).CreateDelegate(MethodDelegateType);
      return returnDelegate;
    }
  }

  public partial class CompileOneMethod
  {
    public static string DebugListing = "";
    public string MethodIL = ""; 

   
    public void GetCodes(StringBuilder sb)
    {
      sb.Append("Method ").Append(MethodName).AppendLine();
      sb.Append('_', 80).AppendLine();
      sb.AppendLine(E.IL.Text.ToString());
      sb.Append('_', 80).AppendLine();
    }

    /// <summary>
    /// If the stringBuilder has the string s at its end, remove it (by reducing the StringBuilder length by the length of the s string).
    /// </summary>
    public static void SubtractFromStringBuilder(StringBuilder sb, string s)
    {
      int offset = sb.Length - s.Length ; 
      if (s != null && s != "" && s.Length <= sb.Length && sb[offset] == s[0]) {
        for (int a = 0; a < s.Length; a++) if (sb[offset + a] != s[a]) return; 
        sb.Length -= s.Length; 
      }
    }

    internal void CompareExpectedAndCompiledDelegateTypes(Type expectedDelegateType)
    {
      CompareExpectedAndCompiledDelegateTypes(false, expectedDelegateType); 
    }

    internal void CompareExpectedAndCompiledDelegateTypes(bool ignoreSecondArgument, Type expectedDelegateType)
    {
      Type[] theArguments = Arguments ; 
      string expectedName = ReturnType == null ? "Action" : "Func";
      int countReturnType = ReturnType == null ? 0 : 1;
      if (ignoreSecondArgument) {
        theArguments = ( from x in theArguments.Select((item, index) => new {item, index}) where x.index != 1 select x.item ).ToArray() ; 
      }

      if (theArguments.Length > 0) expectedName += "`" + theArguments.Length;
      if (expectedDelegateType.Name[0] != 'F' && expectedName[0] == 'F') InputList.ThrowException("The delegate used is a Func so expected the method to have a return type.");
      if (expectedDelegateType.Name[0] != 'A' && expectedName[0] == 'A') InputList.ThrowException("The delegate used is an Action, so expected the method to have a return type of void.");
      if (expectedDelegateType.Name != expectedName) InputList.ThrowException(
        "The delegate used has " + (expectedDelegateType.Name.StrToInt() - countReturnType) + " parameters while the method\n" +
        "is defined with " + (expectedName.StrToInt() - countReturnType) + " arguments.");

      Type[] check = expectedDelegateType.GetGenericArguments();
      for (var i = 0; i < check.Length; i++) {
        if (check[i] != theArguments[i] && !theArguments[i].IsDerivedFrom ( check[i] ) ) {
          if (i == check.Length - 1 && ReturnType != null) {
            InputList.ThrowException("The delegate has a return type of '" + Parser.TypeToString(check[i]) +
            "'\nbut the return type of the method is '" + Parser.TypeToString(theArguments[i]) + "'.");
          } else {
            InputList.ThrowException("The delegate has parameter " + (i + 1) + " as a '" + Parser.TypeToString(check[i]) +
            "'\nbut the corresponding argument in the method is of type '" + Parser.TypeToString(theArguments[i]) + "'.");
          }
        }
      }
    }

    protected static void CheckForUnprocessedSymbols(LexList inputList)
    {
      for (int i = 0; i < inputList.Count; i++) {
        LexToken tok = inputList[i];
        if (tok.Kind >= LexKind.Symbol00 && tok.Kind <= LexKind.Symbol11 ) {
          inputList.Index = i;
          tok.ThrowException("Unprocessed symbol '" + inputList.Str + "' here.", inputList);
        }
      }
    }

    public LexList InputList;
    public BindingFlags MethodVisibility;
    public string Signature; 

    // Following only visible to subclasses.
    protected Emit E;
    protected Type ReturnType = null ;
    protected TypeParser Parser;
    protected Type[] Arguments;
    protected Type MethodDelegateType ; 
    protected string MethodName;
    protected ThisEnv This;
    protected HashSet<int> TypeCastLocationSet; 


    // Everything beyond this point is not externally accessible. 
    private BindingFlags Visibility = BindingFlags.Public; 
    private int NestingLevel = 0;
    private Stack<LoopLabels> LoopLabelsStack = new Stack<LoopLabels>();

    class LoopLabels
    {
      public Label ContinueLabel;
      public Label BreakLabel;
      public LoopLabels(Emit E)
      {
        ContinueLabel = E.IL.DefineLabel();
        BreakLabel = E.IL.DefineLabel();
      }
    }

    protected CompileOneMethod(TypeParser parser, LexList list, Type classType, BindingFlags vis )
    {
      try {
        MethodVisibility = vis; 
        InputList = list.Clone();
        CheckForUnprocessedSymbols(list); 
        E = new Emit(list);
        Parser = parser;
        DoMethodHeader(classType); // Consumes LexTokens from the method head, up to the method's code block opening {
        InputList.CheckStr("{", "Expected the opening bracket for the method's code body here." );
        list.Index = InputList.Index; 
        list.SkipToAfterClosingBracket();
      } catch (LexListException lle) {
        throw lle;
      } catch (Exception ex) {
        list.CurrentToken().ThrowException(ex.Message, list); 
      }
    }

    // Looks for the syntax:
    //  '(' TypeExpression ')' ( '(' | Value | Identifier )
    private bool IsThisATypeCast()
    {
      if (InputList.Str == "(") {
        for (int j = InputList.Index + 1; j < InputList.Count - 1; j++) {
          switch (InputList[j].Str) {
          case "(":
          case "{":
          case "}": return false;
          case ")":
            LexKind kind = InputList[j + 1].Kind;
            if (InputList[j + 1].Str == "(" ||
              kind == LexKind.String ||
              kind == LexKind.Double ||
              kind == LexKind.Int ||
              kind == LexKind.Float ||
              kind == LexKind.Long ||
              kind == LexKind.Identifier ||
              kind == LexKind.Char ||
              kind == LexKind.Bool
            ) {
              return true;
            } else {
              return false;
            }
          }
        }
      }
      return false;
    }

    // Processes the method code body.
    protected void FinishMethod (ThisEnv thisEnv) 
    {
      if (thisEnv != null) {
        This = thisEnv;
      }
      E.MakeGenerator(MethodName, this.GetType().Module);
      bool fullReturn = DoStatementBody();
      if (ReturnType == null) {
        E.IL.Emit(OpCodes.Ret);
      } else if (!fullReturn && ReturnType != null) {
        InputList.ThrowException("Not all paths terminate in a return statement."); 
      }
      MethodIL = DebugListing = E.IL.Text.ToString() ; 
    }

    private void CallMethod(MethodInfo mi)
    {
      if (mi.IsStatic) {
        E.IL.Emit(OpCodes.Call, mi);
      } else {
        E.IL.Emit(OpCodes.Callvirt, mi);
      }
    }

    private static string ExtractSignature(LexList list)
    {
      StringBuilder sb = new StringBuilder(); 
      for ( int i = list.Index ; i<list.Count ; i++ ) {
        sb.Append ( list[i].Str ) ; 
        if (list[i].Str == ")") break ; 
        sb.Append ( ' ' ) ; 
      }
      return sb.ToString(); 
    }

    private void DoMethodHeader(Type classType)
    {
      ReturnType = null; 
      if (InputList.AtEnd()) InputList.ThrowException("Unexpected end of text.");
      BindingFlags visibility = BindingFlags.Public;

      Signature = ExtractSignature(InputList);
      if ( classType == null && ( InputList.Str == "private" || InputList.Str == "public") ) InputList.Next();  // If classType != null, this is called from MakeClass which has already done the access modifiers.
      if (InputList.Str == "void") {
        InputList.Next();
      } else {
        ReturnType = Parser.ParseType(InputList, true, false, visibility);
        if (E != null) {
          E.ReturnType(ReturnType);
        }
      }
      List<Type> args = new List<Type>();
      MethodName = InputList.GetIdentifier("Expected the method name here");
      InputList.CheckStrAndAdvance("(", "Expected an ( to open the method's parameters here.");

      if (E != null && classType != null) {
        LexToken thisToken = new LexToken("this", LexKind.Identifier, InputList);
        E.DeclareArg(classType, thisToken, InputList);
        args.Add(classType);
        LexToken methodTableToken = new LexToken("methodTable", LexKind.Identifier, InputList);
        E.DeclareArg(typeof(List<Delegate>), methodTableToken, InputList);
        args.Add(typeof(List<Delegate>));
      }

      while (InputList.Str != ")") {
        Type argType = Parser.ParseType(InputList, true, false, visibility);
        LexToken token = InputList.CurrentToken();
        string argName = InputList.GetIdentifier("argument name");
        if (E != null) E.DeclareArg(argType, token, InputList);
        args.Add(argType);
        if (InputList.Str == ",") InputList.Next(); else break;
      }

      InputList.CheckStrAndAdvance(")", "Expected either a , or a ) at the end of the method's parameter list.");
      if (ReturnType != null) args.Add(ReturnType);
      Arguments = args.ToArray();
      MethodDelegateType = TypeParser.GetMethodDelegateType(InputList, Arguments, ReturnType != null); 
    }

    private bool DoStatementBody()
    {
      InputList.CheckStrAndAdvance("{", "Expected opening { brackets here.");
      NestingLevel++;
      bool fullReturn = false; 
      while (InputList.Str != "}") fullReturn = DoStatement();
      InputList.Next(); 
      NestingLevel--;
      E.HideNestedLocals(NestingLevel);
      return fullReturn; 
    }

    // This method echoes the structure of the DoStatement method. 
    public static bool IsVarDeclaration(LexList theList)
    {
      int save = theList.Index;
      if (theList.Str == "public" || theList.Str == "private") theList.Next(); else return false; 
      switch (theList.Str) {

      case "++":
      case "--":
      case "return":
      case "if":
      case "for":
      case "foreach":
      case "while":
      case "break":
      case "continue":
      case "var":
      case "{":
      default:
        if (theList.Kind == LexKind.Identifier && (theList.LookAtToken(+1).Str == "++" || theList.LookAtToken(+1).Str == "--")) {
          theList.Index = save; 
          return false;
        } else {
          int posStatementEnd = FindEndOfStatement(theList, false);
          if (posStatementEnd == -1) {
            theList.Index = save;
            return false;
          }
          string assignmentOperation = "";
          int posEqual = FindEqualSignBeforeEndOfStatement(theList, posStatementEnd, out assignmentOperation);
          if (posEqual == -1) {
            if (theList[posStatementEnd].Str == ")") {
              theList.Index = save;
              return false;
            } else if (theList[posStatementEnd].Kind == LexKind.Identifier) {
              theList.Index = save;
              return true; // The one we want, it is a statement of the format Type id ;
            } else {
              theList.Index = save;
              return false;
            }
          } else {
            theList.Index = save;
            return false;
          }
        }
      }
    }

    private bool DoStatement() { return DoStatement(false); }
    private bool DoStatement(bool endByCloseBracket )
    {
      bool fullReturn = false; 
        switch (InputList.Str) {

        case "++":
        case "--": IncrementingStatement(); break;
        case "return": ReturnStatement(); fullReturn = true;  break;
        case "if": fullReturn = IfStatement(); return fullReturn ;
        case "for": ForStatement(); return false ;
        case "foreach": ForEachStatement(); return false;
        case "while": WhileStatement(); return false ;
        case "break":
        case "continue": BreakContinueStatement(); break;
        case "switch": InputList.ThrowException("Sorry, the switch structure is not implemented."); break; 
        case "var": VarDeclaration_ImplicitType(); break;
        case "{": fullReturn = DoStatementBody(); return fullReturn ;
        default:
          if (InputList.Kind == LexKind.Identifier && (InputList.LookAtToken(+1).Str == "++" || InputList.LookAtToken(+1).Str == "--")) {
            IncrementingStatement();
          } else {
            int posStatementEnd = FindEndOfStatement(InputList,true);
            string assignmentOperation = ""; 
            int posEqual = FindEqualSignBeforeEndOfStatement(InputList, posStatementEnd, out assignmentOperation );
            if (posEqual == -1) {
              if (InputList[posStatementEnd].Str == ")") {
                DoCallStatement();
              } else if (InputList[posStatementEnd].Kind == LexKind.Identifier) {
                VarDeclaration_ExplicitType();
              } else {
                InputList.ThrowException("Unknown statement here.");
              }
            } else {
              bool isImplicitThisMember = This != null && This.IsMember(InputList.CurrentToken());
              if (InputList[posEqual - 1].Str == "]") {
                if (assignmentOperation != "") InputList.ThrowException("Assignment =" + assignmentOperation + " not implemented for array element references."); 
                int openSquarePosition = FindMatchingOpenBracket("[", "]", posEqual - 1);
                DoArrayElementAssignment(openSquarePosition, isImplicitThisMember);
              } else if (isImplicitThisMember) {
                if (assignmentOperation != "") InputList.ThrowException("Assignment =" + assignmentOperation + " not implemented for implicit 'this' references.");
                DoDotIdAssignment(posEqual - 2, true);
              } else if (posEqual == InputList.Index + 1) {
                DoVarAssignment(assignmentOperation);
              } else if (InputList[posEqual - 2].Str == ".") {
                if (assignmentOperation != "") InputList.ThrowException("Assignment =" + assignmentOperation + " not implemented for dotted references."); 
                DoDotIdAssignment(posEqual - 2, false);
              } else {
                if (assignmentOperation != "") InputList.ThrowException("Assignment =" + assignmentOperation + " not expected for a variable declaration and initialisation."); 
                VarDeclaration_ExplicitType();
              }
            }
          }
          break;
        }
      if (endByCloseBracket) {
        if (InputList.Str != ")") InputList.ThrowException("Expected a ')' to end this statement.");
      } else {
        if (InputList.Str != "}" && InputList.Str != ";") InputList.ThrowException("Expected ';' (or '}')at end of statement.");
        InputList.Next();
      }
      return fullReturn; 
    }

    private void BreakContinueStatement()
    {
      if (LoopLabelsStack.Count == 0) InputList.ThrowException("The " + InputList.Str + " is not inside a loop of some sort.");
      if (InputList.Str == "break") {
        E.IL.Emit(OpCodes.Br, LoopLabelsStack.Peek().BreakLabel);
      } else {
        E.IL.Emit(OpCodes.Br, LoopLabelsStack.Peek().ContinueLabel);
      }
      InputList.Next(); 
    }

    private void WhileStatement()
    {
      LoopLabelsStack.Push ( new LoopLabels ( E ) ) ;

      E.IL.MarkLabel(LoopLabelsStack.Peek().ContinueLabel);
      InputList.Next();
      InputList.CheckStrAndAdvance("(", "Expected the loop invariant expression here.");
      ExpState exp = DoExpression () ;
      exp.CheckIsBoolType(InputList, "Expected a boolean expression for the while invariant." ); 
      InputList.CheckStrAndAdvance ( ")" , "Expected closing ) here." ) ;
      E.IL.Emit ( OpCodes.Brfalse, LoopLabelsStack.Peek().BreakLabel ) ; 
      DoStatement();
      E.IL.Emit(OpCodes.Br, LoopLabelsStack.Peek().ContinueLabel);
      E.IL.MarkLabel ( LoopLabelsStack.Peek().BreakLabel) ; 
      LoopLabelsStack.Pop() ; 
    }

    private void ForEachStatement()
    {
      LexToken varToken ; // The name of the forEach variable
      LexToken enumeratorToken; // The name (not to be used by the code itself) of the local variable that holds the foreach enumerator structure.
      ExpState exp ;// The type of the foreach expression (which evaluates to an enumerator structure of the correct type).
      MethodInfo Method_GetEnumerator ; // The methodInfo on the foreach expression result to get the enumerator structure. 
      Type enumeratorType ; // The type of this enumerator structure
      LocalBuilder enumerator ; // The IL reference to the variable that holds the enumerator structure.
      MethodInfo method_get_Current; // The method info for the get_Current method of the enumerator.
      MethodInfo method_MoveNext; // The method info for the MoveNext method of the enumerator.
      MethodInfo method_Dispose; // The method info for the Dispose method of the enumerator.
      Type varType ; // The type of the foreach variable (that one that is explicitly used by the source code).
      Label Lconditional ; // The label to jump to to work out if there is anything more in the enumeration.
      Label Lcontinue ; // The loop around and start again label.
      Label Lbreak; 

      Lconditional = E.IL.DefineLabel();
      Lcontinue = E.DefineLabel();
      Lbreak = E.DefineLabel(); 

      OpenForStatement();
      InputList.CheckStrAndAdvance("var", "Expected a 'var' after the '(' in a foreach statement.");

      varToken = InputList.CurrentToken(); 
      enumeratorToken = new LexToken(varToken.Str + "__enumerator_", LexKind.Identifier, InputList); 

      InputList.GetIdentifier("Expected the variable name here.");
      InputList.CheckStrAndAdvance("in", "Expected an 'in' after the foreach variable."); 
      exp = DoExpression();
      InputList.CheckStrAndAdvance(")", "Expected the closing ')' for the foreach here."); 
      if (exp.ResultType == null) InputList.ThrowException("Unknown input here, expected a value that has a GetEnumerator() method.");

      if (exp.ResultType.IsArray) {
        LocalBuilder indexerVar = E.DeclareAnonymousLocal(typeof(int), InputList );
        LocalBuilder elementVar = E.DeclareLocal(exp.ResultType.GetElementType(), varToken, NestingLevel, InputList);
        LocalBuilder arrayVar = E.DeclareAnonymousLocal(exp.ResultType, InputList);
        E.IL.Emit(OpCodes.Stloc, arrayVar); 
        E.IL.Emit(OpCodes.Ldc_I4_0);
        E.IL.Emit(OpCodes.Stloc, indexerVar); // ix = 0 ;
        E.IL.MarkLabel(Lconditional);
        
        E.IL.Emit(OpCodes.Ldloc, arrayVar);
        MethodInfo method_Length = exp.ResultType.GetMethod("get_Length", BindingFlags.Public | BindingFlags.Instance);
        CallMethod ( method_Length); 
        E.IL.Emit(OpCodes.Ldloc, indexerVar);
        E.IL.Emit(OpCodes.Ble, Lbreak);

        E.IL.Emit(OpCodes.Ldloc, arrayVar);
        E.IL.Emit(OpCodes.Ldloc, indexerVar); 
        E.IL.Emit(OpCodes.Ldelem, exp.ResultType.GetElementType());

        E.IL.Emit(OpCodes.Stloc, elementVar);

        DoStatement();

        E.IL.MarkLabel(Lcontinue); 
        E.IL.Emit(OpCodes.Ldloc, indexerVar);
        E.IL.Emit(OpCodes.Ldc_I4_1); 
        E.IL.Emit(OpCodes.Add);
        E.IL.Emit(OpCodes.Stloc, indexerVar);
        E.IL.Emit(OpCodes.Br, Lconditional);

        E.IL.MarkLabel(Lbreak); 
      } else {
        Method_GetEnumerator = exp.ResultType.GetMethod("GetEnumerator", BindingFlags.Public | BindingFlags.Instance);
        if (Method_GetEnumerator == null) InputList.ThrowException("Expected this type '" + exp.ResultType.Name + " to have a GetEnumerator method.");
        enumeratorType = Method_GetEnumerator.ReturnType;
        if (enumeratorType == null) InputList.ThrowException("The GetEnumertor method of '" + exp.ResultType.Name + "' does not have a return type.");
        enumerator = E.DeclareLocal(enumeratorType, enumeratorToken, NestingLevel, InputList);

        method_get_Current = enumeratorType.GetMethod("get_Current", BindingFlags.Public | BindingFlags.Instance);
        method_MoveNext = enumeratorType.GetMethod("MoveNext", BindingFlags.Public | BindingFlags.Instance);
        method_Dispose = enumeratorType.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance);

        varType = method_get_Current.ReturnType;
        E.DeclareLocal(varType, varToken, NestingLevel, InputList);
        CallMethod ( Method_GetEnumerator);
        E.StoreToVar(enumeratorToken.Str, Visibility);

        E.IL.BeginExceptionBlock();

        E.IL.Emit(OpCodes.Br, Lconditional);
        E.IL.MarkLabel(Lcontinue);
        if (enumeratorType.IsValueType) E.IL.Emit(OpCodes.Ldloca, enumerator); else E.IL.Emit(OpCodes.Ldloc, enumerator);
        CallMethod( method_get_Current);
        E.StoreToVar(varToken.Str, Visibility);

        DoStatement();

        E.IL.MarkLabel(Lconditional);
        if (enumeratorType.IsValueType) E.IL.Emit(OpCodes.Ldloca, enumerator); else E.IL.Emit(OpCodes.Ldloc, enumerator);
        CallMethod(method_MoveNext);
        E.IL.Emit(OpCodes.Brtrue, Lcontinue);

        E.IL.BeginFinallyBlock();

        if (method_Dispose != null) {
          if (enumeratorType.IsValueType) E.IL.Emit(OpCodes.Ldloca, enumerator); else E.IL.Emit(OpCodes.Ldloc, enumerator);
          E.IL.Emit(OpCodes.Constrained, enumeratorType);
          CallMethod ( method_Dispose);
        }
        E.IL.EndExceptionBlock();
      }
      CloseForStatement();
    }

    private void ForStatement()
    {
      OpenForStatement(); 

      // Loop initialisation
      if (InputList.Str != ";") DoStatement(); else InputList.Next(); // The for loop initialisation clause.
      Label continueLabel = E.IL.DefineLabel ( ) ; 
      E.IL.MarkLabel ( continueLabel ) ; 
      
      // Loop conditional
      if (InputList.Str != ";") {
        ExpState state = DoExpression();
        state.CheckIsBoolType(InputList, "Expected a boolean expression here.");
        E.IL.Emit(OpCodes.Brfalse, LoopLabelsStack.Peek().BreakLabel);
        InputList.CheckStrAndAdvance(";", "Expected a ';' here."); 
      } else {
        InputList.Next();
      }

      // Loop advancement
      Label skipAround = E.IL.DefineLabel();
      E.IL.Emit(OpCodes.Br, skipAround);
      E.IL.MarkLabel(LoopLabelsStack.Peek().ContinueLabel);
      if (InputList.Str != ")") DoStatement(true);   // The for loop advancement clause.
      E.IL.Emit(OpCodes.Br, continueLabel);
      E.IL.MarkLabel(skipAround);

      // Loop statement body
      InputList.CheckStrAndAdvance(")", "Expected the closing ')' of the for loop header.");
      DoStatement(); // The for loop statement body.
      E.IL.Emit(OpCodes.Br, LoopLabelsStack.Peek().ContinueLabel);
      E.IL.MarkLabel(LoopLabelsStack.Peek().BreakLabel);

      CloseForStatement();
    }

    private void OpenForStatement()
    {
      NestingLevel++; 
      LoopLabelsStack.Push(new LoopLabels(E));
      InputList.Next();
      InputList.CheckStrAndAdvance("(", "Expected the opening '(' of the for loop header here.");
    }

    private void CloseForStatement()
    {
      LoopLabelsStack.Pop();
      NestingLevel--;
      E.HideNestedLocals(NestingLevel);
    }

    private bool IfStatement()
    {
      
      bool endLabelWanted = true; 
      DoIfConditional();
      Label endLabel = E.IL.DefineLabel();
      Label nextLabel = E.IL.DefineLabel();
      E.IL.Emit(OpCodes.Brfalse, nextLabel);
      bool fullReturn = DoStatement();
      if (!fullReturn) E.IL.Emit(OpCodes.Br, endLabel); 

      while (InputList.Str == "else") {
        E.IL.MarkLabel(nextLabel);
        InputList.Next();
        if (InputList.Str == "if") {
          DoIfConditional();
          nextLabel = E.IL.DefineLabel();
          E.IL.Emit(OpCodes.Brfalse, nextLabel);
          bool ret = DoStatement();
          fullReturn &= ret; 
          if (!ret) E.IL.Emit(OpCodes.Br, endLabel); 
        } else {
          fullReturn &= DoStatement();
          endLabelWanted = false; 
          break; 
        }
      }
      E.IL.MarkLabel(endLabel); 
      if (endLabelWanted) E.IL.MarkLabel(nextLabel);
      return fullReturn; 
    }

    private void DoIfConditional()
    {
      InputList.Next();
      InputList.CheckStrAndAdvance("(", "Expected the if conditional expression here.");
      ExpState exp = DoExpression();
      exp.CheckIsBoolType(InputList, "Expected a boolean expression for the if conditional.");
      InputList.CheckStrAndAdvance(")", "Expected closing ) here.");
    }

    private int FindMatchingOpenBracket(string openBracket, string closeBracket, int closingPosition )
    {
      int nesting = 0; 
      for (int i = closingPosition; i >= 0; i--) {
        if (InputList[i].Str == openBracket) nesting++; else if (InputList[i].Str == closeBracket) nesting--;
        if (nesting == 0) return i; 
      }
      InputList[closingPosition].ThrowException("Unable to find matching opening bracket", InputList);
      return -1; 
    }

    private void ReturnStatement()
    {
      InputList.Next();
      ExpState es = DoExpression();
      if (es == null) {
        if (ReturnType != null) InputList.ThrowException("There needs to be a return expression."); 
      } else {
        if (ReturnType == null) InputList.ThrowException("This method has a void return type, so no expression expected after the return keyword."); 
        ConvertToType(es.ResultType, ReturnType);
      }
      E.IL.Emit(OpCodes.Ret);
    }

    private void VarDeclaration_ImplicitType()
    {
      InputList.Next();
      Type theType = null ; 
      LexToken token = InputList.CurrentToken();
      InputList.GetIdentifier("Expected the name of a new local variable here.");
      if (E.IsLocalOrArg(token, ref theType)) InputList.GetIdentifier("The variable '" + token.Str + "' has already been defined.");
      InputList.CheckStrAndAdvance("=", "Expected an = sign here.");
      ExpState ess = DoExpression();
      theType = ess.ResultType;
      E.DeclareLocal(theType, token, NestingLevel, InputList);
      E.StoreToVar(token.Str, Visibility); 
    }

    private void VarDeclaration_ExplicitType()
    {
      Type theType = Parser.ParseType(InputList, true, false, Visibility);
      LexToken token = InputList.CurrentToken();
      InputList.GetIdentifier("Expected the name of a new local variable here.");
      Type unusedType = null ; 
      if (E.IsLocalOrArg(token, ref unusedType)) InputList.GetIdentifier("The variable '" + token.Str + "' has already been defined.");
      ExpState ess = null;
      if (InputList.CurrentToken().Str != ";") {
        InputList.CheckStrAndAdvance("=", "Expected an = sign here.");
        ess = DoExpression();
      }
      if (theType != null && ess != null && !ess.IsNull) ConvertToType(ess.ResultType, theType);
      E.DeclareLocal(theType, token, NestingLevel, InputList);
      if (ess != null) E.StoreToVar(token.Str, Visibility);
    }

    private static int FindEndOfStatement(LexList theList, bool failIfNotFound)
    {
      if (theList == null) return -1; 
      int a = theList.Index;
      while (true) {
        if (a >= theList.Count) return -1; 
        if (theList[a].Str == "") {
          if (failIfNotFound) {
            theList[a].ThrowException("Expected a ; or a } to mark the end of the statement here.", theList);
          } else {
            return -1;
          }
        }
        if (theList[a].Str == ";" || theList[a].Str == "}") {
          return a - 1;
        }
        a++;
      }
    }

    private static int FindEqualSignBeforeEndOfStatement(LexList theList, int end, out string operation )
    {
      operation = ""; 
      for (int a = theList.Index; a <= end; a++) {
        switch (theList[a].Str) {
        case "=": return a;
        case "+=": operation = "+"; return a;
        case "-=": operation = "-"; return a;
        case "*=": operation = "*"; return a;
        case "/=": operation = "/"; return a;
        }
      }
      return -1; 
    }

    private void DoDotIdAssignment(int positionOfDot , bool isImplicit)
    {
      ExpState exp ;
      if (isImplicit) {
        exp = new ExpState ( E.LoadFromVar("this", BindingFlags.Public)) ;
      } else {
        InputList.TemporaryEndAt(positionOfDot - 1);
        exp = DoExpression();
        InputList.TemporaryEndAt();
        InputList.CheckStrAndAdvance(".", "Expected a dot here.");
      }
      LexToken DotId = InputList.CurrentToken();
      InputList.GetIdentifier("Expected a field or property identifier here.");
      InputList.CheckStrAndAdvance("=", "Expected an assignment = here.");
      ExpState value = DoExpression();

      BindingFlags bf = Visibility;
      if (exp.AsStatic) bf |= BindingFlags.Static; else bf |= BindingFlags.Instance;
     
      int madeFieldIndex = -1 ;
      Type thisType = null; 
      if (This != null && This.ClassType == exp.ResultType && ((madeFieldIndex = This.GetMadeFieldIndex(DotId , out thisType )) != -1)) {
        ConvertToType(value.ResultType, thisType) ;
        StoreToMadeField(madeFieldIndex, thisType, false);
        return ; 
      }

      FieldInfo fi = exp.ResultType.GetField(DotId.Str, bf ) ; 
      if (fi != null) {
        ConvertToType(value.ResultType, fi.FieldType);
        if (exp.AsStatic) E.IL.Emit(OpCodes.Stsfld, fi); else E.IL.Emit(OpCodes.Stfld, fi);
        return;
      }

      PropertyInfo pi = exp.ResultType.GetProperty(DotId.Str, bf ) ; 
      if (pi != null) {
        MethodInfo mi = pi.GetSetMethod();
        if (mi == null) InputList.ThrowException("The property '" + pi.Name + "' does not have a public setter method.");
        ConvertToType(value.ResultType, pi.PropertyType);
        CallMethod(mi); 
        return;
      }

      DotId.ThrowException("The identifier '" + DotId.Str + "' is not a field or settable property in class '" + Parser.TypeToString(exp.ResultType) + "'.", InputList);
    }

    private void DoVarAssignment(string assignmentOperation) // assignmentOperation could be an empty string, or one of +-*/.
    {
      Type theType = null;
      bool isLocal = E.IsLocalOrArg(InputList.CurrentToken(), ref theType) ;
      bool isField = false; 
      int madeFieldIndex = -1 ; 
      if (!isLocal && This != null) {
        madeFieldIndex = This.GetMadeFieldIndex(InputList.CurrentToken(), out theType) ;
        isField = madeFieldIndex >= 0 ; 
      }
      if (isLocal || isField) {
        string theVarName = InputList.GetIdentifier();
        InputList.CheckStrAndAdvance(assignmentOperation + "=", "Unknown syntax here");
        if (assignmentOperation != "") {
          if (isLocal) {
            E.LoadFromVar(theVarName, Visibility);
          } else {
            LoadFromMadeField(madeFieldIndex, theType, true );
          }
        }
        ExpState state = DoExpression();
        ConvertToType(state.ResultType, theType);
        if (assignmentOperation != "") {
          if (state.ResultType == typeof(string)) {
            if (assignmentOperation != "+") InputList.ThrowException("Cannot do a " + assignmentOperation + "= assignment on string types.");
            CallStringConcat(); 
          } else {
            switch (assignmentOperation) {
            case "+": E.IL.Emit(OpCodes.Add); break;
            case "-": E.IL.Emit(OpCodes.Sub); break;
            case "*": E.IL.Emit(OpCodes.Mul); break;
            case "/": E.IL.Emit(OpCodes.Div); break;
            }
          }
        }
        if (isLocal) {
          E.StoreToVar(theVarName, Visibility);
        } else {
          StoreToMadeField(madeFieldIndex, theType, true ); 
        }
      } else {
        InputList.GetIdentifier("Expected the name of a local variable or a method variable here.");
      }
    }

    private void LoadFromMadeField(int madeFieldIndex, Type theType, bool loadThis)
    {
      if (loadThis) E.LoadFromVar("this", Visibility);
      FieldInfo fi = This.ClassType.GetField("Fields", BindingFlags.Public | BindingFlags.Instance) ;
      E.IL.Emit(OpCodes.Ldfld, fi );
      E.IL.Emit(OpCodes.Ldc_I4, madeFieldIndex);
      CallMethod ( fi.FieldType.GetMethod("get_Item", BindingFlags.Public | BindingFlags.Instance));
      if (theType != typeof(object)) {
        if (theType.IsValueType) {
          E.IL.Emit(OpCodes.Unbox_Any, theType);
        } else {
          E.IL.Emit(OpCodes.Castclass, theType);
        }
      }
    }

    private void StoreToMadeField(int madeFieldIndex, Type theType, bool loadThis )
    {
      LocalBuilder temp = E.DeclareAnonymousLocal(theType, InputList);
      E.IL.Emit(OpCodes.Stloc, temp); 
      if (loadThis) E.LoadFromVar("this", Visibility);
      FieldInfo fi = This.ClassType.GetField("Fields", BindingFlags.Public | BindingFlags.Instance);
      E.IL.Emit(OpCodes.Ldfld, fi);
      E.IL.Emit(OpCodes.Ldc_I4, madeFieldIndex);
      E.IL.Emit(OpCodes.Ldloc, temp);
      if (theType != typeof(object)) {
        if (theType.IsValueType) {
          E.IL.Emit(OpCodes.Box, theType );
        }
      }
      CallMethod ( fi.FieldType.GetMethod("set_Item", BindingFlags.Instance | BindingFlags.Public)); 
    }

    private void DoCallStatement()
    {
      ExpState state = DoExpression();
      if (InputList.Str != ";") InputList.ThrowException("Expected the end of the call statement here.");
      if (state != null) {
        // The method call left some result on the stack, so need to get rid of it.
        E.IL.Emit(OpCodes.Pop);
      }
    }

    // Expression               =  ExpOr RefTail                                                     .
    // ExpOr                    =  ExpAnd *( '||' ExpAnd )                                           .
    // ExpAnd                   =  ExpBitOr *( '&&' ExpBitOr )                                       .
    // ExpBitOr                 =  ExpBitXor *( '|' ExpBitXor )                                      .
    // ExpBitXor                =  ExpBitAnd *( '^' ExpBitAnd )                                      .
    // ExpBitAnd                =  ExpEqual *( '&' ExpEqual )                                        .
    // ExpEqual                 =  ExpCompare [ ( '!=' | '==' ) ExpCompare ]                         .
    // ExpCompare               =  ExpShift [ ( '<' | '>' | '<=' | '>=' ) ExpShift ]                 .
    // ExpShift                 =  ExpAdd [ ( '<<' | '>>' ) ExpAdd ]                                 .
    // ExpAdd                   =  ExpMul *( ( '+' | '-' ) ExpMul )                                  .
    // ExpMul                   =  ExpUnary *( ( '*' | '/' | '%' ) ExpUnary )                        .
    // ExpUnary                 =  *( ( '-' | '~' | '!' ) ) ExpValue                                 .
    // ExpValue                 =  [ TypeCast ] '(' Expression ')' | Constant | Constructor | Ref    .
    // TypeCast                 =  '(' Type ')'                                                      .
    // Constructor              =  'new' OneType FunctionPara                                        .
    // Constant                 =  ( Bool | String | Float | Double | Single | Int | Long | Enum )   .
    //                                                                                               .
    // Ref                      =   ( StaticClass '.' StaticMemberId | LocalVariable ) RefTail       .
    // RefTail                  =  *( '.' Id | ArrayPara | FunctionPara )                            .
    // FunctionPara             =  '(' Expression *( ',' Expression ) ')'                            .
    // ArrayPara                =  '[' Expression *( ',' Expression ) ']'                            .


    private ExpState DoExpOr() { return DoExpOrAndShortCircuit("||", OpCodes.Brtrue, DoExpAnd); }
    private ExpState DoExpAnd() { return DoExpOrAndShortCircuit("&&", OpCodes.Brfalse, DoExpBitOr); }

    private ExpState DoExpOrAndShortCircuit(string opName, OpCode opCode, Func<ExpState>doNext)
    {
      ExpState state = doNext();
      if (InputList.Str == opName) {
        Label finished = E.IL.DefineLabel();
        while (InputList.Str == opName) {
          E.IL.Emit(OpCodes.Dup); 
          E.IL.Emit(opCode, finished);
          E.IL.Emit(OpCodes.Pop); 
          InputList.Next();
          state.CheckTypeIs(InputList, typeof(bool), "Expected a bool operand for " + opName);
          state = doNext();
          state.CheckTypeIs(InputList, typeof(bool), "Expected a bool operand for " + opName);
          state = new ExpState(typeof(bool));
        }
        E.IL.MarkLabel(finished);
      }
      return state;
    }

    
    private const int Int_Int = 0;
    private const int Int_Long = 1;
    private const int Int_Float = 2;
    private const int Int_Double = 3;
    private const int Int_Char = 4;
 
    private const int Long_Int = 5;
    private const int Long_Long = 6;
    private const int Long_Float = 7;
    private const int Long_Double = 8;
    private const int Long_Char = 9;

    private const int Float_Int = 10;
    private const int Float_Long = 11;
    private const int Float_Float = 12;
    private const int Float_Double = 13;
    private const int Float_Char = 14; 

    private const int Double_Int = 15;
    private const int Double_Long = 16;
    private const int Double_Float = 17;
    private const int Double_Double = 18;
    private const int Double_Char = 19; 

    private const int Char_Int = 20;
    private const int Char_Long = 21;
    private const int Char_Float = 22;
    private const int Char_Double = 23;
    private const int Char_Char = 24; 

    private static int GetTypePair(Type initialType, Type finalType)
    {
      int result = 0;
      if (initialType == typeof(long)) {
        result += 1;
      } else if (initialType == typeof(float)) {
        result += 2;
      } else if (initialType == typeof(double)) {
        result += 3;
      } else if (initialType == typeof(int)) {
      } else if (initialType == typeof(char)) {
        result += 4; 
      } else {
        return -1;
      }
      result *= 5;
      if (finalType == typeof(long)) {
        result += 1;
      } else if (finalType == typeof(float)) {
        result += 2;
      } else if (finalType == typeof(double)) {
        result += 3;
      } else if (finalType == typeof(int)) {
      } else if (finalType == typeof(char)) {
        result += 4; 
      } else {
        return -1;
      }
      return result;
    }

    private ExpState DoExpBitOr() { return DoExpBitLogic("|", OpCodes.Or, DoExpBitXor); }
    private ExpState DoExpBitXor() { return DoExpBitLogic("^", OpCodes.Xor, DoExpBitAnd) ; }
    private ExpState DoExpBitAnd() { return DoExpBitLogic("&", OpCodes.And, DoExpEqual) ; }


    private ExpState DoExpBitLogic(string opName, OpCode opCode, Func<ExpState> doNext)
    {
      ExpState state = doNext();
      if (state == null) return null; 
      OperandSize os = new OperandSize(InputList, state, OperandSizes.Bool | OperandSizes.Integer , E ); 
      while (InputList.Str == opName) {
        InputList.Next();
        os.CheckBitArithmetical(state) ; 
        state = doNext();
        os.NextBitArithmetical(state); 
        E.IL.Emit(opCode); 
      }
      return state; 
    }

    private ExpState DoExpEqual()
    {
      ExpState state = DoExpCompare();
      if (state == null) return null; 
      OperandSize os = new OperandSize(InputList, state, OperandSizes.Integer | OperandSizes.Real, E);
      if (InputList.Str == "!=") {
        state = Comparison(state, OpCodes.Ceq, os);
        BooleanNot(); 
      } else if (InputList.Str == "==") {
        state = Comparison(state, OpCodes.Ceq,os);
      }
      return state;
    }

    private void BooleanNot()
    {
      E.IL.Emit(OpCodes.Ldc_I4_0);
      E.IL.Emit(OpCodes.Ceq);
    }

    private ExpState Comparison(ExpState state, OpCode op, OperandSize os)
    {
      InputList.Next();
      ExpState secondState = DoExpCompare();
      os.NextComparison(secondState); 
      if (op == OpCodes.Ceq) {
        state.CheckEqualityTypes(InputList, secondState.ResultType);
      } else {
        state.CheckComparisonTypes(InputList, secondState.ResultType);
      }
      state = new ExpState(typeof(bool));
      E.IL.Emit(op);
      return state; 
    }

    private ExpState DoExpCompare()
    {
      ExpState state = DoExpShift();
      if (state == null) return null; 
      OperandSize os = new OperandSize(InputList, state, OperandSizes.Integer | OperandSizes.Real , E);
      if (InputList.Str == "<") {
        state = Comparison(state, OpCodes.Clt, os); 
      } else if (InputList.Str == ">") {
        state = Comparison(state, OpCodes.Cgt, os);
      } else if (InputList.Str == "<=") {
        state = Comparison(state, OpCodes.Cgt, os);
        BooleanNot(); 
      } else if (InputList.Str == ">=") {
        state = Comparison(state, OpCodes.Clt, os);
        BooleanNot();
      }
      return state;
    }

    private ExpState DoExpShift()
    {
      ExpState state = DoExpAdd();
      if (InputList.Str == ">>") {
        DoExpShiftOperand(state, OpCodes.Shr);
      } else if (InputList.Str == "<<") {
        DoExpShiftOperand(state, OpCodes.Shl);
      }
      return state;
    }

    private void DoExpShiftOperand(ExpState state, OpCode op)
    {
      state.CheckIsIntType(InputList, "Expected left operand of the shift operator to be an integer type.");
      InputList.Next();
      ExpState secondState = DoExpAdd(); 
      secondState.CheckIsIntType ( InputList , "Expected right operand of the shift operator to be an integer type.");
      E.IL.Emit(op);
    }

    private ExpState DoExpAdd()
    {
      ExpState state = DoExpMul();
      if (state == null) return null; 
      OperandSize os = new OperandSize(InputList, state, OperandSizes.Real | OperandSizes.Integer, E);
      while (true) {
        if (InputList.Str == "+") {
          if (state.ResultType == typeof(string)) {
            InputList.Next(); 
            state = DoExpMul();
            if (state.ResultType == typeof(string)) {
              CallStringConcat();
            } else {
              ConvertValueToString(state);
              CallStringConcat();
              state = new ExpState(typeof(string)); 
            } 
          } else {
            state = DoExpAddOperand(state, OpCodes.Add, os);
          }
        } else if (InputList.Str == "-") {
          state = DoExpAddOperand(state, OpCodes.Sub, os);
        } else {
          break;
        }
      }
      return state;
    }

    private void CallStringConcat()
    {
      MethodInfo mi = typeof(string).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(string) }, null);
      E.IL.Emit(OpCodes.Call, mi);
    }

    private MethodInfo ConvertValueToString(ExpState state)
    {
      GetAddressOfValueTypeForDispatch(state); 
      MethodInfo mi = state.ResultType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null); 
      E.IL.Emit(OpCodes.Call, mi);
      return mi;
    }

    private ExpState DoExpAddOperand(ExpState state, OpCode op, OperandSize os)
    {
      os.CheckArithmetical(state);
      InputList.Next();
      ExpState secondState = DoExpMul();
      os.Next(secondState);
      E.IL.Emit(op); 
      return new ExpState(state.ResultType, secondState.ResultType);
    }

    private ExpState DoExpMul()
    {
      ExpState state = DoExpUnary();
      if (state == null) return null;
      OperandSize os = new OperandSize(InputList, state, OperandSizes.Real | OperandSizes.Integer, E);
      while (true) {
        if (InputList.Str == "*") {
          state = DoExpMulOperand(state, OpCodes.Mul,os);
        } else if (InputList.Str == "/") {
          state = DoExpMulOperand ( state, OpCodes.Div,os ) ; 
        } else if (InputList.Str == "%") {
          state = DoExpMulOperand ( state, OpCodes.Rem ,os) ; 
        } else {
          break;
        }
      }
      return state;
    }

    private ExpState DoExpMulOperand(ExpState state, OpCode op, OperandSize os)
    {
      os.CheckArithmetical(state); 
      InputList.Next();
      ExpState secondState = DoExpUnary();
      os.Next(secondState); 
      E.IL.Emit(op); 
      return new ExpState(state.ResultType, secondState.ResultType);
    }

    private ExpState DoExpUnary()
    {
      Stack<string> ops = new Stack<string>();
      while (true) {
        if (InputList.Str == "!") {
          ops.Push(InputList.Str);
          InputList.Next();
        } else if (InputList.Str == "~") {
          ops.Push(InputList.Str);
          InputList.Next();
        } else if (InputList.Str == "-") {
          ops.Push(InputList.Str);
          InputList.Next();
        } else {
          break;
        }
      }
      ExpState state = DoExpValue();
      while (ops.Count > 0) {
        switch (ops.Pop()) {
        case "!":
          state.CheckTypeIs(InputList, typeof(bool), "Expected bool operand to the ! operator" ) ;
          E.IL.Emit(OpCodes.Ldc_I4_0); 
          E.IL.Emit(OpCodes.Ceq); 
          break;
        case "-":
          state.CheckIsNumberType(InputList, "Expected operand to - to be a number.");
          E.IL.Emit(OpCodes.Neg); 
          break;
        case "~":
          state.CheckIsIntType(InputList, "Expected operand to ~ to be an int.");
          E.IL.Emit(OpCodes.Not); 
          break;
        }
      }
      state = DoRefTail(state, false); 
      return state;
    }

    private ExpState DoExpValue()
    {
      ExpState state = null;
      if (InputList.Str == "(") {
        if (IsThisATypeCast()) {
          InputList.Next();
          Type castingToType = Parser.ParseType(InputList, true, false, BindingFlags.Public | BindingFlags.Instance);
          InputList.CheckStrAndAdvance(")", "Expected closing ) after the typecast.");
          ExpState originalState = DoExpValue(); 
          state = ConvertToType(originalState.ResultType, castingToType);
        } else {
          InputList.Next();
          state = DoExpression();
          InputList.CheckStrAndAdvance(")", "Expected closing bracket here.");
        }
      } else if (InputList.Str == "new") {
        InputList.Next();
        Type newType = Parser.ParseType(InputList, true, true, Visibility);
        Type[] types;
        if (newType.IsArray) {
          if (newType.GetArrayRank() > 1) InputList.ThrowException("Only arrays of rank 1 are allowed.");
          types = LoadParametersAndReturnTypes("]");
          while ( InputList.Str == "[" ) {
            newType = newType.MakeArrayType() ;
            InputList.Next(); 
            InputList.CheckStrAndAdvance ( "]" , "Expected closing ] here." ) ; 
          }
          E.IL.Emit(OpCodes.Newarr, newType.GetElementType());
        } else {
          if (InputList.Str != "(") InputList.ThrowException("Expected bracketed parameter list for the constructor here.");
          types = ReadParametersAndReturnTypes_NoEmit(")");                 //ParameterConversion
          ConstructorInfo ci = newType.GetConstructor(types);
          if (ci == null) InputList.ThrowException("Unable to find constructor to match these parameters."); 
          LoadParameters(")", GetParameterTypes(ci.GetParameters()));
          E.IL.Emit(OpCodes.Newobj, ci);
        }
        state = new ExpState(newType);
      } else if (InputList.Str == "null") {
        InputList.Next();
        state = new ExpState();
        E.IL.Emit(OpCodes.Ldc_I4_0);
      } else if (InputList.Str == "++" || InputList.Str == "--") {
        state = DoRef();
      } else {
        switch (InputList.Kind) {
        case LexKind.Double:
          double d = InputList.Str.StrToDouble();
          E.IL.Emit(OpCodes.Ldc_R8, d);
          InputList.Next();
          state = new ExpState(typeof(double));
          break;
        case LexKind.Float:
          float f = InputList.Str.StrToFloat();
          E.IL.Emit(OpCodes.Ldc_R4, f);
          InputList.Next();
          state = new ExpState(typeof(float));
          break;
        case LexKind.Int:
          int i = InputList.Str.StrToInt();
          E.IL.Emit(OpCodes.Ldc_I4, i);
          InputList.Next();
          state = new ExpState(typeof(int));
          break;
        case LexKind.Long:
          long l = InputList.Str.StrToLong();
          E.IL.Emit(OpCodes.Ldc_I8, l);
          InputList.Next();
          state = new ExpState(typeof(long));
          break;
        case LexKind.Char:
          int ich = (int)InputList.Str[1];
          InputList.Next();
          E.IL.Emit(OpCodes.Ldc_I4, ich);
          state = new ExpState(typeof(char));
          break;
        case LexKind.String:
          string s = InputList.GetStringValue();
          E.IL.Emit(OpCodes.Ldstr, s);
          state = new ExpState(typeof(string));
          break;
        case LexKind.Bool:
          if (InputList.CurrentToken().ActualObject is bool) {
            if (((bool)(InputList.CurrentToken().ActualObject))) {
              E.IL.Emit(OpCodes.Ldc_I4_1);
            } else {
              E.IL.Emit(OpCodes.Ldc_I4_0);
            }
            InputList.Next();
            state = new ExpState(typeof(bool));
          }
          break;
        case LexKind.Identifier:
          if (InputList.Str == "true") {
            E.IL.Emit(OpCodes.Ldc_I4_1);
            InputList.Next();
            state = new ExpState(typeof(bool));
          } else if (InputList.Str == "false") {
            E.IL.Emit(OpCodes.Ldc_I4_0);
            InputList.Next();
            state = new ExpState(typeof(bool));
          } else {
            state = DoRef();
          }
          break;
        case LexKind.Type:
          state = DoRef();
          break;
        }

      }
      return state;
    }

    private Type[] GetParameterTypes(ParameterInfo[] parameterInfo)
    {
      Type[] paras = new Type[parameterInfo.Length];
      for (int i = 0; i < parameterInfo.Length; i++) {
        paras[i] = parameterInfo[i].ParameterType; 
      }
      return paras; 
    }

    private ExpState DoExpression()
    {
      ExpState state = DoExpCond();
      if (InputList.Str == "?") {
        LocalBuilder lb = E.DeclareAnonymousLocal(typeof(bool), InputList);
        E.IL.Emit(OpCodes.Stloc, lb);
        if (state.ResultType != typeof(bool)) InputList.ThrowException("Must be a boolean expression before the ? operator.");
        InputList.Next();
        ExpState state1 = DoExpression();
        if (InputList.Str != ":") InputList.ThrowException("Expected a : operator here (to match the previous ? operator).");
        InputList.Next();
        ExpState state2 = DoExpression();
        if (state1.ResultType != state2.ResultType) InputList.ThrowException("The first operand to the ? operator is of type '" + state1.ResultType.Name +
         "' while the second is for a different type '" + state2.ResultType.Name + "'.");
        if (E != null) {
          E.IL.Emit(OpCodes.Ldloc, lb);
          Label lab = E.IL.DefineLabel();
          Label lab2 = E.IL.DefineLabel();
          E.IL.Emit(OpCodes.Brtrue, lab);
          LocalBuilder temp = E.DeclareAnonymousLocal(state1.ResultType, InputList);
          E.IL.Emit(OpCodes.Stloc, temp);
          E.IL.Emit(OpCodes.Pop);
          E.IL.Emit(OpCodes.Ldloc, temp);
          E.IL.Emit(OpCodes.Br, lab2);
          E.IL.MarkLabel(lab);
          E.IL.Emit(OpCodes.Pop);
          E.IL.MarkLabel(lab2);
        }
        return state2;
      } else {
        return state;
      }
    }

    private ExpState DoExpCond()
    {
      ExpState state = DoExpOr();
      return state;
    }

    private ExpState ConvertToType(Type from, Type to)           
    {
      if (from == to) return new ExpState(to); 
      if (from == typeof(object)) {
        if (to.IsValueType) {
          E.IL.Emit(OpCodes.Unbox_Any, to);
        } else {
          E.IL.Emit(OpCodes.Castclass, to);
        }
      } else if (to == typeof(object)) {
        if (from.IsValueType) {
          E.IL.Emit(OpCodes.Box, from);
        } else {
          E.IL.Emit(OpCodes.Castclass, to);
        }
      } else if (from.IsEnum && Enum.GetUnderlyingType(from) == to) {
        // nothing more needed
      } else if (from == typeof(byte) && to == typeof(int) ) {
        E.IL.Emit(OpCodes.Conv_I4); 
      } else if (from == typeof(byte) && to == typeof(long)) {
        E.IL.Emit(OpCodes.Conv_I8); 
      } else if (from.IsValueType && to.IsValueType) {
        if (from.IsEnum) from = Enum.GetUnderlyingType(from); 
        
        switch (GetTypePair(from, to)) {
        case Char_Int : 
        case Char_Char: 
        case Int_Int:
        case Long_Long:
        case Float_Float:
        case Double_Double:
          break;
        case Char_Long: 
        case Float_Long:
        case Double_Long:
        case Int_Long:
          E.IL.Emit(OpCodes.Conv_I8); 
          break;
        case Long_Int:
        case Float_Int:
        case Double_Int:
          E.IL.Emit(OpCodes.Conv_I4);
          break;
        case Char_Double: 
        case Int_Double:
        case Long_Double:
        case Float_Double:
          E.IL.Emit(OpCodes.Conv_R8);
          break;
        case Char_Float : 
        case Int_Float:
        case Long_Float:
        case Double_Float:
          E.IL.Emit(OpCodes.Conv_R4); 
          break;
        default:
          InputList.ThrowException("Cannot convert between the types " + from.Name + " and " + to.Name + ".");
          break; 
        }
      } else if (!from.IsValueType && !to.IsValueType && from.IsSubclassOf(to)) {
        E.IL.Emit(OpCodes.Castclass, to);
      } else if (from.IsDerivedFrom(to) || to.IsDerivedFrom(from)) {
        E.IL.Emit(OpCodes.Castclass, to); 
      } else {
        InputList.ThrowException ( "Cannot cast from '" + Parser.TypeToString(from) + "' to '" + Parser.TypeToString(to) + "'." ) ; 
      }
      return new ExpState(to); 
    }

    private bool IsClassFieldOfMethod(ref ExpState theState, out bool implicitDot)
    {
      LexToken token;
      implicitDot = false ; 
      token = InputList.CurrentToken();
      if (token.Str == "this") {
        InputList.Next();
        InputList.CheckStrAndAdvance(".", "Expected a dot after the 'this' keyword.");
        InputList.CheckKind(LexKind.Identifier);
        token = InputList.CurrentToken();
        InputList.Prev();
        if (This == null) InputList.ThrowException("A 'this.' is not valid here.");
        if (This.IsMember(token)) {
          DoMemberOfClassThis(ref theState);
        } else if (This.IsCompiledMethod(token)) {
          DoCompiledClassMethod(token, ref theState);
        } else if (This.IsMadeField ( token )) {
          DoMemberOfClassThis(ref theState); 
        } else {
          InputList.ThrowException("Expected a field or a method after the 'this.'.");
        }
        return true; 
      } else if (InputList.Kind != LexKind.Identifier) {
        return false;
      } else if (This != null && This.IsMember(token)) {
        DoMemberOfClassThis( ref theState);
        implicitDot = true ; 
        return true; 
      } else if (This != null && This.IsCompiledMethod(token)) {
        InputList.Next();
        DoCompiledClassMethod(token, ref theState);
        return true;
      } else if (This != null && This.IsMadeField(token)) {
        DoMemberOfClassThis(ref theState);
        implicitDot = true; 
        return true;
      }
      return false;
    }

    /// <summary>
    /// Calling one of our own compiled methods. 
    /// </summary>
    private void DoCompiledClassMethod(LexToken methodName, ref ExpState newState)
    {
      Type methodDelegate;
      Type methodReturnType; 
      int methodIndex = This.GetMethodIndex(methodName, out methodDelegate, out methodReturnType );
      LoadLocalVariable("methodTable");
      E.IL.Emit(OpCodes.Ldc_I4, methodIndex);

      CallMethod ( typeof(List<Delegate>).GetMethod("get_Item", BindingFlags.Public | BindingFlags.Instance));

      E.IL.Emit(OpCodes.Castclass, methodDelegate); 
      newState = CallDelegate(new ExpState(methodDelegate), true); 
    }

    private Type GetTypeOfLocalVariable(string name)
    {
      LexToken nameToken = new LexToken(name, LexKind.Identifier, InputList);
      Type type = null;
      if (!E.IsLocalOrArg(nameToken, ref type)) InputList.Prev().ThrowException("ProgramError, '" + name + "' not found");
      return type;
    }

    private Type LoadLocalVariable(string name)
    {
      LexToken nameToken = new LexToken(name, LexKind.Identifier, InputList);
      Type type = null;
      if (!E.IsLocalOrArg(nameToken, ref type)) InputList.Prev().ThrowException("ProgramError, '" + name + "' not found");
      E.LoadFromVar(nameToken.Str, Visibility);
      return type;
    }

    /// <summary>
    /// Accessing a member of the class instance which our compiled method is extending.
    /// </summary>
    private void DoMemberOfClassThis(ref ExpState newState )
    {
      Type type = LoadLocalVariable("this"); 
      newState = new ExpState(type); 
    }

    enum IncKind { None, Inc, Dec }

    private IncKind GetIncKind()
    {
      if (InputList.Str == "++") {
        InputList.Next();
        return IncKind.Inc;
      } else if (InputList.Str == "--") {
        InputList.Next();
        return IncKind.Dec;
      } else {
        return IncKind.None;
      }
    }

    private void IncrementingStatement()
    {
      Type type = null ; 
      string tok ; 
      IncKind incKind = GetIncKind();
      if (incKind != IncKind.None) {
        if (!E.IsLocalOrArg(InputList.CurrentToken(), ref type)) InputList.ThrowException ( "Only a local variable or parameter can be used with ++ or --." ) ;
        tok = InputList.Str;
        InputList.Next();
      } else {
        if (!E.IsLocalOrArg(InputList.CurrentToken(), ref type)) InputList.ThrowException ( "Only a local variable or parameter can be used with ++ or --." ) ;
        tok = InputList.Str;
        InputList.Next(); 
        incKind = GetIncKind(); 
      }
      Incrementer(incKind, tok, type); 
    }

    private void Incrementer(IncKind incKind, string varName, Type type)
    {
      if (incKind == IncKind.None) return;
      int value = 1;
      if (incKind == IncKind.Dec) value = -1; 

      E.LoadFromVar(varName, Visibility);
      if (type == typeof(int) ) {
        E.IL.Emit (OpCodes.Ldc_I4,value) ;
      } else if ( type == typeof(long) ) {
        E.IL.Emit(OpCodes.Ldc_I8,value ) ; 
      } else if ( type == typeof(float) ) {
        E.IL.Emit(OpCodes.Ldc_R4,value) ; 
      } else if ( type == typeof(double)) {
        E.IL.Emit(OpCodes.Ldc_R8, value); 
      } else {
        InputList.ThrowException("A ++ or a -- can only be used on a numeric variable of type int,long,float,double.");
      }
      E.IL.Emit(OpCodes.Add ) ; 
      E.StoreToVar(varName , Visibility ) ; 
    }

    private ExpState DoRef()
    {
      string name = "";
      bool implicitDot = false;
      ExpState newState = null;

      IncKind incKind = GetIncKind();  

      Type type = null; 
      int madeFieldIndex = -1 ; 
      if (E.IsLocalOrArg(InputList.CurrentToken(), ref type)) {
        InputList.GetToken();
        Incrementer(incKind, InputList.LookAtToken(-1).Str, type); 
        E.LoadFromVar(InputList.LookAtToken(-1).Str, Visibility);
        if (incKind == IncKind.None) {
          incKind = GetIncKind(); 
          Incrementer(incKind, InputList.LookAtToken(-2).Str, type);
        }
        newState = new ExpState(type); 
      } else if (This != null && ((madeFieldIndex = This.GetMadeFieldIndex(InputList.CurrentToken() , out type )) != -1)) {
        if (incKind != IncKind.None) InputList.ThrowException("Sorry, ++ or -- only implemented for local variables and method parameters.");
        LoadFromMadeField(madeFieldIndex, type, true);
        InputList.Next();
        newState = new ExpState(type);
        implicitDot = false; 
      } else if (IsClassFieldOfMethod(ref newState, out implicitDot)) {
        if (incKind != IncKind.None) InputList.ThrowException("Sorry, ++ or -- only implemented for local variables and method parameters.");
 
      } else {
        if (incKind != IncKind.None) InputList.ThrowException("Sorry, ++ or -- only implemented for local variables and method parameters.");
        Type staticClass = Parser.ParseType(InputList, false, false, Visibility);
        if (staticClass != null) {
          if (InputList.AtEnd()) {
            return new ExpState(staticClass, true);
          }
          ExpState state = Parser.ParseMember(staticClass, InputList, Visibility | BindingFlags.Static, false);
          if (state != null && state.Field != null) {
            newState = new ExpState(state.ResultType);
            if (state.Field.IsLiteral && state.Field.FieldType.IsEnum) {
              Type underlyingType = Enum.GetUnderlyingType(state.Field.FieldType);
              if (underlyingType == typeof(int)) {
                E.IL.Emit(OpCodes.Ldc_I4, (int)(state.Field.GetValue(null)));
              } else if (underlyingType == typeof(long)) {
                E.IL.Emit(OpCodes.Ldc_I8, (long)(state.Field.GetValue(null)));
              }
            } else {
              E.IL.Emit(OpCodes.Ldsfld, state.Field);
            }
          } else {
            newState = DoMethodOrProperty(newState, new ExpState(staticClass), BindingFlags.Static);
          }
        } else {
          if (incKind != IncKind.None) InputList.ThrowException("Did not expect a ++ or -- here." ) ; 
          name = InputList.Str;
          if (name == "typeof") {
            LexToken token = InputList.CurrentToken();
            InputList.Next();
            InputList.CheckStrAndAdvance("(", "Expected a '(' bracket here.");
            type = GetTypeSpecifier(false);
            InputList.CheckStrAndAdvance(")", "Expected a ')' bracket here.");
            E.IL.Emit(OpCodes.Ldtoken, type);
            Type theType = typeof(Type);
            MethodInfo mi = theType.GetMethod("GetTypeFromHandle");
            if (mi == null) token.ThrowException("Program error trying to get 'GetTypeFromHandle'.", InputList);
            E.IL.Emit(OpCodes.Call, mi);
            type = typeof(Type);
          } else {
            InputList.ThrowException("Expected class type or local variable or typeof here.");
          }
          newState = new ExpState(type);
        }
      }
      return DoRefTail(newState, implicitDot);
    }

    // Does a static or instance method or property.
    private ExpState DoMethodOrProperty(ExpState newState, ExpState previousState, BindingFlags flags)
    {
      string description = (flags == BindingFlags.Static) ? "static" : "instance"; 
      MethodInfo mi = null;
      bool isExtension = false; 
      LexList errorReporting = InputList.Clone(); 
      string name = InputList.GetIdentifier("Expected the name of a " + description + " field, property or method here.");
      Type[] parameterTypes = null;
      if (InputList.Str == "(") {
        parameterTypes = ReadParametersAndReturnTypes_NoEmit(")");   //ParameterConversion
        mi = previousState.ResultType.GetMethod(name, Visibility | flags, null, parameterTypes, null);

        isExtension = true; 
        if (mi == null) mi = Parser.GetExtensionMethod(name, previousState.ResultType, parameterTypes, errorReporting);
        if (mi == null) InputList.Prev().ThrowException("The name '" + name + "' not recognised as a " + description + " field, property or method of '" + previousState.ResultType.Name + "'.");
        if (mi.IsGenericMethod) InputList.Prev().ThrowException("Method '" + name + " is generic, which is not allowed.");
        LoadParameters(")", GetParameterTypes(mi.GetParameters())); 
      } else {
        parameterTypes = new Type[0];
        if (InputList.CurrentToken().Str == "[") parameterTypes = LoadParametersAndReturnTypes("]" );
        PropertyInfo pi = previousState.ResultType.GetProperty(name, Visibility | flags, null, null, parameterTypes, null);
        if (pi == null) InputList.Prev().ThrowException("The " + description + " name '" + name + "' in class '" + Parser.TypeToString(previousState.ResultType) + "' was not found.");
        mi = pi.GetGetMethod();
        if (mi == null) InputList.Prev().ThrowException("The property '" + name + "' does not have a Get method.");
      }
      CallMethod(mi); 
      if (mi.ReturnType == null || mi.ReturnType == typeof(void)) newState = null; else newState = new ExpState(mi.ReturnType);
      return newState;
    }

    delegate void testName ( int i, float j) ;

    private ExpState DoRefTail(ExpState state, bool implicitDot)
    {
      if (state == null) return null;
      if (implicitDot) {
        state = DoRefTailAfterDot(state, true);
      }
      while (true) {
        switch (InputList.Str) {
        case ".":
          state = DoRefTailAfterDot(state, false);
          break;
        case "(":
          ExpState newState = CallDelegate(state, false);
          state = newState;
          break;
        case "[":
          MethodInfo unusedMi;
          state = DoArrayReference(state, true, out unusedMi);
          break;
        default:
          goto breakWhile;
        }
        if (state == null) goto breakWhile;
      }

    breakWhile:
      if (InputList.Str == "++" || InputList.Str == "--") InputList.ThrowException("Sorry, ++ and -- only implemented for local vars or method arguments."); 
      return state;
    }

    private ExpState DoRefTailAfterDot(ExpState state, bool implicitDot )
    {
      ExpState previousState = state;

      Type type = null ;
      int madeFieldIndex = -1; 
      if (This != null && (InputList.Str == "." ) && (madeFieldIndex = This.GetMadeFieldIndex(InputList.LookAtToken(1), out type)) != -1) {
        LoadFromMadeField(madeFieldIndex, type, false );
        InputList.Next();
        ExpState newState = new ExpState(type);
        InputList.Next(); 
        return newState;
      }

      state = Parser.ParseMember(state.ResultType, InputList, Visibility | BindingFlags.Instance, implicitDot );
      if (state != null) {
        if (state.Field != null) {
          E.IL.Emit(OpCodes.Ldfld, state.Field);
          state = new ExpState(state.Field.FieldType);
        } else {
          InputList.ThrowException("Unknown syntax here.");
        }
      } else {
        if (InputList.Str == "GetType" && previousState.ResultType.IsValueType ) { 
          // Special case for GetType
          E.IL.Emit(OpCodes.Box, previousState.ResultType); 
        } else {
          GetAddressOfValueTypeForDispatch(previousState);
        }
        state = DoMethodOrProperty(state, previousState, BindingFlags.Instance);
      }
      return state;
    }

    private ExpState CallDelegate(ExpState state, bool addTwoExtraParameters)
    {
      LexToken tok = InputList.CurrentToken();
      Type[] paraTypes;
      int adjustCount = 0; 
      if (addTwoExtraParameters) {
        adjustCount = -2; 
        Type[] extras = new Type[] { GetTypeOfLocalVariable("this"), GetTypeOfLocalVariable("methodTable") };
        paraTypes = extras.Concat(ReadParametersAndReturnTypes_NoEmit(")")).ToArray();   //ParameterConversion
      } else {
        paraTypes = ReadParametersAndReturnTypes_NoEmit(")");
      }

      Type[] delegateTypes;
      Type delegateReturnType;
      MethodInfo invokeMethod = TypeParser.GetDelegateInfo(InputList, state.ResultType, out delegateReturnType, out delegateTypes);
      if (delegateTypes.Length != paraTypes.Length) tok.ThrowException("Expected " + (delegateTypes.Length+adjustCount) + 
        " delegate parameters, but actually have " + (paraTypes.Length+adjustCount) + ".", InputList );
      for (int i = 0; i < delegateTypes.Length; i++) {
        if (delegateTypes[i].IsSubclassOf(paraTypes[i])) tok.ThrowException(
          "Parameter " + i + " is " + Parser.TypeToString(paraTypes[i]) + " which is not derived from " + Parser.TypeToString(delegateTypes[i]) + ".", InputList);
      }
      if (addTwoExtraParameters) {
        LoadLocalVariable("this");
        LoadLocalVariable("methodTable");
      }
      LoadParameters(")", delegateTypes.GetSubArray ( 2 ) ); 
      ExpState newState; 
      if (delegateReturnType == typeof(void)) newState = null; else newState = new ExpState(delegateReturnType);
      CallMethod(invokeMethod); 
      return newState;
    }

    private void GetAddressOfValueTypeForDispatch(ExpState previousState)
    {
      if (previousState.ResultType.IsValueType) {
        if (previousState.ResultType.IsEnum) {
          E.IL.Emit(OpCodes.Box, previousState.ResultType); 
        } else {
          LocalBuilder lb = E.DeclareAnonymousLocal(previousState.ResultType, InputList);
          E.IL.Emit(OpCodes.Stloc, lb);
          E.IL.Emit(OpCodes.Ldloca, lb);
        }
      }
    }

    private void DoArrayElementAssignment(int openSquarePosition, bool implicitThis)
    {
      ExpState exp ;
        InputList.TemporaryEndAt(openSquarePosition - 1);
        exp = DoExpression();
        InputList.TemporaryEndAt();
      MethodInfo mi; 
      ExpState arrayElement = DoArrayReference(exp, false, out mi);
      InputList.CheckStrAndAdvance("=", "Expected assignment equal here.");
      ExpState valueExp = DoExpression();
      ConvertToType(valueExp.ResultType, arrayElement.ResultType);
      if (mi == null) {
        E.IL.Emit(OpCodes.Stelem, arrayElement.ResultType);
      } else { 
        CallMethod ( mi ) ; 
      }
    }

    private ExpState DoArrayReference(ExpState state, bool isRead , out MethodInfo mi)
    {
      mi = null;
      Type[] arrayParaTypes = LoadParametersAndReturnTypes("]" );

      if (arrayParaTypes.Length != 1) InputList.ThrowException("Expected only one array index here, followed by a ]");

      if (state.ResultType != null && state.ResultType.IsArray) {
        // Integer indexing of an array.
        if (arrayParaTypes[0] != typeof(int)) InputList.ThrowException("Expected integer type index expression.");
        state.CheckIsArrayType(InputList, "Expected an array type here.");
        state = new ExpState(state.ResultType.GetElementType());
        if (isRead) E.IL.Emit(OpCodes.Ldelem, state.ResultType);
      } else {
        // Is an array property.

        ParameterInfo para = null;
        ParameterInfo otherPara = null ; 

        PropertyInfo[] pii = state.ResultType.GetProperties(Visibility | BindingFlags.Instance);
        foreach (var p in pii) {
          MethodInfo mii ;
          if (isRead) mii = p.GetGetMethod(false); else mii = p.GetSetMethod(false);
          if (mii != null) {
            ParameterInfo[] pas = mii.GetParameters();
            if ((pas.Length == 1 && isRead) || (pas.Length == 2 && !isRead)) {
              mi = mii;
              para = pas[0];
              if (!isRead) otherPara = pas[1] ; 
              break;
            }
          }
        }

        if (mi == null) InputList.ThrowException("Expected an array index property here.");
        if (para.ParameterType != arrayParaTypes[0]) InputList.ThrowException("Expected array index parameter type to be " + para.ParameterType.ToString());
        if (isRead) {
          CallMethod(mi); 
        }
        if (isRead) {
          state = new ExpState(mi.ReturnType);
        } else {
          state = new ExpState(otherPara.ParameterType); 

        }
      }
      return state;
    }

    /// <summary>
    /// This will read the parameters, work out their types and return them. 
    /// If emission of code is active, then this will NOT emit any code, and will reset the input cursor back to the start of the parameters. 
    /// If emission of code is not active, then this will still not emit any code, but it will not put the cursor back.
    /// It must always be called in front of a call to LoadParameters. LoadParameters will actually emit the code if code emission is active. 
    /// </summary>
    private Type[] ReadParametersAndReturnTypes_NoEmit(string closeBracket)
    {
      Type unused = null; 
      if (E.IL.Active) {
        E.IL.Active = false;
        int save = InputList.Index;
        Type[] paras = ProcessParameters_ConvertIfNeeded_ReturnTypes(closeBracket, null);
        E.IL.Active = true; 
        InputList.Index = save; 
        return paras; 
      } else {
        return ProcessParameters_ConvertIfNeeded_ReturnTypes(closeBracket, null);
      }
    }

    /// <summary>
    /// This will Re-read parameters that have already been read by a call to ReadParametersAndReturnTypes_NoEmit. 
    /// It code emission is active it will emit code to load them, and convert them as necessary
    /// It must always be called after a call to ReadParametersAndReturnTypes_NoEmit.
    /// </summary>
    private void LoadParameters(string closeBracket, Type[] wantedTypes )
    {
      Type unused = null; 
      if (E.IL.Active) ProcessParameters_ConvertIfNeeded_ReturnTypes(closeBracket, wantedTypes );
    }

    /// <summary>
    /// This will Read the parameters and emit code to load them. 
    /// </summary>
    private Type[] LoadParametersAndReturnTypes(string closeBracket)
    {
      return ProcessParameters_ConvertIfNeeded_ReturnTypes(closeBracket, null ); 
    }

    /// <summary>
    /// This does the actual parameter reading and conversion and processing. It is only meant to be called from the 
    /// ReadParametersAndReturnTypes_NoEmit, LoadParameters and LoadParametersAndReturnTypes methods. 
    /// </summary>
    private Type[] ProcessParameters_ConvertIfNeeded_ReturnTypes(string closeBracket, Type[] wantedTypes )
    {
      List<Type> paraTypes = new List<Type>();
      InputList.Next();
      while (InputList.Str != closeBracket) {
        Type aParaType = DoExpression().ResultType;
        if (aParaType == null) aParaType = typeof(object);

        int top = paraTypes.Count;
        if (wantedTypes != null && wantedTypes[top] != aParaType) {
          ConvertToType(aParaType, wantedTypes[top]);
          aParaType = wantedTypes[top];
        }

        paraTypes.Add(aParaType);
        if (InputList.Str == closeBracket) break;
        InputList.CheckStrAndAdvance(",", "Expected either a , or a " + closeBracket + " here.");
      }
      InputList.Next();
      return paraTypes.ToArray();
    }

    private Type GetTypeSpecifier(bool closeAngleRequired)
    {
      bool pendingCloseAngle = false;
      Type theType = Parser.ParseType(InputList, true, false, out pendingCloseAngle, Visibility );
      if (closeAngleRequired) {
        if (!pendingCloseAngle) InputList.CheckStrAndAdvance ( ">" , "Expected closing > here." ) ; 
      } else {
        if (pendingCloseAngle) InputList.ThrowException ( "Did not expect a '>' here." ) ; 
      }
      return theType ; 
    }
  }
}
