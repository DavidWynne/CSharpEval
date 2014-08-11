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

/* To run this test program, set the conditional symbols 
 *   Class;Dialog;Repl;Test
 * into the Build/Conditional Compilation Symbols edit box for this project.
 *
 * To demonstrate the error dialog box, put in some syntax error in the 'partial class TestClass' code.
 */ 

#if Test && Class && Dialog && Repl

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections;

namespace Kamimu
{

  public enum TestEnum { One, Two, Three }

  public class TestClassBase
  {
    public char BaseChar = 'X';
    public static int ExampleForEach()
    {
      HashSet<int> test = new HashSet<int>();
      test.Add(1); test.Add(2);
      int j = 0; 
      foreach (var i in test) {
        j += i; 
      }
      return j; 
    }
    public static string TestDiv()
    {
      int i = 33;
      double k = 4.5; 
      return (i/k).ToString() ;
    }

    public static Type ExampleTypeOf()
    {
      return typeof(int); 
    }
    public static string TestStringBuilder()
    {
      StringBuilder temp = new StringBuilder();
      temp.Append("hello");
      return temp.ToString(); 
    }

    public static void TestCall(TestClassBase tc, Delegate[] delegates)
    {
      Action<TestClassBase, Delegate[]> action = (Action<TestClassBase, Delegate[]>)delegates[3];
      action(tc, delegates); 

    }
    public static Type TestGetType()
    {
      int i = 2;
      string s = i.ToString(); 
      return  i.GetType();
    }

    public static double Test () 
    { 
      return "23.45".StrToDouble() ; 
    }
  }

  public class TestClassExtension
  {
    public int TestClassInt = 6677;
    public float TestClassFloat = 66.77F;
    public string TestClassString = "TestClassExt";
    public List<Object> Fields = new List<object>(); 


    public static string GetTheStringFn(TestClassExtension TheThis, Delegate[] TheMethodTable )
    {
      return ((Func<TestClassExtension,Delegate[],string>)(TheMethodTable[0]))(TheThis,TheMethodTable); 
    }

    public int TestField(object methodTable, int i)
    {
      Fields[0] = (object)i;
      return (int)Fields[0]; 
    }

    public void SetArray(int[] input)
    {
      Fields[0] = (object)input;
    }
    public int Get(int i)
    {
      return ((int[])(Fields[0]))[i];
    }
    public void Set(int i, int v)
    {
      ((int[])(Fields[0]))[i] = v;
    }

  
  }

  public class TestInt
  {
    public int IntValue;
    public TestInt(int i) { IntValue = i; }
  }


  public class TestClass : TestClassBase 
  {
    public List<Object> Fields; 
    public List<TestClass> TestList = new List<TestClass>() ; 

    static public string FloatToString ( float f ) { return f.ToString() ; }
    static public string DoubleToString ( double f ) { return f.ToString() ; }
    static public string IntToString ( int f ) { return f.ToString() ; }
    static public string LongToString ( long f ) { return f.ToString() ; }

    static public int TestStaticInt = -1;
    public void SetTestStaticInt(int i) { TestStaticInt = i; }
    public static void StaticSetTestStaticInt(int i) { TestStaticInt = i; }

    static public int HowMany = 0; 
    static public int TestClassStaticInt = 34 ;

    static public int ForPropertyTest = -1 ;
    static public int TestClassStaticPropertyInt { get { return ForPropertyTest; } set { ForPropertyTest = value; } }

    static public int[] TestStaticIntArray = new int[10];

    public TestClass() { }
    public TestClass(int k, int j) { TestClassInt = k; Index = j; }

    public int TestClassInt = 64;
    public int Index = -1; 
    public string this[string s] { get { return s + "s"; } }
    public string TheDepth { get { return "Depth = " + Depth; } } 
    public TestClass NestedTestClass;
    public TestClass[] TestClassArray = new TestClass[4]; 
    public int Depth;
    public TestClass GetTestClassArray(int which) { return TestClassArray[which]; }
    public TestClass FetchNestedTestClass { get { return NestedTestClass; } }
    public int[] TestIntArray = new int[10]; 
    public string Id = ""; 
    public TestClass(string id) { Id = id; }
    public string Info { get { return "Info " + Index; } }
    public int PropertyValue; 
    public int TestProperty { get { return PropertyValue; } set { PropertyValue = value; } }
    public string GenericInfo<T>(T t)
    {
      return "Generic " + t.ToString() + " is a " + typeof(T).Name;
    }
    public string GenericInfoThree<T, S, Q>(T t, Q q, S s, int i)
    {
      return "Generic " +
        t.ToString() + " is a " + typeof(T).Name + " and " +
        q.ToString() + " is a " + typeof(Q).Name + " and " +
        s.ToString() + " is a " + typeof(S).Name + " and " +
        "i = " + i.ToString();
    }
    public string GenericInfoThreeA<T, S, Q>(T t, S s, Q q, int i)
    {
      return "Generic " +
        t.ToString() + " is a " + typeof(T).Name + " and " +
        q.ToString() + " is a " + typeof(Q).Name + " and " +
        s.ToString() + " is a " + typeof(S).Name + " and " +
        "i = " + i.ToString();
    }
    public TestClass(int depth)
    {
      Index = HowMany; 
      HowMany++;
      Depth = depth;
      if (depth > 0) {
        for (int i = 0; i < TestClassArray.Length; i++) TestClassArray[i] = new TestClass(depth - 1);
        NestedTestClass = new TestClass(depth-1) { Depth = depth - 1 };
      }
    }
    static public TestClass StaticTestClass = new TestClass (7);  
  }

  public struct TestStruct
  {
    public int TestStructInt;
    public TestStruct(int i) { TestStructInt = i; }
    public static int TestStructStaticInt = 1111 ; 
  }

  public class TestClass2
  {
    static public int TestClass2StaticInt = 34;
    public int TestClass2Int = 64;
    public static Func<int> Fn = () => 256;
    public static Func<int> GetFn() { return Fn; }
    public static Func<Func<int>> GetGetFn() { return () => GetFn(); }
    public static Func<Func<Func<int>>> GetGetGetFn() { return () => GetGetFn(); }
    public static string TestForEachChar()
    {
      string s = "";
      var e = "123456789".GetEnumerator();
      char ch;
      try {
        goto L0;
      L1:
        ch = e.Current;
        s += ch.ToString(); 
      L0:
        if (e.MoveNext()) goto L1;
      } finally {
      }
      return s; 
    }

    public static string TestForEachList()
    {
      List<int> list = new List<int>();
      for (var j = 100; j > 0; j = j - 2) {
        list.Add(j);
      }
      string result = "";
      foreach (var s in list) {
        result = result + ' ' + s.ToString();
      }
      return result;
    }

    public static int TestCharAdd()
    {
      char ch = '1';
      int num = 100 + ch - '0';
      return num;
    }

    public static long fn()
    {
      char ch = '1';
      long num = 10;
      num = num * 10 + ch - '0';
      return num;
    }

  }

  public class TestProgram
  {

    static StringBuilder MakeMethodSB = new StringBuilder();

    static void Test(bool flag, int testNum)
    {
      if (!flag) MakeMethodSB.AppendLine("[test " + testNum + "] wrong result.");
    }

    static long Test(int a, int b, long c) { return a ^ b ^ c; }

    static int FnOne(int i) { return i; }  
    static int FnTwo ( int i ) { return 2*i ; }

    static public string TestIntToStr(int i)
    {
      TestClass[] anArray = new TestClass[4];
      anArray[2].TestClassInt  = i ;
      return (anArray[3].TestClassInt + anArray[2].TestClassInt).ToString(); 
    }


    static LexList LexListGet(string s)
    {
      LexListBuilder lb = new LexListBuilder();
      lb.AddAndPromoteQuotes(s);
      return lb.ToLexList(); 
    }

    static bool CompareIntArrays(int[] a1, int[] a2)
    {
      if (a1.Length != a2.Length) return false;
      for (int i = 0; i < a1.Length; i++) {
        if (a1[i] != a2[i]) return false;
      }
      return true;
    }

    static bool CompareTestClassArrays(TestClass[] a1, TestClass[] a2)
    {
      if (a1.Length != a2.Length) return false;
      for (int i = 0; i < a1.Length; i++) {
        if (a1[i].TestClassInt != a2[i].TestClassInt) return false;
      }
      return true;
    }

    static int[] MakeTestIntArray(int howMany)
    {
      int[] temp = new int[howMany];
      for (int i = 0; i < howMany; i = i + 1) {
        temp[i] = i * 2;
      }
      return temp;
    }

    static TestClass[] MakeTestClassArray(int howMany)
    {
      TestClass[] temp = new TestClass[howMany];
      for (int i = 0; i < howMany; i = i + 1) {
        temp[i] = new TestClass(); 
        temp[i].TestClassInt = i * 3;
      }
      return temp;
    }

    private static void TestType(TypeParser parser, string s, Type expectedType, int testNum)
    {
      bool pendingCloseAngle;
      Type foundType = null; 
      try {
        foundType = parser.Parse(LexListGet(s), true, false, out pendingCloseAngle, BindingFlags.Public);
      } catch (Exception ex) {
        MakeMethodSB.AppendLine("[Test " + testNum + "] " + ex.Message );
        return; 
      }
      if (foundType != expectedType) {
        MakeMethodSB.AppendLine("[Test " + testNum+ "] For '" + s + "' expected \n" + parser.TypeToString(expectedType) + " but got " + (foundType == null ? "\nNULL" : ("\n" + parser.TypeToString(foundType))));
      }

    }

    // Unimplemented:
    //  out and ref parameters (in the method header)   
    //  out and ref parameters (inside the method)    
    //  all of Linq
    //  ++ and -- operators are limited to local vars and arguments. No brackets allowed, as in (i)++.
    //  +=, -=, etc assignments are limited to local vars and arguments only.
    //  assignments inside an expression
    //  do { } while loop
    //  yield return
    //  overloading resolution is simplified
    //  Can use generics inside a method, but cannot define a generic method (this is actually a .NET limitation).
    //  multidimensional arrays
    //  operator overloading
    //  byte, sbyte, short, ushort, uint, ulong in arithmetical expressions... 
    //  does not detect readonly when compiling an assignment to a field 
    //  unsafe code. 
    //  checked and unchecked. lock. 
    //  Exception handling of any kind.
    //  gotos and labels. 
    //  Switch statements.
    //  Lambdas

    public static void MakeMethodTest(TypeParser parser, int testNum)
    {
      int startTestNum = testNum;
      Stopwatch sw = new Stopwatch();
      sw.Start();

      // test jagged arrays.

      {
        LexListBuilder lb = new LexListBuilder(); 
        lb.Add (@"
          public int fn ( TestClass test ) 
          {
            List<`TestClass> list =  test.TestList ; 
            return list.Count ; 
          }", "TestClass" , typeof(TestClass) ) ;
        Func<TestClass, int> fn = MakeMethod<Func<TestClass, int>>.Compile(parser, lb.ToLexList());
        TestClass tc = new TestClass();
        Test(tc.TestList.Count == fn(tc), testNum++);
      }

      {
        Func<TestClass, int> fn = MakeMethod<Func<TestClass, int>>.Compile(parser, LexListGet(@"
          public int fn ( TestClass test ) 
          {
            int ix = 3 ; 
            {
              List<TestClass> list =  test.TestList ; 
              ix = list.Count ; 
            }
            return ix ; 
          }"));
        TestClass tc = new TestClass();
        Test(tc.TestList.Count == fn(tc), testNum++);
      }

      {
        Func<TestClass, int> fn = MakeMethod<Func<TestClass, int>>.Compile(parser, LexListGet(@"
          public int fn ( TestClass test ) 
          {
            List<TestClass> list =  test.TestList ; 
            return list.Count ; 
          }"));
        TestClass tc = new TestClass();
        Test(tc.TestList.Count == fn(tc), testNum++);
      }

      {
        Func<int, int, int[][]> fn = MakeMethod<Func<int, int, int[][]>>.Compile(parser, LexListGet(@"
          public int[][] fn ( int width , int height )  
          {
            int[][] testArray = new int[width][];
            for (int i = 0; i < width; i++) {
              testArray[i] = new int[height];
              for (int j = 0; j < testArray[i].Length; j++) {
                testArray[i][j] = i * 10 + j;
              }
            }
            return testArray ; 
          }"));
        int[][] testArray = fn(4, 5);
        for (int i = 0; i < testArray.Length; i++) {
          for (int j = 0; j < testArray[i].Length; j++) {
            Test(testArray[i][j] == i * 10 + j, testNum++);
          }
        }

      }

      {
        Func<int[][], int> fn = MakeMethod<Func<int[][], int>>.Compile(parser, LexListGet(@"
          public int fn ( int[][] theArray ) 
          {
            theArray[3][4] = 3344 ; 
            return theArray[2][3] ; 
          }"));
        int[][] testArray = new int[10][];
        for (int i = 0; i < testArray.Length; i++) {
          testArray[i] = new int[10];
          for (int j = 0; j < testArray[i].Length; j++) {
            testArray[i][j] = i * 10 + j;
          }
        }
        Test(fn(testArray) == 23, testNum++);
        Test(testArray[3][4] == 3344, testNum++);
      }


      {
        Func<Func<int>, int> fn = MakeMethod<Func<Func<int>, int>>.Compile(parser, LexListGet(@"
          public int TestFn ( Func<int> fn ) 
          {
            return fn() ; 
          }"));
        Func<int> inputFn = () => 234;
        Test(fn(inputFn) == 234, testNum++);
      }

      {
        Func<Func<int>, Func<int>> fn = MakeMethod<Func<Func<int>, Func<int>>>.Compile(parser, LexListGet(@"
          public Func<int> TestFn ( Func<int> fn ) 
          {
            return fn ; 
          }"));
        Func<int> inputFn = () => 234;
        Test(fn(inputFn) == inputFn, testNum++);
      }

      {
        TestClass tc = new TestClass();
        Action<TestClass> init;
        Action<TestClass, Func<int>> setFn;
        Func<TestClass, Func<int>> getFn;
        MakeClass mc = new MakeClass(parser, LexListGet(@"
          partial class TestClass { 
            public Func<int> Fn ;
            public void SetFn ( Func<int> fn ) { Fn = fn ; }
            public Func<int> GetFn ( ) { return Fn ; }
          }  ")).
          GetAction<TestClass, Func<int>>("SetFn", out setFn).
          GetFunc<TestClass, Func<int>>("GetFn", out getFn).
          GetAction<TestClass>("FieldsInitialiser", out init);
        init(tc);
        Func<int> inputFn = () => 112233;
        setFn(tc, inputFn);
        Test(getFn(tc) == inputFn, testNum++);
      }
      {
        TestClass tc = new TestClass();
        Action<TestClass> init;
        MakeClass mc = new MakeClass(parser, LexListGet("partial class TestClass { public string S ; public void SetString ( string s ) { S = s ; }}  ")).GetAction<TestClass>("FieldsInitialiser", out init);
        init(tc);
        mc.DoStatement<TestClass>(tc, "SetString ( \"hello\" ) ;");
        Test(mc.DoExpression<TestClass, string>(tc, "S") == "hello", testNum++);
      }

      {
        TestClass tc = new TestClass();
        Action<TestClass> init;
        MakeClass mc = new MakeClass(parser, LexListGet("partial class TestClass { public string S ; public void SetString (  ) { S = \"what\" ; }}  ")).GetAction<TestClass>("FieldsInitialiser", out init);
        init(tc);
        mc.DoStatement<TestClass>(tc, "SetString ( ) ;");
        Test(mc.DoExpression<TestClass, string>(tc, "S") == "what", testNum++);
      }

      {
        TestClass tc = new TestClass();
        Action<TestClass> init;
        MakeClass mc = new MakeClass(parser, LexListGet("partial class TestClass { public string[] AddedString ; public int[] AddedInt ;}  ")).GetAction<TestClass>("FieldsInitialiser", out init);
        init(tc);
        mc.DoStatement<TestClass>(tc, "AddedInt = new int[100] ; for ( var i = 0 ; i<AddedInt.Length ; i++) AddedInt[i] = i*2 ; ");
        Test(mc.DoExpression<TestClass, int>(tc, "AddedInt.Length") == 100, testNum++);
        int len = mc.DoExpression<TestClass, int>(tc, "AddedInt.Length");
        Func<TestClass, int, int> fn = null;
        mc.AddMethodsAndFields(LexList.Get("partial class TestClass { public int Fn(int i) { return AddedInt[i] ; } }"), false).GetFunc<TestClass, int, int>("Fn", out fn);
        for (int i = 0; i < len; i++) Test(fn(tc, i) == (i * 2), testNum++);
      }

      {
        TestClass tc = new TestClass();
        Action<TestClass> init;
        MakeClass mc = new MakeClass(parser, LexListGet("partial class TestClass { public string[] AddedString ; public int[] AddedInt ;}  ")).GetAction<TestClass>("FieldsInitialiser", out init);
        init(tc);
        mc.DoStatement<TestClass>(tc, "this.AddedInt = new int[100] ; for ( var i = 0 ; i<this.AddedInt.Length ; i++) this.AddedInt[i] = i*2 ; ");
        Test(mc.DoExpression<TestClass, int>(tc, "this.AddedInt.Length") == 100, testNum++);
        int len = mc.DoExpression<TestClass, int>(tc, "this.AddedInt.Length");
        Func<TestClass, int, int> fn = null;
        mc.AddMethodsAndFields(LexList.Get("partial class TestClass { public int Fn(int i) { return this.AddedInt[i] ; } }"), false).GetFunc<TestClass, int, int>("Fn", out fn);
        for (int i = 0; i < len; i++) Test(fn(tc, i) == (i * 2), testNum++);
      }

      {
        TestClass tc = new TestClass();
        Action<TestClass> init;
        MakeClass mc = new MakeClass(parser, LexListGet("partial class TestClass { public string[] AddedString ; public int[] AddedInt ;}  ")).GetAction<TestClass>("FieldsInitialiser", out init);
        init(tc);
        mc.DoStatement<TestClass>(tc, "AddedString = new string[100] ; for ( var i = 0 ; i<AddedString.Length ; i++) AddedString[i] = \"string \" + (i*2) ; ");
        Test(mc.DoExpression<TestClass, int>(tc, "AddedString.Length") == 100, testNum++);
        int len = mc.DoExpression<TestClass, int>(tc, "AddedString.Length");
        Func<TestClass, int, string> fn = null;
        mc.AddMethodsAndFields(LexList.Get("partial class TestClass { public string Fn(int i) { return AddedString[i] ; } }"), false).GetFunc<TestClass, int, string>("Fn", out fn);
        for (int i = 0; i < len; i++) Test(fn(tc, i) == "string " + (i * 2), testNum++);
      }

      {
        TestClass tc = new TestClass();
        Action<TestClass> init;
        MakeClass mc = new MakeClass(parser, LexListGet("partial class TestClass { public string[] AddedString ; public int[] AddedInt ;}  ")).GetAction<TestClass>("FieldsInitialiser", out init);
        init(tc);
        mc.DoStatement<TestClass>(tc, "this.AddedString = new string[100] ; for ( var i = 0 ; i<this.AddedString.Length ; i++) this.AddedString[i] = \"string \" + (i*2) ; ");
        Test(mc.DoExpression<TestClass, int>(tc, "this.AddedString.Length") == 100, testNum++);
        int len = mc.DoExpression<TestClass, int>(tc, "this.AddedString.Length");
        Func<TestClass, int, string> fn = null;
        mc.AddMethodsAndFields(LexList.Get("partial class TestClass { public string Fn(int i) { return this.AddedString[i] ; } }"), false).GetFunc<TestClass, int, string>("Fn", out fn);
        for (int i = 0; i < len; i++) Test(fn(tc, i) == "string " + (i * 2), testNum++);
      }

      {
        TestClass tc = new TestClass();
        Action<TestClass> init;
        MakeClass mc = new MakeClass(parser, LexListGet("partial class TestClass { public string AddedString ; public int AddedInt ;}  ")).GetAction<TestClass>("FieldsInitialiser", out init);
        init(tc);
        Test(mc.DoExpression<TestClass, string>(tc, "this.AddedString") == null, testNum++);
        mc.DoStatement<TestClass>(tc, "this.AddedString = \"Hello\" ; ");
        Test(mc.DoExpression<TestClass, string>(tc, "this.AddedString") == "Hello", testNum++);
      }

      {
        TestClass tc = new TestClass();
        Action<TestClass> init;
        MakeClass mc = new MakeClass(parser, LexListGet("partial class TestClass { public string AddedString ; public int AddedInt ;}  ")).GetAction<TestClass>("FieldsInitialiser", out init);
        init(tc);
        Test(mc.DoExpression<TestClass, string>(tc, "AddedString") == null, testNum++);
        mc.DoStatement<TestClass>(tc, "AddedString = \"Hello\" ; ");
        Test(mc.DoExpression<TestClass, string>(tc, "AddedString") == "Hello", testNum++);
      }

      {
        TestClass tc = new TestClass();
        MakeClass mc = new MakeClass(parser, LexListGet("partial class TestClass { }  "));
        Test(mc.DoExpression<TestClass, int>(tc, "TestClassInt") == tc.TestClassInt, testNum++);
        tc.TestClassInt = 389;
        Test(mc.DoExpression<TestClass, int>(tc, "TestClassInt") == tc.TestClassInt, testNum++);
        mc.DoStatement<TestClass>(tc, "TestClassInt = 2343");
        Test(tc.TestClassInt == 2343, testNum++);
        tc.Id = "Hello";
        Test(mc.DoExpression<TestClass, string>(tc, "Id") == tc.Id, testNum++);
        mc.DoStatement<TestClass>(tc, "Id=\"What\"");
        Test(tc.Id == "What", testNum++);
        Test(mc.DoExpression<TestClass, string>(tc, "Id") == tc.Id, testNum++);
        mc.DoStatement<TestClass>(tc, "Id=\"What2\"", false);
        Test(tc.Id == "What2", testNum++);
        Test(mc.DoExpression<TestClass, string>(tc, "Id") == tc.Id, testNum++);
        mc.DoStatement<TestClass>(tc, "Id='What3'", true);
        Test(tc.Id == "What3", testNum++);
        Test(mc.DoExpression<TestClass, string>(tc, "Id") == tc.Id, testNum++);
      }

      {
        TestClass tc = new TestClass();
        MakeClass mc = new MakeClass(parser, LexListGet("partial class TestClass { }  "));
        Test(mc.DoExpression<TestClass, int>(tc, "this.TestClassInt") == tc.TestClassInt, testNum++);
        tc.TestClassInt = 389;
        Test(mc.DoExpression<TestClass, int>(tc, "this.TestClassInt") == tc.TestClassInt, testNum++);
        mc.DoStatement<TestClass>(tc, "this.TestClassInt = 2343");
        Test(tc.TestClassInt == 2343, testNum++);
        tc.Id = "Hello";
        Test(mc.DoExpression<TestClass, string>(tc, "this.Id") == tc.Id, testNum++);
        mc.DoStatement<TestClass>(tc, "this.Id=\"What\"");
        Test(tc.Id == "What", testNum++);
        Test(mc.DoExpression<TestClass, string>(tc, "this.Id") == tc.Id, testNum++);
        mc.DoStatement<TestClass>(tc, "this.Id=\"What2\"", false);
        Test(tc.Id == "What2", testNum++);
        Test(mc.DoExpression<TestClass, string>(tc, "this.Id") == tc.Id, testNum++);
        mc.DoStatement<TestClass>(tc, "this.Id='What3'", true);
        Test(tc.Id == "What3", testNum++);
        Test(mc.DoExpression<TestClass, string>(tc, "this.Id") == tc.Id, testNum++);
      }


      Test(MakeMethod.DoExpression<long>("1+2", false) == 3, testNum++);
      Test(MakeMethod.DoExpression<int>("1+2*7-41%2+(int)23D/3", false) == 1 + 2 * 7 - 41 % 2 + (int)23D / 3, testNum++);
      Test(MakeMethod.DoExpression<int>("TestClass2.TestClass2StaticInt", false) == TestClass2.TestClass2StaticInt, testNum++);
      Test(MakeMethod.DoExpression<double>("97D/13", false) == 97D / 13, testNum++);

      TestStruct.TestStructStaticInt = 1111;
      Test(MakeMethod.DoExpression<int>("TestStruct.TestStructStaticInt") == 1111, testNum++);
      MakeMethod.DoStatement("TestStruct.TestStructStaticInt = 2222;");
      Test(MakeMethod.DoExpression<int>("TestStruct.TestStructStaticInt") == 2222, testNum++);



      {
        Func<int, int> fn = MakeMethod<Func<int, int>>.Compile(parser, LexListGet(@"
          int fn ( int i ) 
          {
            if (i == 1 ) return -1 ; 
            else if ( i == 2 ) return -2 ; 
            else if ( i == 3 ) return -3 ; 
            else return i ; 
          }"));
        Test(fn(1) == -1, testNum++);
        Test(fn(2) == -2, testNum++);
        Test(fn(3) == -3, testNum++);
        Test(fn(4) == 4, testNum++);
      }

      {
        Func<long> fn = MakeMethod<Func<long>>.Compile(parser, LexListGet(@"
          long fn () 
          {
            int ch = 99 ; 
            long num = 10 ; 
            num = num * 10 + ch - 98 ; 
            return num ; 
          }"));
        long n = fn();
        Test(fn() == 101, testNum++);
      }

      {
        Func<long> fn = MakeMethod<Func<long>>.Compile(parser, LexListGet(@"
          long fn () 
          {
            char ch = `1` ; 
            long num = 10 ; 
            num = num * 10 + ch - `0` ; 
            return num ; 
          }"));
        long n = fn();
        Test(fn() == 101, testNum++);
      }

      {
        Func<long> fn = MakeMethod<Func<long>>.Compile(parser, LexListGet(@"
          long fn () 
          {
            char ch = `1` ; 
            long num = 100 + ch - `0` ; 
            return num ; 
          }"));
        long n = fn();
        Test(fn() == 101, testNum++);
      }

      {
        Func<int> fn = MakeMethod<Func<int>>.Compile(parser, LexListGet(@"
          int fn () 
          {
            char ch = `1` ; 
            int num = 10 ; 
            num = num * 10 + ch - `0` ; 
            return num ; 
          }"));
        Test(fn() == 101, testNum++);
      }

      {
        Func<int> fn = MakeMethod<Func<int>>.Compile(parser, LexListGet(@"
          int fn () 
          {
            char ch = `1` ; 
            int num = 100 + ch - `0` ; 
            return num ; 
          }"));
        Test(fn() == 101, testNum++);
      }

      {
        Func<string> fn = MakeMethod<Func<string>>.Compile(parser, LexList.Get(@"
          string TestForEachChar ()
          {
            string s = """" ; 
            foreach ( var ch in ""123456789"" ) s += ch.ToString() ;  
            return s ; 
          } "));
        Test(fn() == "123456789", testNum++);
      }

      {
        Func<int> fn = MakeMethod<Func<int>>.Compile(parser, LexListGet(@"
          int TestDelegate ()
          {
            return TestClass2.Fn () ; 
          }"));
        Test(fn() == TestClass2.Fn(), testNum++);

      }

      {
        Func<int> fn = MakeMethod<Func<int>>.Compile(parser, LexListGet(@"
          int TestDelegate ()
          {
            return TestClass2.GetFn ()() ; 
          }"));
        Test(fn() == TestClass2.Fn(), testNum++);

      }
      {
        Func<int> fn = MakeMethod<Func<int>>.Compile(parser, LexListGet(@"
          int TestDelegate ()
          {
            return TestClass2.GetGetGetFn ()()()() ; 
          }"));
        Test(fn() == TestClass2.Fn(), testNum++);

      }

      Test(((MakeMethod<Func<double>>.Compile(parser, LexListGet("double Test () { return '23.45'.StrToDouble() ; }")))()) == "23.45".StrToDouble(), testNum++);


      TestType(parser, "System.Int32", typeof(System.Int32), testNum++);
      TestType(parser, "Dictionary<string,int>", typeof(Dictionary<string, int>), testNum++);
      TestType(parser, "LexKind", typeof(LexKind), testNum++);
      TestType(parser, "List<List<List<List<List<int>>>>>", typeof(List<List<List<List<List<int>>>>>), testNum++);
      TestType(parser, "List<List<List<List<int>>>>", typeof(List<List<List<List<int>>>>), testNum++);
      TestType(parser, "List<List<List<int>>>", typeof(List<List<List<int>>>), testNum++);
      TestType(parser, "List<List<int[]>[]>[]", typeof(List<List<int[]>[]>[]), testNum++);
      TestType(parser, "List<List<int[]>>", typeof(List<List<int[]>>), testNum++);
      TestType(parser, "List<List<int>>", typeof(List<List<int>>), testNum++);
      TestType(parser, "List<int>", typeof(List<int>), testNum++);
      TestType(parser, "int", typeof(int), testNum++);
      TestType(parser, "int[]", typeof(int[]), testNum++);
      TestType(parser, "List<List<Dictionary<string,List<double>>>>", typeof(List<List<Dictionary<string, List<double>>>>), testNum++);

      {
        Func<long> fn = MakeMethod<Func<long>>.Compile(parser, LexListGet(@"
          long testAssignment ()
          {
            int i = 456 ; 
            object o = (object)( i + 1000 ) ;
            int ii = (object)((int)o + 1000 ) ;
            long ll = (object)((long)((int)(object)ii + 1000 ) + 1000 );
            return (long)ll + 1000 ; 
          }"));
        Test(fn() == 5456, testNum++);
      }

      {
        Func<string> fn = MakeMethod<Func<string>>.Compile(parser, LexListGet(@"
          string test ()
          {
            string s = 'a'.ToString() + 'bbbb' + (34.56).ToString() + 'ccc' ; 
            return s ; 
          }"));
        Test(fn() == "abbbb34.56ccc", testNum++);
      }

      {
        Func<long> fn = MakeMethod<Func<long>>.Compile(parser, LexListGet(@"
          long testAssignment ()
          {
            int i = 456 ; 
            object o = (object)i ;
            int ii = (object)(int)o ;
            long ll = (object)(long)(int)(object)ii ;
            return (long)ll ; 
          }"));
        Test(fn() == 456, testNum++);
      }

      {
        Func<object, TestClassExtension> fn = MakeMethod<Func<object, TestClassExtension>>.Compile(parser, LexListGet(@"
          TestClassExtension testAssignment (object o)
          {
            return (TestClassExtension) o ; 
          }"));
        TestClassExtension tce = new TestClassExtension();
        Test(fn((object)tce) == tce, testNum++);
      }

      {
        Func<object, Double> fn = MakeMethod<Func<object, double>>.Compile(parser, LexListGet(@"
          double testAssignment (object o)
          {
            return  2 * (double) o ; 
          }"));
        double dd = 123.45;
        Test(fn((object)dd) == 2 * dd, testNum++);
      }

      {
        Func<TestClassExtension, int> getLength;
        Action<TestClassExtension, int[]> setArray;
        Action<TestClassExtension> actInit;
        MakeClass mc = new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public int[] LocalInt ;
            public void SetArray ( int[] input ) 
            {
              LocalInt = input ; 
            }   
            public int GetLength () { return LocalInt.Length ; }
          }")).
            GetFunc<TestClassExtension, int>("GetLength", out getLength).
            GetAction<TestClassExtension, int[]>("SetArray", out setArray).
            GetAction<TestClassExtension>("FieldsInitialiser", out actInit);
        TestClassExtension tce = new TestClassExtension();
        actInit(tce);
        int[] thearray = new int[300];
        setArray(tce, thearray);
        Test(getLength(tce) == thearray.Length, testNum++);
      }


      {
        Func<TestClassExtension, int> getLength;
        Action<TestClassExtension, int[]> setArray;
        Action<TestClassExtension> actInit;
        MakeClass mc = new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public int[] LocalInt ;
            public void SetArray ( int[] input ) 
            {
              this.LocalInt = input ; 
            }   
            public int GetLength () { return this.LocalInt.Length ; }
          }")).
            GetFunc<TestClassExtension, int>("GetLength", out getLength).
            GetAction<TestClassExtension, int[]>("SetArray", out setArray).
            GetAction<TestClassExtension>("FieldsInitialiser", out actInit);
        TestClassExtension tce = new TestClassExtension();
        actInit(tce);
        int[] thearray = new int[300];
        setArray(tce, thearray);
        Test(getLength(tce) == thearray.Length, testNum++);
      }



      {
        Func<TestClassExtension, int, int> getFn;
        Action<TestClassExtension, int, int> setAction;
        Action<TestClassExtension> actInit;
        Action<TestClassExtension, int[]> setArray;
        MakeClass mc = new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public void Set ( int i , int v )  
            {
              LocalInt[i+2] = (v+100)*2 ; 
            }
            public int[] LocalInt ;
            public void SetArray ( int[] input ) 
            {
              LocalInt = input ; 
            }   
            public int Get ( int i )  
            {
              int j = 100 ; 
              return  LocalInt[i+2]/2 - j ; 
            }
          }")).
            GetFunc<TestClassExtension, int, int>("Get", out getFn).
            GetAction<TestClassExtension, int, int>("Set", out setAction).
            GetAction<TestClassExtension, int[]>("SetArray", out setArray).
            GetAction<TestClassExtension>("FieldsInitialiser", out actInit);
        TestClassExtension tce = new TestClassExtension();
        actInit(tce);
        int[] thearray = new int[300];
        setArray(tce, thearray);
        setAction(tce, 22, 33);
        Test(getFn(tce, 22) == 33, testNum++);
      }

      {
        Func<TestClassExtension, int, int> getFn;
        Action<TestClassExtension, int, int> setAction;
        Action<TestClassExtension> actInit;
        Action<TestClassExtension, int[]> setArray;
        MakeClass mc = new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public void Set ( int i , int v )  
            {
              this.LocalInt[i+2] = (v+100)*2 ; 
            }
            public int[] LocalInt ;
            public void SetArray ( int[] input ) 
            {
              this.LocalInt = input ; 
            }   
            public int Get ( int i )  
            {
              int j = 100 ; 
              return  this.LocalInt[i+2]/2 - j ; 
            }
          }")).
            GetFunc<TestClassExtension, int, int>("Get", out getFn).
            GetAction<TestClassExtension, int, int>("Set", out setAction).
            GetAction<TestClassExtension, int[]>("SetArray", out setArray).
            GetAction<TestClassExtension>("FieldsInitialiser", out actInit);
        TestClassExtension tce = new TestClassExtension();
        actInit(tce);
        int[] thearray = new int[300];
        setArray(tce, thearray);
        setAction(tce, 22, 33);
        Test(getFn(tce, 22) == 33, testNum++);
      }

      {
        Func<TestClassExtension, int, int> getFn;
        Action<TestClassExtension, int, int> setAction;
        Action<TestClassExtension> actInit;
        Action<TestClassExtension, int[]> setArray;
        MakeClass mc = new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public void Set ( int i , int v )  
            {
              LocalInt[i] = v ; 
            }
            public int[] LocalInt ;
            public void SetArray ( int[] input ) 
            {
              LocalInt = input ; 
            }   
            public int Get ( int i )  
            {
              return LocalInt[i] ; 
            }
          }")).
            GetFunc<TestClassExtension, int, int>("Get", out getFn).
            GetAction<TestClassExtension, int, int>("Set", out setAction).
            GetAction<TestClassExtension, int[]>("SetArray", out setArray).
            GetAction<TestClassExtension>("FieldsInitialiser", out actInit);
        TestClassExtension tce = new TestClassExtension();
        actInit(tce);
        int[] thearray = new int[300];
        setArray(tce, thearray);
        setAction(tce, 22, 33);
        Test(getFn(tce, 22) == 33, testNum++);
      }

      {
        Func<TestClassExtension, int, int> getFn;
        Action<TestClassExtension, int, int> setAction;
        Action<TestClassExtension> actInit;
        Action<TestClassExtension, int[]> setArray;
        MakeClass mc = new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public void Set ( int i , int v )  
            {
              this.LocalInt[i] = v ; 
            }
            public int[] LocalInt ;
            public void SetArray ( int[] input ) 
            {
              this.LocalInt = input ; 
            }   
            public int Get ( int i )  
            {
              return this.LocalInt[i] ; 
            }
          }")).
            GetFunc<TestClassExtension, int, int>("Get", out getFn).
            GetAction<TestClassExtension, int, int>("Set", out setAction).
            GetAction<TestClassExtension, int[]>("SetArray", out setArray).
            GetAction<TestClassExtension>("FieldsInitialiser", out actInit);
        TestClassExtension tce = new TestClassExtension();
        actInit(tce);
        int[] thearray = new int[300];
        setArray(tce, thearray);
        setAction(tce, 22, 33);
        Test(getFn(tce, 22) == 33, testNum++);
      }

      {
        Func<TestClassExtension, int, int> fn;
        Action<TestClassExtension> actInit;
        MakeClass mc = new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public int LocalInt ; 
            public int TestThis ( int i ) 
            {
              this.LocalInt = i ; 
              return this.LocalInt ; 
            }
          }")).GetFunc<TestClassExtension, int, int>("TestThis", out fn).GetAction<TestClassExtension>("FieldsInitialiser", out actInit);
        TestClassExtension tce = new TestClassExtension();
        actInit(tce);
        Test(fn(tce, 1234) == 1234, testNum++);
      }

      {
        Func<TestClassExtension, int, int> fn;
        Action<TestClassExtension> actInit;
        MakeClass mc = new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public int LocalInt ; 
            public int TestThis ( int i ) 
            {
              LocalInt = i ; 
              return LocalInt ; 
            }
          }")).GetFunc<TestClassExtension, int, int>("TestThis", out fn).GetAction<TestClassExtension>("FieldsInitialiser", out actInit);
        TestClassExtension tce = new TestClassExtension();
        actInit(tce);
        Test(fn(tce, 1234) == 1234, testNum++);
      }

      {
        Action<TestClassExtension, int> setAction;
        Func<TestClassExtension, int> getFn;
        Action<TestClassExtension> actInit;
        MakeClass mc = new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public int LocalInt ; 
            public void Set ( int i ) 
            {
              LocalInt = i ; 
            }
            public int Get () 
            {
              return LocalInt ; 
            }
          }")).
            GetFunc<TestClassExtension, int>("Get", out getFn).
            GetAction<TestClassExtension>("FieldsInitialiser", out actInit).
            GetAction<TestClassExtension, int>("Set", out setAction);
        TestClassExtension tce = new TestClassExtension();
        actInit(tce);
        setAction(tce, 34565);
        Test(getFn(tce) == 34565, testNum++);
      }

      {
        Action<TestClassExtension, int> setAction;
        Func<TestClassExtension, int> getFn;
        Action<TestClassExtension> actInit;
        MakeClass mc = new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public int LocalInt ; 
            public void Set ( int i ) 
            {
              this.LocalInt = i ; 
            }
            public int Get () 
            {
              return this.LocalInt ; 
            }
          }")).
            GetFunc<TestClassExtension, int>("Get", out getFn).
            GetAction<TestClassExtension>("FieldsInitialiser", out actInit).
            GetAction<TestClassExtension, int>("Set", out setAction);
        TestClassExtension tce = new TestClassExtension();
        actInit(tce);
        setAction(tce, 34565);
        Test(getFn(tce) == 34565, testNum++);
      }

      {
        Action<TestClassExtension, double> setAction;
        Func<TestClassExtension, double> getFn;
        Action<TestClassExtension> actInit;
        MakeClass mc = new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public double LocalInt ; 
            public void Set ( double i ) 
            {
              this.LocalInt = i ; 
            }
            public double Get () 
            {
              return this.LocalInt ; 
            }
          }")).
            GetFunc<TestClassExtension, double>("Get", out getFn).
            GetAction<TestClassExtension>("FieldsInitialiser", out actInit).
            GetAction<TestClassExtension, double>("Set", out setAction);
        TestClassExtension tce = new TestClassExtension();
        actInit(tce);
        setAction(tce, 3456.5);
        Test(getFn(tce) == 3456.5, testNum++);
      }

      {
        Action<TestClassExtension, double> setAction;
        Func<TestClassExtension, double> getFn;
        Action<TestClassExtension> actInit;
        MakeClass mc = new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public double LocalInt ; 
            public void Set ( double i ) 
            {
              LocalInt = i ; 
            }
            public double Get () 
            {
              return LocalInt ; 
            }
          }")).
            GetFunc<TestClassExtension, double>("Get", out getFn).
            GetAction<TestClassExtension>("FieldsInitialiser", out actInit).
            GetAction<TestClassExtension, double>("Set", out setAction);
        TestClassExtension tce = new TestClassExtension();
        actInit(tce);
        setAction(tce, 3456.5);
        Test(getFn(tce) == 3456.5, testNum++);
      }

      {
        Action<TestClassExtension, List<int>> setAction;
        Func<TestClassExtension, List<int>> getFn;
        Action<TestClassExtension> actInit;
        MakeClass mc = new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public List<int> LocalInt ; 
            public void Set ( List<int> i ) 
            {
              LocalInt = i ; 
            }
            public List<int> Get () 
            {
              return LocalInt ; 
            }
          }")).
            GetFunc<TestClassExtension, List<int>>("Get", out getFn).
            GetAction<TestClassExtension>("FieldsInitialiser", out actInit).
            GetAction<TestClassExtension, List<int>>("Set", out setAction);
        TestClassExtension tce = new TestClassExtension();
        actInit(tce);
        List<int> testlist = new List<int>();
        setAction(tce, testlist);
        Test(getFn(tce) == testlist, testNum++);
      }
      {
        Action<TestClassExtension, List<int>> setAction;
        Func<TestClassExtension, List<int>> getFn;
        Action<TestClassExtension> actInit;
        MakeClass mc = new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public List<int> LocalInt ; 
            public void Set ( List<int> i ) 
            {
              this.LocalInt = i ; 
            }
            public List<int> Get () 
            {
              return this.LocalInt ; 
            }
          }")).
            GetFunc<TestClassExtension, List<int>>("Get", out getFn).
            GetAction<TestClassExtension>("FieldsInitialiser", out actInit).
            GetAction<TestClassExtension, List<int>>("Set", out setAction);
        TestClassExtension tce = new TestClassExtension();
        actInit(tce);
        List<int> testlist = new List<int>();
        setAction(tce, testlist);
        Test(getFn(tce) == testlist, testNum++);
      }


      {
        Func<string> fn = MakeMethod<Func<string>>.Compile(parser, LexListGet(@"
          string testDiv ()
          {
            return (33 / 4.5).ToString() ;
          }"));
        Test((33 / 4.5).ToString() == fn(), testNum++);
      }
      {
        Func<string> fn = MakeMethod<Func<string>>.Compile(parser, LexListGet(@"
          string testDiv ()
          {
            return ( 40.5/ 3 ).ToString() ;
          }"));
        Test((40.5 / 3).ToString() == fn(), testNum++);
      }


      {
        Func<string> fn = MakeMethod<Func<string>>.Compile(parser, LexListGet(@"
          string testAssignment ()
          {
            string result = '' ; 
            result += 'hello ' ; 
            result += 'there from ' + 'the code ' + 77 ; 
            return result ; 
          }"));
        Test("hello there from the code 77" == fn(), testNum++);
      }

      {
        Func<string> fn = MakeMethod<Func<string>>.Compile(parser, LexListGet(@"
          string testAssignment ()
          {
            string result = '' ; 
            int i = 33 ; 
            i += 44 ; 
            result += i.ToString () + ` ` ; 
            i -= 17 ; 
            result += i.ToString () + ` ` ; 
            i *= 18 ; 
            result += i.ToString () + ` ` ; 
            i /= 7 ; 
            result += i.ToString () + ` ` ; 
            return result ; 
          }"));
        string result = "";
        int i = 33;
        i += 44;
        result += i.ToString() + " ";
        i -= 17;
        result += i.ToString() + " ";
        i *= 18;
        result += i.ToString() + " ";
        i /= 7;
        result += i.ToString() + " ";
        string rr = fn();
        Test(result == rr, testNum++);
      }

      {
        Func<string> fn = MakeMethod<Func<string>>.Compile(parser, LexListGet(@"
          string testInc ()
          {
            string result = '' ; 
            int i = 33 ; 
            result = result + ` ` + i ; 
            --i ; 
            result = result + ` ` + i ; 
            int k = -- i ; 
            result = result + ` ` + k + ` ` + i ; 
            ++i ; 
            result = result + ` ` + i ; 
            k = ++ i ; 
            result = result + ` ` + k + ` ` + i ; 
            k = 20 + ++ i ; 
            result = result + ` ` + k + ` ` + i ; 
            k = 20 + ++ i - 30 ; 
            result = result + ` ` + k + ` ` + i ; 
            k = 20 + --i;
            result = result + ` ` + k + ' ' + i;
            k = 20 + --i - 30;
            result = result + ` ` + k + ' ' + i; 
            
            result = result + ` ` + i ; 
            i-- ; 
            result = result + ` ` + i ; 
            k =  i-- ; 
            result = result + ` ` + k + ` ` + i ; 
            i++ ; 
            result = result + ` ` + i ; 
            k =  i ++; 
            result = result + ` ` + k + ` ` + i ; 
            k = 20 +  i ++; 
            result = result + ` ` + k + ` ` + i ; 
            k = 20 +  i++ - 30 ; 
            result = result + ` ` + k + ` ` + i ; 
            k = 20 + i--;
            result = result + ` ` + k + ' ' + i;
            k = 20 + i-- - 30;
            result = result + ` ` + k + ' ' + i;
            return result ; 
          }"));

        string result = "";
        int i = 33;
        result = result + ' ' + i;
        --i;
        result = result + ' ' + i;
        int k = --i;
        result = result + ' ' + k + ' ' + i;
        ++i;
        result = result + ' ' + i;
        k = ++i;
        result = result + ' ' + k + ' ' + i;
        k = 20 + ++i;
        result = result + ' ' + k + ' ' + i;
        k = 20 + ++i - 30;
        result = result + ' ' + k + ' ' + i;
        k = 20 + --i;
        result = result + ' ' + k + ' ' + i;
        k = 20 + --i - 30;
        result = result + ' ' + k + ' ' + i;

        result = result + ' ' + i;
        i--;
        result = result + ' ' + i;
        k = i--;
        result = result + ' ' + k + ' ' + i;
        i++;
        result = result + ' ' + i;
        k = i++;
        result = result + ' ' + k + ' ' + i;
        k = 20 + i++;
        result = result + ' ' + k + ' ' + i;
        k = 20 + i++ - 30;
        result = result + ' ' + k + ' ' + i;
        k = 20 + i--;
        result = result + ' ' + k + ' ' + i;
        k = 20 + i-- - 30;
        result = result + ' ' + k + ' ' + i;

        Test(fn() == result, testNum++);

      }

      {
        Func<string> fn = MakeMethod<Func<string>>.Compile(parser, LexListGet(@"
          string TestFor ()
          {
            int[] array = new int[100] ; 
            int k = 0 ;   
            for ( var j = 100 ; j > 0 ; j = j - 2 ) {
              array[k] = j ; 
              k = k + 1 ; 
            }
            int[] anotherArray = new int[k] ; 
            for ( var j = 0 ; j<k ; j = j + 1 ) anotherArray[j] = array[j] ; 
            string result = '' ; 
            foreach ( var s in anotherArray ) {
              result = result + ` ` + s.ToString() ; 
            }
              
            return result ; 
          }"));

        string r = "";
        for (var j = 100; j > 0; j = j - 2) {
          r = r + ' ' + j;
        }
        Test(r == fn(), testNum++);
      }

      {
        Func<string> fn = MakeMethod<Func<string>>.Compile(parser, LexListGet(@"
          string TestFor ()
          {
            List<int> list = new List<int> () ;  
            for ( var j = 100 ; j > 0 ; j = j - 2 ) {
              list.Add ( j ) ; 
            }
            string result = '' ; 
            foreach ( var s in list ) {
              result = result + ` ` + s.ToString() ; 
            }
              
            return result ; 
          }"));

        string r = "";
        for (var j = 100; j > 0; j = j - 2) {
          r = r + ' ' + j;
        }
        Test(r == fn(), testNum++);
      }

      {
        Func<string> fn = MakeMethod<Func<string>>.Compile(parser, LexListGet(@"
          string TestFor ()
          {
            List<string> list = new List<string> () ;  
            for ( var j = 100 ; j > 0 ; j = j - 2 ) {
              list.Add ( j.ToString() ) ; 
            }
            string result = '' ; 
            foreach ( var s in list ) {
              result = result + ` ` + s ; 
            }
              
            return result ; 
          }"));

        string r = "";
        for (var j = 100; j > 0; j = j - 2) {
          r = r + ' ' + j;
        }
        Test(r == fn(), testNum++);
      }

      {
        Func<string> fn = MakeMethod<Func<string>>.Compile(parser, LexListGet(@"
          string TestFor ()
          {
            string result = '' ; 
            for ( var j = 100 ; j > 0 ; j = j - 2 ) {
              result = result + ` ` + j ; 
            }
            return result ; 
          }"));

        string r = "";
        for (var j = 100; j > 0; j = j - 2) {
          r = r + ' ' + j;
        }
        Test(r == fn(), testNum++);
      }



      {
        Func<Dictionary<string, int>> fn = MakeMethod<Func<Dictionary<string, int>>>.Compile(parser, LexListGet(@"
          Dictionary<string,int> TestDict () 
          {
            Dictionary<string,int> dict = new Dictionary<string,int> () ;
            dict['one'] = 1 ; 
            dict['two'] = 2 ; 
            dict['three'] = 3 ; 
            dict['six'] = dict['three'] * 2 ; 
            dict['twelve'] = dict['two'] * dict['three'] * 2 ;
            return dict ; 
          }
        "));

        Dictionary<string, int> dict = fn();
        Test(dict["one"] == 1, testNum++);
        Test(dict["two"] == 2, testNum++);
        Test(dict["three"] == 3, testNum++);
        Test(dict["six"] == 6, testNum++);
        Test(dict["twelve"] == 12, testNum++);
      }

      {
        Func<string> fn = MakeMethod<Func<string>>.Compile(parser, LexListGet(@" 
          string TestConversions () 
          {
            string result = '' ; 
            Point P = new Point ( 3 , 4 ) ; 
            result = result + ` ` + P.ToString() ; 
            P = new Point ( 3F , 4F ) ; 
            result = result + ` ` + P.ToString() ; 
            P = new Point ( 3D , 4D ) ; 
            result = result + ` ` + P.ToString() ; 
            P = new Point ( 3L , 4L ) ; 
            result = result + ` ` + P.ToString() ;

            result = result + ` ` + TestClass.IntToString ( 24 ) ; 
 
            result = result + ` ` + TestClass.LongToString ( 24 ) ; 
            result = result + ` ` + TestClass.LongToString ( 24L ) ; 
 
            result = result + ` ` + TestClass.FloatToString ( 24 ) ; 
            result = result + ` ` + TestClass.FloatToString ( 24L ) ; 
            result = result + ` ` + TestClass.FloatToString ( 24F ) ; 

            result = result + ` ` + TestClass.DoubleToString ( 24 ) ; 
            result = result + ` ` + TestClass.DoubleToString ( 24L ) ; 
            result = result + ` ` + TestClass.DoubleToString ( 24F ) ; 
            result = result + ` ` + TestClass.DoubleToString ( 24D ) ; 

            return result ; 
          }
        "));
        string result = "";
        Point P = new Point(3, 4);
        result = result + ' ' + P.ToString();
        P = new Point(3F, 4F);
        result = result + ' ' + P.ToString();
        P = new Point(3D, 4D);
        result = result + ' ' + P.ToString();
        P = new Point(3L, 4L);
        result = result + ' ' + P.ToString();

        result = result + ' ' + TestClass.IntToString(24);

        result = result + ' ' + TestClass.LongToString(24);
        result = result + ' ' + TestClass.LongToString(24L);

        result = result + ' ' + TestClass.FloatToString(24);
        result = result + ' ' + TestClass.FloatToString(24L);
        result = result + ' ' + TestClass.FloatToString(24F);

        result = result + ' ' + TestClass.DoubleToString(24);
        result = result + ' ' + TestClass.DoubleToString(24L);
        result = result + ' ' + TestClass.DoubleToString(24F);
        result = result + ' ' + TestClass.DoubleToString(24D);

        Test(fn() == result, testNum++);
      }

      {
        Func<TestClass, List<int>, Dictionary<string, List<int>>, int[]> fn = MakeMethod<Func<TestClass, List<int>, Dictionary<string, List<int>>, int[]>>.Compile(parser, LexListGet(@"
          int[] TestGetHashCode (TestClass tc,List<int> list, Dictionary<string,List<int>> dict) 
          {
            int[] array = new int[20] ; 

            int i = 2 ; 
            array[4] = i.GetHashCode() ; 
            array[5] = tc.GetHashCode() ; 
            array[6] = list.GetHashCode() ; 
            array[7] = dict.GetHashCode() ;
            BindingFlags bf = BindingFlags.Public ; 
            array[8] = bf.GetHashCode() ;  
            Point p = new Point (9D,7D) ; 
            array[9] = p.GetHashCode() ; 
            return array ; 
         }"));

        int[] array = new int[20];
        int i = 2;
        array[4] = i.GetHashCode();
        TestClass tc = new TestClass();
        array[5] = tc.GetHashCode();
        List<int> list = new List<int>();
        array[6] = list.GetHashCode();
        Dictionary<string, List<int>> dict = new Dictionary<string, List<int>>();
        array[7] = dict.GetHashCode();
        BindingFlags bf = BindingFlags.Public;
        array[8] = bf.GetHashCode();
        Point p = new Point(9, 7);
        array[9] = p.GetHashCode();
        int[] arrayFn = fn(tc, list, dict);
        for (int j = 0; j < array.Length; j++) {
          Test(array[j] == arrayFn[j], testNum++);
        }
      }



      {
        Func<string[]> fn = MakeMethod<Func<string[]>>.Compile(parser, LexListGet(@"
          string[] TestType () 
          {
            string[] array = new string[20] ; 

            int i = 2 ; 
            array[4] = i.ToString() ; 
            TestClass tc = new TestClass() ; 
            array[5] = tc.ToString() ; 
            List<int> list = new List<int>() ; 
            array[6] = list.ToString() ; 
            Dictionary<string,List<int>> dict = new Dictionary<string,List<int>> () ; 
            array[7] = dict.ToString() ;
            BindingFlags bf = BindingFlags.Public ; 
            array[8] = bf.ToString() ;  
            Point p = new Point (9D,7D) ; 
            array[9] = p.ToString() ; 
            return array ; 
         }"));

        string[] arrayFn = fn();
        string[] array = new string[20];
        int i = 2;
        array[4] = i.ToString();
        TestClass tc = new TestClass();
        array[5] = tc.ToString();
        List<int> list = new List<int>();
        array[6] = list.ToString();
        Dictionary<string, List<int>> dict = new Dictionary<string, List<int>>();
        array[7] = dict.ToString();
        BindingFlags bf = BindingFlags.Public;
        array[8] = bf.ToString();
        Point p = new Point(9, 7);
        array[9] = p.ToString();
        for (int j = 0; j < array.Length; j++) {
          Test(array[j] == arrayFn[j], testNum++);
        }
      }


      {
        Func<Type[]> fn = MakeMethod<Func<Type[]>>.Compile(parser, LexListGet(@"
          Type[] TestType () 
          {
            Type[] array = new Type[20] ; 

            int i = 2 ; 
            array[4] = i.GetType() ; 
            TestClass tc = new TestClass() ; 
            array[5] = tc.GetType() ; 
            List<int> list = new List<int>() ; 
            array[6] = list.GetType() ; 
            Dictionary<string,List<int>> dict = new Dictionary<string,List<int>> () ; 
            array[7] = dict.GetType() ;
            BindingFlags bf = BindingFlags.Public ; 
            array[8] = bf.GetType() ;  
            Point p = new Point (2,3) ; 
            array[9] = p.GetType() ; 
            return array ; 
         }"));

        Type[] arrayFn = fn();
        Type[] array = new Type[20];
        int i = 2;
        array[4] = i.GetType();
        TestClass tc = new TestClass();
        array[5] = tc.GetType();
        List<int> list = new List<int>();
        array[6] = list.GetType();
        Dictionary<string, List<int>> dict = new Dictionary<string, List<int>>();
        array[7] = dict.GetType();
        BindingFlags bf = BindingFlags.Public;
        array[8] = bf.GetType();
        Point p = new Point(2D, 3D);
        array[9] = p.GetType();
        for (int j = 0; j < array.Length; j++) {
          Test(array[j] == arrayFn[j], testNum++);
        }
      }

      {
        Func<Type[]> fn = MakeMethod<Func<Type[]>>.Compile(parser, LexListGet(@"
          Type[] TestType () 
          {
            Type[] array = new Type[20] ; 
            array[0] = typeof(int) ;
            array[1] = typeof(TestClass) ; 
            array[2] = typeof(List<int>) ; 
            array[3] = typeof(Dictionary<string,List<int>>) ; 

            return array ; 
         }"));

        Type[] arrayFn = fn();
        Type[] array = new Type[20];
        array[0] = typeof(int);
        array[1] = typeof(TestClass);
        array[2] = typeof(List<int>);
        array[3] = typeof(Dictionary<string, List<int>>);

        for (int j = 0; j < array.Length; j++) {
          Test(array[j] == arrayFn[j], testNum++);
        }
      }


      {
        Func<int[]> fn = MakeMethod<Func<int[]>>.Compile(parser, LexListGet(@"
          int[] TestTwoLoops () 
          {
            int[] test = new int[10] ; 
            for ( int i = 0 ; i< 3 ; i++ ) test[i] = i ; 
            for ( int i = 9 ; i>4 ; i-- ) test[i] = i ; 
            return test ; 
          }"));

        int[] testfn = fn();
        int[] test = new int[10];
        for (int i = 0; i < 3; i++) test[i] = i;
        for (int i = 9; i > 4; i--) test[i] = i;
        for (int i = 0; i < testfn.Length; i++) {
          Test(testfn[i] == test[i], testNum++);
        }
      }


      {
        Func<bool[]> fn = MakeMethod<Func<bool[]>>.Compile(parser, LexListGet(@"
          bool[] TestNullComparisons() 
          {
            bool[] returns = new bool[100] ; 
            TestClass ptr1 = null ; 
            TestClass ptr2 = new TestClass () ;
            int ix = 0 ;  
            returns[ix++] = ptr1 == null ;  
            returns[ix++] = ptr1 != null ;  
            returns[ix++] = ptr2 == null ;  
            returns[ix++] = ptr2 != null ;  
            returns[ix++] = null == ptr1 ;  
            returns[ix++] = null != ptr1  ;  
            returns[ix++] = null == ptr2 ;  
            returns[ix++] = null != ptr2 ;  
            returns[ix++] = null == null ;  
            returns[ix++] = null != null ; 
            returns[ix++] = ptr1 == ptr1 ;  
            returns[ix++] = ptr1 != ptr1 ; 
            returns[ix++] = ptr2 == ptr2 ;  
            returns[ix++] = ptr2 != ptr2 ; 
            return returns ;  
          }"));

        bool[] testone = new bool[100];
        TestClass ptr1 = null;
        TestClass ptr2 = new TestClass();
        int ix = 0;
        testone[ix++] = ptr1 == null;
        testone[ix++] = ptr1 != null;
        testone[ix++] = ptr2 == null;
        testone[ix++] = ptr2 != null;
        testone[ix++] = null == ptr1;
        testone[ix++] = null != ptr1;
        testone[ix++] = null == ptr2;
        testone[ix++] = null != ptr2;
        testone[ix++] = null == null;
        testone[ix++] = null != null;
        testone[ix++] = ptr1 == ptr1;
        testone[ix++] = ptr1 != ptr1;
        testone[ix++] = ptr2 == ptr2;
        testone[ix++] = ptr2 != ptr2;

        bool[] testtwo = fn();

        for (int i = 0; i < testone.Length; i++) {
          Test(testtwo[i] == testone[i], testNum++);
        }
      }
      {
        Func<string[]> fn = MakeMethod<Func<string[]>>.Compile(parser, LexListGet(@"
          string[] TestEnumsToString() 
          {
            string[] returns = new string[5] ; 
            returns[0] = BindingFlags.Public.ToString() ; 
            returns[1] = ( BindingFlags.Public | BindingFlags.Instance ) .ToString() ; 
            BindingFlags bf = BindingFlags.Instance ; 
            returns[3] = bf.ToString() ; 
            return returns ;  
          }"));

        string[] testvalues = fn();

        string[] names = new string[5];
        names[0] = BindingFlags.Public.ToString();
        names[1] = (BindingFlags.Public | BindingFlags.Instance).ToString();
        BindingFlags bf = BindingFlags.Instance;
        names[3] = bf.ToString();


        for (int i = 0; i < testvalues.Length; i++) {
          Test(names[i] == testvalues[i], testNum++);
        }
      }

      {
        Func<int[]> fn = MakeMethod<Func<int[]>>.Compile(parser, LexListGet(@"
          int[] TestNestedLocalDeclarations () 
          {
            int[] values = new int[5] ;
            int a = 2 ; 
            { 
              int b = 64 ; 
              values[0] = b ; 
              {
                int c = 256 ; 
                values[1] = c ; 
              }
              values[2] = b ; 
            }
            values[3] = a ; 
            return values ; 
          }"));

        int[] testvalues = fn();
        int[] values = new int[5];
        int a = 2;
        {
          int b = 64;
          values[0] = b;
          {
            int c = 256;
            values[1] = c;
          }
          values[2] = b;
        }
        values[3] = a;
        for (int i = 0; i < values.Length; i++) {
          Test(values[i] == testvalues[i], testNum++);
        }
      }

      {
        Func<int[]> fn = MakeMethod<Func<int[]>>.Compile(parser, LexListGet(@"
          int[] TestNestedLocalDeclarations () 
          {
            int[] values = new int[6] ;
            int a = 2 ; 
            { 
              int b = 64 ; 
              values[0] = b ; 
              {
                int c = 256 ; 
                values[1] = c ; 
              }
              {
                int c = 1024 ; 
                values[4] = c ; 
              }
              {
                int c = 2222 ; 
                values[5] = c ; 
              }
              values[2] = b ; 
            }
            values[3] = a ; 
            return values ; 
          }"));

        int[] testvalues = fn();
        int[] values = new int[6];
        int a = 2;
        {
          int b = 64;
          values[0] = b;
          {
            int c = 256;
            values[1] = c;
          }
          {
            int c = 1024;
            values[4] = c;
          }
          {
            int c = 2222;
            values[5] = c;
          }
          values[2] = b;
        }
        values[3] = a;
        for (int i = 0; i < values.Length; i++) {
          Test(values[i] == testvalues[i], testNum++);
        }
      }

      {
        Func<int> fn = MakeMethod<Func<int>>.Compile(parser, LexListGet(@"
          int TestForEachArray () 
          {
            int tot = 0 ; 
            TestClass[] array = new TestClass[34] ; 
            for ( var i = 0 ; i<array.Length ; i++ ) {
              array[i] = new TestClass () ; 
              array[i].TestClassInt = i ; 
            }
            foreach ( var k in array ) tot = tot + k.TestClassInt ; 
            return tot ; 
          }"));

        int tot = 0;
        TestClass[] array = new TestClass[34];
        for (var i = 0; i < array.Length; i++) {
          array[i] = new TestClass();
          array[i].TestClassInt = i;
        }
        foreach (var k in array) tot = tot + k.TestClassInt;

        Test(fn() == tot, testNum++);
      }

      {
        Func<int> fn = MakeMethod<Func<int>>.Compile(parser, LexListGet(@"
          int TestForEachArray () 
          {
            int tot = 0 ; 
            int[] array = new int[34] ; 
            for ( var i = 0 ; i<array.Length ; i ++ ) array[i] = i ; 
            foreach ( var k in array ) tot = tot + k ; 
            return tot ; 
          }"));

        int tot = 0;
        int[] array = new int[34];
        for (var i = 0; i < array.Length; i++) array[i] = i;
        foreach (var k in array) tot = tot + k;

        Test(fn() == tot, testNum++);
      }

      {
        Func<string> fn = MakeMethod<Func<string>>.Compile(parser, LexListGet(@"
          string TestForEachArray () 
          {
            string tot = '' ; 
            string[] array = new string[34] ; 
            for ( var i = 0 ; i<array.Length ; i ++ ) array[i] = i.ToString() ; 
            foreach ( var k in array ) tot = tot + k ; 
            return tot ; 
          }"));

        string tot = "";
        string[] array = new string[34];
        for (var i = 0; i < array.Length; i++) array[i] = i.ToString();
        foreach (var k in array) tot = tot + k;

        Test(fn() == tot, testNum++);
      }


      {
        Action<TestClass, bool> action = MakeMethod<Action<TestClass, bool>>.Compile(parser, LexListGet(@"
          void TestReturnEarly (TestClass tc, bool flag)
          {
            tc.TestClassInt = 555 ; 
            if (flag) return ;
            tc.TestClassInt = 777 ; 
          }"));
        TestClass tc = new TestClass();
        action(tc, false);
        Test(tc.TestClassInt == 777, testNum++);
        tc = new TestClass();
        action(tc, true);
        Test(tc.TestClassInt == 555, testNum++);
      }

      {
        Func<int> fn = MakeMethod<Func<int>>.Compile(parser, LexListGet(@"
          int TestInt ()
          {
            int Number = 23 ;
            int test = ( Number + Number ) * Number ; 
            return test ; 
          }"));
        int number = fn();
        Test(fn() == (23 + 23) * 23, testNum++);
      }
      { // Number is the name of a static class somewhere in the system libraries.
        Func<int> fn = MakeMethod<Func<int>>.Compile(parser, LexListGet(@"
          int TestInt ()
          {
            int Number = 23 ;
            return Number ; 
          }"));
        int number = fn();
        Test(fn() == 23, testNum++);
      }


      {
        Func<bool, bool, bool, string> fn = MakeMethod<Func<bool, bool, bool, string>>.Compile(parser, LexListGet(@"
          string TestTernaryOp ( bool flag , bool flag2 ,  bool flag3 ) 
          {
            return flag ? ( flag2 ? 'Flag&Flag2' : 'Flag&!Flag2' ) : ( flag3 ? '!Flag&Flag3' : '!Flag&!Flag3' ) ; 
          }"));
        for (int i = 0; i <= 7; i++) {
          bool flag = (i & 1) == 1;
          bool flag2 = (i & 2) == 2;
          bool flag3 = (i & 4) == 4;
          Test(fn(flag, flag2, flag3) == (flag ? (flag2 ? "Flag&Flag2" : "Flag&!Flag2") : (flag3 ? "!Flag&Flag3" : "!Flag&!Flag3")), testNum++);
        }
      }
      {
        Func<bool, string> fn = MakeMethod<Func<bool, string>>.Compile(parser, LexListGet(@"
          string TestTernaryOp ( bool flag ) 
          {
            return flag ? 'TRUE' : 'FALSE' ; 
          }"));
        Test(fn(true) == "TRUE", testNum++);
        Test(fn(false) == "FALSE", testNum++);
      }
      {
        Func<Type> fn = MakeMethod<Func<Type>>.Compile(parser, LexListGet(@"
          Type TestTypeOf ( ) 
          {
            return typeof ( int ) ; 
          }"));
        Test(fn() == typeof(int), testNum++);
      }
      {
        Func<Type> fn = MakeMethod<Func<Type>>.Compile(parser, LexListGet(@"
          Type TestTypeOf ( ) 
          {
            return typeof ( HashSet<int> ) ; 
          }"));
        Test(fn() == typeof(HashSet<int>), testNum++);
      }

      {
        var s = "";
        Func<int> fn = MakeMethod<Func<int>>.Compile(parser, LexListGet(@"
          int TestForEach () 
          { 
            HashSet<TestInt> set = new HashSet<TestInt> () ;
            set.Add ( new TestInt(3) ) ;
            set.Add ( new TestInt(7) ) ;
            set.Add ( new TestInt(11) ) ;
            int summation = 0 ;
            foreach ( var j in set ) {
              summation = summation + j.IntValue ; 
            }
            return summation ; 
          }"));
        Test(fn() == 21, testNum++);
      }

      {
        var s = "";
        Func<int> fn = MakeMethod<Func<int>>.Compile(parser, LexListGet(@"
          int TestForEach () 
          { 
            HashSet<int> set = new HashSet<int> () ;
            set.Add ( 3 ) ;
            set.Add ( 7 ) ;
            set.Add ( 11 ) ; 
            int summation = 0 ;
            foreach ( var j in set ) {
              summation = summation + j ; 
            }
            return summation ; 
          }"));
        Test(fn() == 21, testNum++);
      }


      {
        var s = "";
        Func<int, TestClass[]> fn = MakeMethod<Func<int, TestClass[]>>.Compile(parser, LexListGet(@"
          TestClass[] MakeTestArray (int howMany) 
          { 
            TestClass[] temp = new TestClass[howMany] ;
            for ( int i = 0 ; i < howMany ; i ++ ) {
              temp[i] = new TestClass () ; 
              temp[i].TestClassInt = i*3 ; 
            }
            return temp ; 
          }"));
        Test(CompareTestClassArrays(fn(1), MakeTestClassArray(1)), testNum++);
        Test(CompareTestClassArrays(fn(3), MakeTestClassArray(3)), testNum++);
        Test(CompareTestClassArrays(fn(32), MakeTestClassArray(32)), testNum++);
        Test(CompareTestClassArrays(fn(333), MakeTestClassArray(333)), testNum++);
      }

      {
        var s = "";
        Func<int, int[]> fn = MakeMethod<Func<int, int[]>>.Compile(parser, LexListGet(@"
          int[] MakeTestArray (int howMany) 
          { 
            int[] temp = new int[howMany] ;
            for ( int i = 0 ; i < howMany ; i++ ) {
              temp[i] = i*2 ; 
            }
            return temp ; 
          }"));
        Test(CompareIntArrays(fn(1), MakeTestIntArray(1)), testNum++);
        Test(CompareIntArrays(fn(3), MakeTestIntArray(3)), testNum++);
        Test(CompareIntArrays(fn(32), MakeTestIntArray(32)), testNum++);
        Test(CompareIntArrays(fn(333), MakeTestIntArray(333)), testNum++);
      }

      {
        Func<TestClassExtension, string> fn;
        new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public string GetTheString () { return GetItA() ; }
            private string GetItA() { return GetIt() + TestClassInt + TestClassInt + `A`; }
            private string GetIt() { return TestClassString ; }
          }")).GetFunc<TestClassExtension, string>("GetTheString", out fn);
        TestClassExtension tce = new TestClassExtension();
        Test(fn(tce) == "TestClassExt66776677A", testNum++);
      }
      {
        Func<TestClassExtension, string> fn;
        new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public string GetTheString () { return GetItA() ; }
            private string GetItA() { return GetIt() + TestClassInt ; }
            private string GetIt() { return TestClassString ; }
          }")).GetFunc<TestClassExtension, string>("GetTheString", out fn);
        TestClassExtension tce = new TestClassExtension();
        Test(fn(tce) == "TestClassExt6677", testNum++);
      }
      {
        Func<TestClassExtension, string> fn;
        new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public string GetTheString () { return GetItA() ; }
            private string GetItA() { return GetIt() + TestClassInt.ToString() ; }
            private string GetIt() { return TestClassString ; }
          }")).GetFunc<TestClassExtension, string>("GetTheString", out fn);
        TestClassExtension tce = new TestClassExtension();
        Test(fn(tce) == "TestClassExt6677", testNum++);
      }
      {
        Func<TestClassExtension, string> fn;
        new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public string GetTheString () { return GetItA() ; }
            private string GetItA() { return GetIt() + (TestClassInt.ToString()) ; }
            private string GetIt() { return TestClassString ; }
          }")).GetFunc<TestClassExtension, string>("GetTheString", out fn);
        TestClassExtension tce = new TestClassExtension();
        Test(fn(tce) == "TestClassExt6677", testNum++);
      }
      {
        Func<TestClassExtension, string> fn;
        new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public string GetTheString () { return GetIt() ; }
            private string GetIt() { return TestClassString ; }
          }")).GetFunc<TestClassExtension, string>("GetTheString", out fn);
        TestClassExtension tce = new TestClassExtension();
        Test(fn(tce) == "TestClassExt", testNum++);
      }
      {
        Func<TestClassExtension, string> fn;
        new MakeClass(parser, LexListGet(@"
          partial class TestClassExtension 
          {
            public string GetTheString () { return TestClassString ; }
          }")).GetFunc<TestClassExtension, string>("GetTheString", out fn);
        TestClassExtension tce = new TestClassExtension();
        Test(fn(tce) == "TestClassExt", testNum++);
      }


      {
        var s = "";
        Func<int, string> fn = MakeMethod<Func<int, string>>.Compile(parser, LexListGet(@"
          string Test (int howMany) 
          { 
            string s = '' ;
            for ( int i = howMany ; i > 0 ; i -- ) {
              if (i == 2) continue ; 
              s = s + i.ToString() + `~` ;
              if (i == 7) break ; 
            }
            return s ; 
          }"));
        Test((s = fn(0)) == "", testNum++);
        Test((s = fn(1)) == "1~", testNum++);
        Test((s = fn(2)) == "1~", testNum++);
        Test((s = fn(3)) == "3~1~", testNum++);
        Test((s = fn(7)) == "7~", testNum++);
        Test((s = fn(8)) == "8~7~", testNum++);
        Test((s = fn(-1)) == "", testNum++);
      }
      {
        var s = "";
        Func<int, string> fn = MakeMethod<Func<int, string>>.Compile(parser, LexListGet(@"
          string Test (int howMany) 
          { 
            string s = '' ;
            for ( int i = howMany ; i > 0 ; i -- ) s = s + i.ToString() + `~` ;
            return s ; 
          }"));
        Test((s = fn(0)) == "", testNum++);
        Test((s = fn(1)) == "1~", testNum++);
        Test((s = fn(2)) == "2~1~", testNum++);
        Test((s = fn(3)) == "3~2~1~", testNum++);
        Test((s = fn(-1)) == "", testNum++);
      }

      {
        Func<int, string> fn = MakeMethod<Func<int, string>>.Compile(parser, LexListGet(@"
          string Test (int i) 
          { 
            string s = '' ;
            if (i == 0) {s = '0'; }
            else if (i == 1 ) s = '1' ; 
            else if (i == 2 ) s = '2' ; 
            return s ; 
          }"));
        Test(fn(0) == "0", testNum++);
        Test(fn(1) == "1", testNum++);
        Test(fn(2) == "2", testNum++);
        Test(fn(3) == "", testNum++);
        Test(fn(-1) == "", testNum++);
      }
      {
        Func<int, string> fn = MakeMethod<Func<int, string>>.Compile(parser, LexListGet(@"
          string Test (int i) 
          { 
            string s = '' ;
            if (i == 0) {s = '0'; }
            return s ; 
          }"));
        Test(fn(0) == "0", testNum++);
        Test(fn(1) == "", testNum++);
        Test(fn(2) == "", testNum++);
        Test(fn(3) == "", testNum++);
        Test(fn(-1) == "", testNum++);
      }

      {
        Func<int, string> fn = MakeMethod<Func<int, string>>.Compile(parser, LexListGet(@"
          string Test (int i) 
          { 
            string s = '' ;
            if (i == 0) {s = '0'; }
            else s = '3' ; 
            return s ; 
          }"));
        Test(fn(0) == "0", testNum++);
        Test(fn(1) == "3", testNum++);
        Test(fn(2) == "3", testNum++);
        Test(fn(3) == "3", testNum++);
        Test(fn(-1) == "3", testNum++);
      }

      {
        Func<int, string> fn = MakeMethod<Func<int, string>>.Compile(parser, LexListGet(@"
          string Test (int i) 
          { 
            string s = '' ;
            if (i == 0) {s = '0'; }
            else if (i == 1 ) s = '1' ; 
            else s = '3' ; 
            return s ; 
          }"));
        Test(fn(0) == "0", testNum++);
        Test(fn(1) == "1", testNum++);
        Test(fn(2) == "3", testNum++);
        Test(fn(3) == "3", testNum++);
        Test(fn(-1) == "3", testNum++);
      }


      {
        Func<int, string> fn = MakeMethod<Func<int, string>>.Compile(parser, LexListGet(@"
          string Test (int i) 
          { 
            string s = '';
            if (i == 0) {s = '0'; }
            else if (i == 1 ) s = '1' ; 
            else if (i == 2 ) s = '2' ; 
            else s = '3' ; 
            return s ; 
          }"));
        Test(fn(0) == "0", testNum++);
        Test(fn(1) == "1", testNum++);
        Test(fn(2) == "2", testNum++);
        Test(fn(3) == "3", testNum++);
        Test(fn(-1) == "3", testNum++);
      }
      {
        string ss = ((MakeMethod<Func<string>>.Compile(parser, LexListGet(@"
          string Test () 
          { 
            var s = '';  
            int i = 4 ; 
            while ( s.Length < 5 ) s = s + i.ToString() ; 
            return s ; }")))());
        Test(ss == "44444", testNum++);


        ss = ((MakeMethod<Func<string>>.Compile(parser, LexListGet(@"
                  string Test () 
                  { 
                    var s = '';  
                    int i = 5 ; 
                    while ( true ) {
                      if (i == 0) break ;  
                      i = i - 1 ;
                      if (i == 3) continue ;
                      s = s + i.ToString() ;
                    }
                    return s ; }")))());
        Test(ss == "4210", testNum++);


        ss = ((MakeMethod<Func<string>>.Compile(parser, LexListGet(@"
          string Test () 
          { 
            var s = '';  
            int i = 4 ; 
            while ( i >= 0 ) {
              s = s + i.ToString() ; 
              i = i - 1 ; 
            }
            return s ; }")))());
        Test(ss == "43210", testNum++);

        Func<int, string> fn1 = MakeMethod<Func<int, string>>.Compile(parser, LexListGet("string Test (int d) { return d.ToString() ; }"));
        string s1 = fn1(1803);
        Test(s1 == "1803", testNum++);

        fn1 = MakeMethod<Func<int, string>>.Compile(parser, LexListGet("string Test (int d) { return d.ToString('00000') ; }"));
        s1 = fn1(1803);
        Test(s1 == "01803", testNum++);


      }

      {
        sw.Stop(); TestClass tc = new TestClass(2); sw.Start();

        (MakeMethod<Action<int>>.Compile(parser, LexListGet("void Test (int i) { new TestClass ().SetTestStaticInt ( i ) ; }")))(789923);
        Test(TestClass.TestStaticInt == 789923, testNum++);

        (MakeMethod<Action<int>>.Compile(parser, LexListGet("void Test (int i) { TestClass.StaticSetTestStaticInt ( i ) ; }")))(7823);
        Test(TestClass.TestStaticInt == 7823, testNum++);


        Test(((MakeMethod<Func<TestClass>>.Compile(parser, LexListGet("TestClass Test () { var tc = new TestClass () ; return tc ; }")))()).GetType() == typeof(TestClass), testNum++);
        Test(((MakeMethod<Func<TestClass>>.Compile(parser, LexListGet("TestClass Test () { var tc = new TestClass ('TestingNewHere') ; return tc ; }")))()).Id == "TestingNewHere", testNum++);
        Test(((MakeMethod<Func<TestClass>>.Compile(parser, LexListGet("TestClass Test () { TestClassBase tc = new TestClass ('TestingNewHere') ; return (TestClass)(object)tc ; }")))()).Id == "TestingNewHere", testNum++);
        Test(((MakeMethod<Func<TestClass>>.Compile(parser, LexListGet("TestClass Test () { TestClassBase tc = new TestClass ('TestingNewHere') ; return (TestClass)((object)tc) ; }")))()).Id == "TestingNewHere", testNum++);
        Test(((TestClass)(((MakeMethod<Func<TestClassBase>>.Compile(parser, LexListGet("TestClassBase Test () { TestClassBase tc = new TestClass ('TestingNewHere') ; return tc ; }")))()))).Id == "TestingNewHere", testNum++);


        Test(((MakeMethod<Func<TestClass, int, int>>.Compile(parser, LexListGet("int Test (TestClass tc,int d) { tc.TestIntArray[3] = d ; return tc.TestIntArray[3] ; }")))(tc, 1123)) == tc.TestIntArray[3], testNum++);
        Test(((MakeMethod<Func<TestClass, int, int>>.Compile(parser, LexListGet("int Test (TestClass tc,int d) { tc.TestClassArray[2].TestIntArray[3] = d ; return tc.TestClassArray[2].TestIntArray[3] ; }")))(tc, 1123)) == tc.TestClassArray[2].TestIntArray[3], testNum++);


        Test(((MakeMethod<Func<int, int>>.Compile(parser, LexListGet("int Test (int d) { TestClass.TestStaticIntArray[3] = d ; return TestClass.TestStaticIntArray[3] ; }")))(1123)) == TestClass.TestStaticIntArray[3], testNum++);
        Test(1123 == TestClass.TestStaticIntArray[3], testNum++);

        TestClass.ForPropertyTest = 39933;
        (MakeMethod<Action<int>>.Compile(parser, LexListGet("void Test (int i) { TestClass.TestClassStaticPropertyInt = i ; }")))(98475);
        Test(TestClass.ForPropertyTest == 98475, testNum++);
        Test(((MakeMethod<Func<int>>.Compile(parser, LexListGet("int Test () { return TestClass.TestClassStaticPropertyInt ; }")))()) == 98475, testNum++);


        TestClass.TestClassStaticInt = 2987;
        Test(((MakeMethod<Func<int>>.Compile(parser, LexListGet("int Test () { return TestClass.TestClassStaticInt ; }")))()) == 2987, testNum++);

        Test(((MakeMethod<Func<TestClass, int, int>>.Compile(parser, LexListGet("int Test (TestClass tc,int d) { tc.TestProperty = d ; return tc.TestProperty ; }")))(tc, 1123)) == tc.PropertyValue, testNum++);
        Test(((MakeMethod<Func<TestClass, int, int>>.Compile(parser, LexListGet("int Test (TestClass tc,int d) { tc.TestProperty = d ; return tc.TestProperty ; }")))(tc, 1123)) == 1123, testNum++);
        Test(((MakeMethod<Func<TestClass, int, int>>.Compile(parser, LexListGet("int Test (TestClass tc, int d) { tc.TestClassInt = d*2 ; return tc.TestClassInt ; }")))(tc, 2345)) == tc.TestClassInt, testNum++);
      }

      {
        sw.Stop(); TestClass tc = new TestClass(); sw.Start();

        Test(((MakeMethod<Func<int, int>>.Compile(parser, LexListGet("int Test (int d) { int x = d ; x = x*2 ; return x ; }")))(2345)) == 2 * 2345, testNum++);
        Test(((MakeMethod<Func<int, int>>.Compile(parser, LexListGet("int Test (int d) { int x = d ; int y = d ; x = x*2 ; y = x*2 ; return x+y ; }")))(2345)) == 6 * 2345, testNum++);


        Test(((MakeMethod<Func<int, int>>.Compile(parser, LexListGet("int Test (int d) { var x = d ; x = x*2 ; return x ; }")))(2345)) == 2 * 2345, testNum++);
        Test(((MakeMethod<Func<int, int>>.Compile(parser, LexListGet("int Test (int d) { var x = d ; var y = d ; x = x*2 ; y = x*2 ; return x+y ; }")))(2345)) == 6 * 2345, testNum++);


        Test(((MakeMethod<Func<int, int>>.Compile(parser, LexListGet("int Test (int d) { var x = d ; return x ; }")))(2345)) == 2345, testNum++);
        Test(((MakeMethod<Func<int, int>>.Compile(parser, LexListGet("int Test (int d) { var x = d ; var y = x ; return 2*y ; }")))(2345)) == 2 * 2345, testNum++);
        Test(((MakeMethod<Func<TestClass, TestClass>>.Compile(parser, LexListGet("TestClass Test (TestClass d) { var x = d ; return x ; }")))(tc)) == tc, testNum++);
        Test(((MakeMethod<Func<TestClass, TestClass>>.Compile(parser, LexListGet("TestClass Test (TestClass d) { TestClass x = d ; return x ; }")))(tc)) == tc, testNum++);

        Test(((MakeMethod<Func<TestClass, TestClass>>.Compile(parser, LexListGet("TestClass Test (TestClass d) { object x = d ; TestClass t = (TestClass)x ; return t ; }")))(tc)) == tc, testNum++);
        Test(((MakeMethod<Func<TestClass, TestClassBase>>.Compile(parser, LexListGet("TestClassBase Test (TestClass d) { TestClassBase x = d ; return x ; }")))(tc)) == (TestClassBase)tc, testNum++);
        Test(((MakeMethod<Func<TestClass, TestClassBase>>.Compile(parser, LexListGet("TestClassBase Test (TestClass d) { object x = d ; return x ; }")))(tc)) == (TestClassBase)tc, testNum++);
        Test(((MakeMethod<Func<TestClass, TestClassBase>>.Compile(parser, LexListGet("TestClassBase Test (TestClass d) { object x = d ; return (TestClassBase)x ; }")))(tc)) == (TestClassBase)tc, testNum++);

      }


      {
        Test((double)((MakeMethod<Func<double, object>>.Compile(parser, LexListGet("object Test (double d) { return (object)d ; }")))(23.452112341234)) == 23.452112341234, testNum++);
        Test((float)((MakeMethod<Func<float, object>>.Compile(parser, LexListGet("object Test (float d) { return (object)d ; }")))(23.45F)) == 23.45F, testNum++);
        Test((long)((MakeMethod<Func<long, object>>.Compile(parser, LexListGet("object Test (long d) { return (object)d ; }")))(2345890111L)) == 2345890111L, testNum++);
        Test((int)((MakeMethod<Func<int, object>>.Compile(parser, LexListGet("object Test (int d) { return (object)d ; }")))(2345)) == 2345, testNum++);

        Test(((MakeMethod<Func<object, double>>.Compile(parser, LexListGet("double Test (object d) { return (double)d ; }")))((object)23.452112341234)) == 23.452112341234, testNum++);
        Test(((MakeMethod<Func<object, float>>.Compile(parser, LexListGet("float Test (object d) { return (float)d ; }")))((object)23.45F)) == 23.45F, testNum++);
        Test(((MakeMethod<Func<object, long>>.Compile(parser, LexListGet("long Test (object d) { return (long)d ; }")))((object)2345890111L)) == 2345890111L, testNum++);
        Test(((MakeMethod<Func<object, int>>.Compile(parser, LexListGet("int Test (object d) { return (int)d ; }")))((object)2345)) == 2345, testNum++);


        Test(((MakeMethod<Func<double, double>>.Compile(parser, LexListGet("double Test (double d) { return (double)d ; }")))(23.452112341234)) == (double)23.452112341234, testNum++);
        Test(((MakeMethod<Func<double, float>>.Compile(parser, LexListGet("float Test (double d) { return (float)d ; }")))(23.452112341234)) == (float)23.452112341234, testNum++);
        Test(((MakeMethod<Func<double, long>>.Compile(parser, LexListGet("long Test (double d) { return (long)d ; }")))(2345211234123.4)) == (long)2345211234123.4, testNum++);
        Test(((MakeMethod<Func<double, int>>.Compile(parser, LexListGet("int Test (double d) { return (int)d ; }")))(23.452112341234)) == (int)23.452112341234, testNum++);

        Test(((MakeMethod<Func<float, double>>.Compile(parser, LexListGet("double Test (float d) { return (double)d ; }")))(23.45F)) == (double)23.45F, testNum++);
        Test(((MakeMethod<Func<float, float>>.Compile(parser, LexListGet("float Test (float d) { return (float)d ; }")))(23.45F)) == (float)23.45F, testNum++);
        Test(((MakeMethod<Func<float, long>>.Compile(parser, LexListGet("long Test (float d) { return (long)d ; }")))(23.45F)) == (long)23.45F, testNum++);
        Test(((MakeMethod<Func<float, int>>.Compile(parser, LexListGet("int Test (float d) { return (int)d ; }")))(23.45F)) == (int)23.45F, testNum++);


        Test(((MakeMethod<Func<long, double>>.Compile(parser, LexListGet("double Test (long d) { return (long)d ; }")))(2345890111L)) == (double)2345890111L, testNum++);
        Test(((MakeMethod<Func<long, float>>.Compile(parser, LexListGet("float Test (long d) { return (long)d ; }")))(2345890111L)) == (float)2345890111L, testNum++);
        Test(((MakeMethod<Func<long, long>>.Compile(parser, LexListGet("long Test (long d) { return (long)d ; }")))(2345890111L)) == (long)2345890111L, testNum++);
        Test(((MakeMethod<Func<long, int>>.Compile(parser, LexListGet("int Test (long d) { return (long)d ; }")))(2345890111L)) == unchecked((int)2345890111L), testNum++);

        Test(((MakeMethod<Func<int, double>>.Compile(parser, LexListGet("double Test (int d) { return (int)d ; }")))(2345)) == (double)2345, testNum++);
        Test(((MakeMethod<Func<int, float>>.Compile(parser, LexListGet("float Test (int d) { return (int)d ; }")))(2345)) == (float)2345, testNum++);
        Test(((MakeMethod<Func<int, long>>.Compile(parser, LexListGet("long Test (int d) { return (int)d ; }")))(2345)) == (long)2345, testNum++);
        Test(((MakeMethod<Func<int, int>>.Compile(parser, LexListGet("int Test (int d) { return (int)d ; }")))(2345)) == (int)2345, testNum++);


      }


      {
        sw.Stop(); TestClass tc = new TestClass(); sw.Start();
        Test(((MakeMethod<Func<object, TestClass>>.Compile(parser, LexListGet("TestClass Test (object o) { return (TestClass)o ; }")))((object)tc)) == tc, testNum++);
        Test(((MakeMethod<Func<TestClass, object>>.Compile(parser, LexListGet("object Test (TestClass tc) { return (object)tc ; }")))(tc)) == (object)tc, testNum++);
        Test(((MakeMethod<Func<TestClass, object>>.Compile(parser, LexListGet("object Test (TestClass tc) { return (Object)tc ; }")))(tc)) == (object)tc, testNum++);
      }

      {
        Test(((MakeMethod<Func<double>>.Compile(parser, LexListGet("double Test () { return '23.45'.StrToDouble() ; }")))()) == "23.45".StrToDouble(), testNum++);
        Test(((MakeMethod<Func<long>>.Compile(parser, LexListGet("long Test () { return '2345'.StrToLong() ; }")))()) == "2345".StrToLong(), testNum++);
        Test(((MakeMethod<Func<int>>.Compile(parser, LexListGet("int Test () { return '2345'.StrToInt() ; }")))()) == "2345".StrToInt(), testNum++);
        //TestIsError(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.GenericInfoThreeA(123L, 'hello', 23.45, 22) ; }")))()) == TestClass.StaticTestClass.GenericInfoThreeA(123L, "hello", 23.45, 22), testNum++);
        //TestIsError(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.GenericInfoThree(123L, 'hello', 23.45, 22) ; }")))()) == TestClass.StaticTestClass.GenericInfoThree(123L, "hello", 23.45, 22), testNum++);
        //TestIsError(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.GenericInfoThreeA<long,string,double>(123L, 'hello', 23.45, 22) ; }")))()) == TestClass.StaticTestClass.GenericInfoThreeA<long, string, double>(123L, "hello", 23.45, 22), testNum++);
        //TestIsError(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.GenericInfoThree<long,double,string>(123L, 'hello', 23.45, 22) ; }")))()) == TestClass.StaticTestClass.GenericInfoThree<long, double, string>(123L, "hello", 23.45, 22), testNum++);

        //TestIsError(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.GenericInfo<long>(123L) ; }")))()) == TestClass.StaticTestClass.GenericInfo<long>(123L), testNum++);
        //TestIsError(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.GenericInfo(123L) ; }")))()) == TestClass.StaticTestClass.GenericInfo(123L), testNum++);
      }

      {
        int howMany = TestClass.HowMany;
        Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.NestedTestClass.Info ; }")))()) == TestClass.StaticTestClass.NestedTestClass.Info, testNum++);
        Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.TestClassArray[2].NestedTestClass.Info ; }")))()) ==
          TestClass.StaticTestClass.TestClassArray[2].NestedTestClass.Info, testNum++);
        Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.TestClassArray[2].TestClassArray[3].NestedTestClass.TestClassArray[0].NestedTestClass.Info ; }")))()) ==
          TestClass.StaticTestClass.TestClassArray[2].TestClassArray[3].NestedTestClass.TestClassArray[0].NestedTestClass.Info, testNum++);
        Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.TestClassArray[2].TestClassArray[3].NestedTestClass.NestedTestClass.NestedTestClass.TestClassArray[0].NestedTestClass.Info ; }")))()) ==
          TestClass.StaticTestClass.TestClassArray[2].TestClassArray[3].NestedTestClass.NestedTestClass.NestedTestClass.TestClassArray[0].NestedTestClass.Info, testNum++);

        Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.FetchNestedTestClass.Info ; }")))()) == TestClass.StaticTestClass.FetchNestedTestClass.Info, testNum++);
        Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.TestClassArray[2].FetchNestedTestClass.Info ; }")))()) ==
          TestClass.StaticTestClass.TestClassArray[2].FetchNestedTestClass.Info, testNum++);
        Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.TestClassArray[2].TestClassArray[3].FetchNestedTestClass.TestClassArray[0].FetchNestedTestClass.Info ; }")))()) ==
          TestClass.StaticTestClass.TestClassArray[2].TestClassArray[3].FetchNestedTestClass.TestClassArray[0].FetchNestedTestClass.Info, testNum++);
        Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.TestClassArray[2].TestClassArray[3].FetchNestedTestClass.NestedTestClass.FetchNestedTestClass.TestClassArray[0].FetchNestedTestClass.Info ; }")))()) ==
          TestClass.StaticTestClass.TestClassArray[2].TestClassArray[3].FetchNestedTestClass.NestedTestClass.FetchNestedTestClass.TestClassArray[0].FetchNestedTestClass.Info, testNum++);

        Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.GetTestClassArray(1).Info ; }")))()) == TestClass.StaticTestClass.GetTestClassArray(1).Info, testNum++);
        Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.TestClassArray[2].GetTestClassArray(1).Info ; }")))()) ==
          TestClass.StaticTestClass.TestClassArray[2].GetTestClassArray(1).Info, testNum++);
        Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.TestClassArray[2].TestClassArray[3].GetTestClassArray(1).TestClassArray[0].GetTestClassArray(1).Info ; }")))()) ==
          TestClass.StaticTestClass.TestClassArray[2].TestClassArray[3].GetTestClassArray(1).TestClassArray[0].GetTestClassArray(1).Info, testNum++);
        Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.TestClassArray[2].TestClassArray[3].GetTestClassArray(1).NestedTestClass.GetTestClassArray(1).TestClassArray[0].GetTestClassArray(1).Info ; }")))()) ==
          TestClass.StaticTestClass.TestClassArray[2].TestClassArray[3].GetTestClassArray(1).NestedTestClass.GetTestClassArray(1).TestClassArray[0].GetTestClassArray(1).Info, testNum++);

        Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.GetTestClassArray(1).Info ; }")))()) == TestClass.StaticTestClass.GetTestClassArray(1).Info, testNum++);
        Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.GetTestClassArray(2).GetTestClassArray(1).Info ; }")))()) ==
          TestClass.StaticTestClass.GetTestClassArray(2).GetTestClassArray(1).Info, testNum++);
        Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.GetTestClassArray(2).GetTestClassArray(3).GetTestClassArray(1).GetTestClassArray(0).GetTestClassArray(1).Info ; }")))()) ==
          TestClass.StaticTestClass.GetTestClassArray(2).GetTestClassArray(3).GetTestClassArray(1).GetTestClassArray(0).GetTestClassArray(1).Info, testNum++);
        Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("string Test () { return TestClass.StaticTestClass.GetTestClassArray(2).GetTestClassArray(3).GetTestClassArray(1).NestedTestClass.GetTestClassArray(1).GetTestClassArray(0).GetTestClassArray(1).Info ; }")))()) ==
          TestClass.StaticTestClass.GetTestClassArray(2).GetTestClassArray(3).GetTestClassArray(1).NestedTestClass.GetTestClassArray(1).GetTestClassArray(0).GetTestClassArray(1).Info, testNum++);
      }

      Test(((MakeMethod<Func<TestEnum>>.Compile(parser, LexListGet("TestEnum Test () { return TestEnum.One ; }")))()) == TestEnum.One, testNum++);
      Test(((MakeMethod<Func<TestEnum>>.Compile(parser, LexListGet("TestEnum Test () { return TestEnum.One | TestEnum.Two ; }")))()) == (TestEnum.One | TestEnum.Two), testNum++);
      Test(((MakeMethod<Func<bool>>.Compile(parser, LexListGet("bool Test () { return TestEnum.One == TestEnum.Two ; }")))()) == (TestEnum.One == TestEnum.Two), testNum++);
      Test(((MakeMethod<Func<bool>>.Compile(parser, LexListGet("bool Test () { return TestEnum.One == TestEnum.One ; }")))()) == (TestEnum.One == TestEnum.One), testNum++);
      Test(((MakeMethod<Func<TestEnum, bool>>.Compile(parser, LexListGet("bool Test (TestEnum t) { return t == TestEnum.Two ; }")))(TestEnum.One)) == (TestEnum.One == TestEnum.Two), testNum++);
      Test(((MakeMethod<Func<TestEnum, bool>>.Compile(parser, LexListGet("bool Test (TestEnum t) { return t == TestEnum.One ; }")))(TestEnum.One)) == (TestEnum.One == TestEnum.One), testNum++);
      Test(((MakeMethod<Func<TestEnum, bool>>.Compile(parser, LexListGet("bool Test (TestEnum t) { return t > TestEnum.Two ; }")))(TestEnum.One)) == (TestEnum.One > TestEnum.Two), testNum++);
      Test(((MakeMethod<Func<TestEnum, bool>>.Compile(parser, LexListGet("bool Test (TestEnum t) { return t > TestEnum.One ; }")))(TestEnum.One)) == (TestEnum.One > TestEnum.One), testNum++);
      Test(((MakeMethod<Func<TestEnum, bool>>.Compile(parser, LexListGet("bool Test (TestEnum t) { return t < TestEnum.Two ; }")))(TestEnum.One)) == (TestEnum.One < TestEnum.Two), testNum++);
      Test(((MakeMethod<Func<TestEnum, bool>>.Compile(parser, LexListGet("bool Test (TestEnum t) { return t < TestEnum.One ; }")))(TestEnum.One)) == (TestEnum.One < TestEnum.One), testNum++);


      Test(((MakeMethod<Func<TestClass, string, string>>.Compile(parser, LexListGet("string Test (TestClass tc, string s) { return tc[s] ; }")))(new TestClass(), "Brod")) == "Brods", testNum++);

      for (int i = 0; i < 9; i++) {
        Test(((MakeMethod<Func<string, int, char>>.Compile(parser, LexListGet("char Test (string s, int i) { return s[i] ; }")))("Helicopter", i)) == "Helicopter"[i], testNum++);
      }
      Test(((MakeMethod<Func<string, char>>.Compile(parser, LexListGet("char Test (string s) { return s[4] ; }")))("Helicopter")) == "Helicopter"[4], testNum++);

      Test(((MakeMethod<Func<int[], int>>.Compile(parser, LexListGet(" int Test ( int[] inArray ) { return inArray[5] ; }")))(new int[] { 3, 4, 5, 6, 7, 8, 9 }) == (8)), testNum++);
      Test(((MakeMethod<Func<string[], string>>.Compile(parser, LexListGet(" string Test ( string[] inArray ) { return inArray[5] ; }")))(new string[] { "3", "4", "5", "6", "7", "8", "9" }) == ("8")), testNum++);
      Test(((MakeMethod<Func<double[], double>>.Compile(parser, LexListGet(" double Test ( double[] inArray ) { return inArray[5] ; }")))(new double[] { 3D, 4D, 5D, 6D, 7D, 8D, 9D }) == (8D)), testNum++);

      {
        var TestStruct1 = new TestStruct(1);
        var TestStruct_Also1 = new TestStruct(1);
        var TestStruct2 = new TestStruct(2);
        Test(((MakeMethod<Func<TestStruct, TestStruct, bool>>.Compile(parser, LexListGet(" bool Test (TestStruct one, TestStruct two ) { return one.TestStructInt == two.TestStructInt ; }")))(TestStruct1, TestStruct1) == (TestStruct1.TestStructInt == TestStruct1.TestStructInt)), testNum++);
        Test(((MakeMethod<Func<TestStruct, TestStruct, bool>>.Compile(parser, LexListGet(" bool Test (TestStruct one, TestStruct two ) { return one.TestStructInt == two.TestStructInt ; }")))(TestStruct1, TestStruct_Also1) == (TestStruct1.TestStructInt == TestStruct_Also1.TestStructInt)), testNum++);
        Test(((MakeMethod<Func<TestStruct, TestStruct, bool>>.Compile(parser, LexListGet(" bool Test (TestStruct one, TestStruct two ) { return one.TestStructInt == two.TestStructInt ; }")))(TestStruct1, TestStruct2) == (TestStruct1.TestStructInt == TestStruct2.TestStructInt)), testNum++);
      }

      {
        var TempTestClass = new TestClass();
        var OtherTempTestClass = new TestClass();
        var TempTestClass2 = new TestClass2();
        Test(((MakeMethod<Func<object, object, bool>>.Compile(parser, LexListGet(" bool Test (object one, object two ) { return one == two ; }")))(TempTestClass, OtherTempTestClass) == (TempTestClass == OtherTempTestClass)), testNum++);
        Test(((MakeMethod<Func<object, object, bool>>.Compile(parser, LexListGet(" bool Test (object one, object two ) { return one == two ; }")))(TempTestClass, TempTestClass2) == ((object)TempTestClass == (object)TempTestClass2)), testNum++);
        Test(((MakeMethod<Func<TestClass, TestClass, bool>>.Compile(parser, LexListGet(" bool Test (TestClass one, TestClass two ) { return one == two ; }")))(TempTestClass, OtherTempTestClass) == (TempTestClass == OtherTempTestClass)), testNum++);

        Test(((MakeMethod<Func<object, object, bool>>.Compile(parser, LexListGet(" bool Test (object one, object two ) { return one != two ; }")))(TempTestClass, OtherTempTestClass) == (TempTestClass != OtherTempTestClass)), testNum++);
        Test(((MakeMethod<Func<object, object, bool>>.Compile(parser, LexListGet(" bool Test (object one, object two ) { return one != two ; }")))(TempTestClass, TempTestClass2) == ((object)TempTestClass != (object)TempTestClass2)), testNum++);
        Test(((MakeMethod<Func<TestClass, TestClass, bool>>.Compile(parser, LexListGet(" bool Test (TestClass one, TestClass two ) { return one != two ; }")))(TempTestClass, OtherTempTestClass) == (TempTestClass != OtherTempTestClass)), testNum++);

        Test(((MakeMethod<Func<TestClass[], TestClass>>.Compile(parser, LexListGet(" TestClass Test ( TestClass[] inArray ) { return inArray[5] ; }")))
          (new TestClass[] { new TestClass(), null, new TestClass(), new TestClass(), new TestClass(), TempTestClass, new TestClass() }) == (TempTestClass)), testNum++);
        Test(((MakeMethod<Func<TestClass[], TestClass>>.Compile(parser, LexListGet(" TestClass Test ( TestClass[] inArray ) { return inArray[4] ; }")))
          (new TestClass[] { new TestClass(), null, new TestClass(), new TestClass(), null, TempTestClass, new TestClass() }) == (null)), testNum++);
      }

      Test(((MakeMethod<Func<bool>>.Compile(parser, LexListGet(" bool Test ( ) { return true ; }")))() == (true)), testNum++);
      Test(((MakeMethod<Func<bool>>.Compile(parser, LexListGet(" bool Test ( ) { return false ; }")))() == (false)), testNum++);
      Test(((MakeMethod<Func<float>>.Compile(parser, LexListGet("  float Test ( ) { return 1234.5678F ; }")))() == (1234.5678F)), testNum++);
      Test(((MakeMethod<Func<float>>.Compile(parser, LexListGet("  float Test ( ) { return 1234.5678 ; }")))() == (1234.5678F)), testNum++);
      Test(((MakeMethod<Func<double>>.Compile(parser, LexListGet("  double Test ( ) { return 1234.5678 ; }")))() == (1234.5678)), testNum++);
      Test(((MakeMethod<Func<string>>.Compile(parser, LexListGet("  string Test ( ) { return 'StringTest' ; }")))() == ("StringTest")), testNum++);
      Test(((MakeMethod<Func<int>>.Compile(parser, LexListGet("  int Test ( ) { return 12345678 ; }")))() == (12345678)), testNum++);
      Test(((MakeMethod<Func<long>>.Compile(parser, LexListGet("  long Test ( ) { return 12345678 ; }")))() == (12345678L)), testNum++);
      Test(((MakeMethod<Func<long>>.Compile(parser, LexListGet("  long Test ( ) { return 1234567812345678L ; }")))() == (1234567812345678L)), testNum++);
      Test(((MakeMethod<Func<int>>.Compile(parser, LexListGet("  int Test ( ) { return 1234567812345678L ; }")))() == (unchecked((int)1234567812345678L))), testNum++);

      Test(((MakeMethod<Func<string, string, string>>.Compile(parser, LexListGet("  string Test ( string a, string b) { return a + b; }")))("One", "Two") == ("OneTwo")), testNum++);
      Test(((MakeMethod<Func<string, string, string>>.Compile(parser, LexListGet("  string Test ( string a, string b) { return a + b + a ; }")))("One", "Two") == ("OneTwoOne")), testNum++);
      Test(((MakeMethod<Func<string, string, string>>.Compile(parser, LexListGet("  string Test ( string a, string b) { return a + b + a + b; }")))("One", "Two") == ("OneTwoOneTwo")), testNum++);

      Func<int, string, int> intStrFn = null;
      Func<double, double, double> doubleFn = null;



      {
        long la, lb;
        int ia, ib;
        long sla, slb;
        int sia, sib;

        sla = 12341234098098;
        sia = 12345;
        slb = 12341234098098;
        sib = 12345;

        double da, db;
        float fa, fb;
        double sda, sdb;
        float sfa, sfb;

        sda = 12341234098098.0;
        sfa = 12345.0F;
        sdb = 12341234098098.0;
        sfb = 12345.0F;

        for (int a = -1; a <= 1; a++) {
          for (int b = -1; b <= 1; b++) {
            la = sla + 1;
            ia = sia + 1;
            lb = slb + 1;
            ib = sib + 1;
            da = sda + 1;
            fa = sfa + 1;
            db = sdb + 1;
            fb = sfb + 1;
            Test(((MakeMethod<Func<int, int, bool>>.Compile(parser, LexListGet("  bool Test ( int  b, int  c) { return b >  c; }")))(ia, ib) == (ia > ib)), testNum++);
            Test(((MakeMethod<Func<long, int, bool>>.Compile(parser, LexListGet(" bool Test ( long b, int  c) { return b >  c; }")))(la, ib) == (la > ib)), testNum++);
            Test(((MakeMethod<Func<int, long, bool>>.Compile(parser, LexListGet(" bool Test ( int  b, long c) { return b >  c; }")))(ia, lb) == (ia > lb)), testNum++);
            Test(((MakeMethod<Func<long, long, bool>>.Compile(parser, LexListGet("bool Test ( long b, long c) { return b >  c; }")))(la, lb) == (la > lb)), testNum++);

            Test(((MakeMethod<Func<float, float, bool>>.Compile(parser, LexListGet("  bool Test ( float  b, float  c) { return b >  c; }")))(fa, fb) == (fa > fb)), testNum++);
            Test(((MakeMethod<Func<double, float, bool>>.Compile(parser, LexListGet(" bool Test ( double b, float  c) { return b >  c; }")))(da, fb) == (da > fb)), testNum++);
            Test(((MakeMethod<Func<float, double, bool>>.Compile(parser, LexListGet(" bool Test ( float  b, double c) { return b >  c; }")))(fa, db) == (fa > db)), testNum++);
            Test(((MakeMethod<Func<double, double, bool>>.Compile(parser, LexListGet("bool Test ( double b, double c) { return b >  c; }")))(da, db) == (da > db)), testNum++);

            Test(((MakeMethod<Func<float, int, bool>>.Compile(parser, LexListGet("  bool Test ( float  b, int  c) { return b >  c; }")))(fa, ib) == (fa > ib)), testNum++);
            Test(((MakeMethod<Func<double, int, bool>>.Compile(parser, LexListGet(" bool Test ( double b, int  c) { return b >  c; }")))(da, ib) == (da > ib)), testNum++);
            Test(((MakeMethod<Func<float, long, bool>>.Compile(parser, LexListGet(" bool Test ( float  b, long c) { return b >  c; }")))(fa, lb) == (fa > lb)), testNum++);
            Test(((MakeMethod<Func<double, long, bool>>.Compile(parser, LexListGet("bool Test ( double b, long c) { return b >  c; }")))(da, lb) == (da > lb)), testNum++);

            Test(((MakeMethod<Func<int, float, bool>>.Compile(parser, LexListGet("  bool Test ( int    b, float  c) { return b >  c; }")))(ia, fb) == (ia > fb)), testNum++);
            Test(((MakeMethod<Func<long, float, bool>>.Compile(parser, LexListGet(" bool Test ( long   b, float  c) { return b >  c; }")))(la, fb) == (la > fb)), testNum++);
            Test(((MakeMethod<Func<int, double, bool>>.Compile(parser, LexListGet(" bool Test ( int    b, double c) { return b >  c; }")))(ia, db) == (ia > db)), testNum++);
            Test(((MakeMethod<Func<long, double, bool>>.Compile(parser, LexListGet("bool Test ( long   b, double c) { return b >  c; }")))(la, db) == (la > db)), testNum++);



            Test(((MakeMethod<Func<int, int, bool>>.Compile(parser, LexListGet("  bool Test ( int  b, int  c) { return b <  c; }")))(ia, ib) == (ia < ib)), testNum++);
            Test(((MakeMethod<Func<long, int, bool>>.Compile(parser, LexListGet(" bool Test ( long b, int  c) { return b <  c; }")))(la, ib) == (la < ib)), testNum++);
            Test(((MakeMethod<Func<int, long, bool>>.Compile(parser, LexListGet(" bool Test ( int  b, long c) { return b <  c; }")))(ia, lb) == (ia < lb)), testNum++);
            Test(((MakeMethod<Func<long, long, bool>>.Compile(parser, LexListGet("bool Test ( long b, long c) { return b <  c; }")))(la, lb) == (la < lb)), testNum++);

            Test(((MakeMethod<Func<float, float, bool>>.Compile(parser, LexListGet("  bool Test ( float  b, float  c) { return b <  c; }")))(fa, fb) == (fa < fb)), testNum++);
            Test(((MakeMethod<Func<double, float, bool>>.Compile(parser, LexListGet(" bool Test ( double b, float  c) { return b <  c; }")))(da, fb) == (da < fb)), testNum++);
            Test(((MakeMethod<Func<float, double, bool>>.Compile(parser, LexListGet(" bool Test ( float  b, double c) { return b <  c; }")))(fa, db) == (fa < db)), testNum++);
            Test(((MakeMethod<Func<double, double, bool>>.Compile(parser, LexListGet("bool Test ( double b, double c) { return b <  c; }")))(da, db) == (da < db)), testNum++);

            Test(((MakeMethod<Func<float, int, bool>>.Compile(parser, LexListGet("  bool Test ( float  b, int  c) { return b <  c; }")))(fa, ib) == (fa < ib)), testNum++);
            Test(((MakeMethod<Func<double, int, bool>>.Compile(parser, LexListGet(" bool Test ( double b, int  c) { return b <  c; }")))(da, ib) == (da < ib)), testNum++);
            Test(((MakeMethod<Func<float, long, bool>>.Compile(parser, LexListGet(" bool Test ( float  b, long c) { return b <  c; }")))(fa, lb) == (fa < lb)), testNum++);
            Test(((MakeMethod<Func<double, long, bool>>.Compile(parser, LexListGet("bool Test ( double b, long c) { return b <  c; }")))(da, lb) == (da < lb)), testNum++);

            Test(((MakeMethod<Func<int, float, bool>>.Compile(parser, LexListGet("  bool Test ( int    b, float  c) { return b <  c; }")))(ia, fb) == (ia < fb)), testNum++);
            Test(((MakeMethod<Func<long, float, bool>>.Compile(parser, LexListGet(" bool Test ( long   b, float  c) { return b <  c; }")))(la, fb) == (la < fb)), testNum++);
            Test(((MakeMethod<Func<int, double, bool>>.Compile(parser, LexListGet(" bool Test ( int    b, double c) { return b <  c; }")))(ia, db) == (ia < db)), testNum++);
            Test(((MakeMethod<Func<long, double, bool>>.Compile(parser, LexListGet("bool Test ( long   b, double c) { return b <  c; }")))(la, db) == (la < db)), testNum++);



            Test(((MakeMethod<Func<int, int, bool>>.Compile(parser, LexListGet("  bool Test ( int  b, int  c) { return b<= c; }")))(ia, ib) == (ia <= ib)), testNum++);
            Test(((MakeMethod<Func<long, int, bool>>.Compile(parser, LexListGet(" bool Test ( long b, int  c) { return b<= c; }")))(la, ib) == (la <= ib)), testNum++);
            Test(((MakeMethod<Func<int, long, bool>>.Compile(parser, LexListGet(" bool Test ( int  b, long c) { return b<= c; }")))(ia, lb) == (ia <= lb)), testNum++);
            Test(((MakeMethod<Func<long, long, bool>>.Compile(parser, LexListGet("bool Test ( long b, long c) { return b<= c; }")))(la, lb) == (la <= lb)), testNum++);

            Test(((MakeMethod<Func<float, float, bool>>.Compile(parser, LexListGet("  bool Test ( float  b, float  c) { return b<= c; }")))(fa, fb) == (fa <= fb)), testNum++);
            Test(((MakeMethod<Func<double, float, bool>>.Compile(parser, LexListGet(" bool Test ( double b, float  c) { return b<= c; }")))(da, fb) == (da <= fb)), testNum++);
            Test(((MakeMethod<Func<float, double, bool>>.Compile(parser, LexListGet(" bool Test ( float  b, double c) { return b<= c; }")))(fa, db) == (fa <= db)), testNum++);
            Test(((MakeMethod<Func<double, double, bool>>.Compile(parser, LexListGet("bool Test ( double b, double c) { return b<= c; }")))(da, db) == (da <= db)), testNum++);

            Test(((MakeMethod<Func<float, int, bool>>.Compile(parser, LexListGet("  bool Test ( float  b, int  c) { return b<= c; }")))(fa, ib) == (fa <= ib)), testNum++);
            Test(((MakeMethod<Func<double, int, bool>>.Compile(parser, LexListGet(" bool Test ( double b, int  c) { return b<= c; }")))(da, ib) == (da <= ib)), testNum++);
            Test(((MakeMethod<Func<float, long, bool>>.Compile(parser, LexListGet(" bool Test ( float  b, long c) { return b<= c; }")))(fa, lb) == (fa <= lb)), testNum++);
            Test(((MakeMethod<Func<double, long, bool>>.Compile(parser, LexListGet("bool Test ( double b, long c) { return b<= c; }")))(da, lb) == (da <= lb)), testNum++);

            Test(((MakeMethod<Func<int, float, bool>>.Compile(parser, LexListGet("  bool Test ( int    b, float  c) { return b<= c; }")))(ia, fb) == (ia <= fb)), testNum++);
            Test(((MakeMethod<Func<long, float, bool>>.Compile(parser, LexListGet(" bool Test ( long   b, float  c) { return b<= c; }")))(la, fb) == (la <= fb)), testNum++);
            Test(((MakeMethod<Func<int, double, bool>>.Compile(parser, LexListGet(" bool Test ( int    b, double c) { return b<= c; }")))(ia, db) == (ia <= db)), testNum++);
            Test(((MakeMethod<Func<long, double, bool>>.Compile(parser, LexListGet("bool Test ( long   b, double c) { return b<= c; }")))(la, db) == (la <= db)), testNum++);



            Test(((MakeMethod<Func<int, int, bool>>.Compile(parser, LexListGet("  bool Test ( int  b, int  c) { return b >=  c; }")))(ia, ib) == (ia >= ib)), testNum++);
            Test(((MakeMethod<Func<long, int, bool>>.Compile(parser, LexListGet(" bool Test ( long b, int  c) { return b >=  c; }")))(la, ib) == (la >= ib)), testNum++);
            Test(((MakeMethod<Func<int, long, bool>>.Compile(parser, LexListGet(" bool Test ( int  b, long c) { return b >=  c; }")))(ia, lb) == (ia >= lb)), testNum++);
            Test(((MakeMethod<Func<long, long, bool>>.Compile(parser, LexListGet("bool Test ( long b, long c) { return b >=  c; }")))(la, lb) == (la >= lb)), testNum++);

            Test(((MakeMethod<Func<float, float, bool>>.Compile(parser, LexListGet("  bool Test ( float  b, float  c) { return b >=  c; }")))(fa, fb) == (fa >= fb)), testNum++);
            Test(((MakeMethod<Func<double, float, bool>>.Compile(parser, LexListGet(" bool Test ( double b, float  c) { return b >=  c; }")))(da, fb) == (da >= fb)), testNum++);
            Test(((MakeMethod<Func<float, double, bool>>.Compile(parser, LexListGet(" bool Test ( float  b, double c) { return b >=  c; }")))(fa, db) == (fa >= db)), testNum++);
            Test(((MakeMethod<Func<double, double, bool>>.Compile(parser, LexListGet("bool Test ( double b, double c) { return b >=  c; }")))(da, db) == (da >= db)), testNum++);

            Test(((MakeMethod<Func<float, int, bool>>.Compile(parser, LexListGet("  bool Test ( float  b, int  c) { return b >=  c; }")))(fa, ib) == (fa >= ib)), testNum++);
            Test(((MakeMethod<Func<double, int, bool>>.Compile(parser, LexListGet(" bool Test ( double b, int  c) { return b >=  c; }")))(da, ib) == (da >= ib)), testNum++);
            Test(((MakeMethod<Func<float, long, bool>>.Compile(parser, LexListGet(" bool Test ( float  b, long c) { return b >=  c; }")))(fa, lb) == (fa >= lb)), testNum++);
            Test(((MakeMethod<Func<double, long, bool>>.Compile(parser, LexListGet("bool Test ( double b, long c) { return b >=  c; }")))(da, lb) == (da >= lb)), testNum++);

            Test(((MakeMethod<Func<int, float, bool>>.Compile(parser, LexListGet("  bool Test ( int    b, float  c) { return b >=  c; }")))(ia, fb) == (ia >= fb)), testNum++);
            Test(((MakeMethod<Func<long, float, bool>>.Compile(parser, LexListGet(" bool Test ( long   b, float  c) { return b >=  c; }")))(la, fb) == (la >= fb)), testNum++);
            Test(((MakeMethod<Func<int, double, bool>>.Compile(parser, LexListGet(" bool Test ( int    b, double c) { return b >=  c; }")))(ia, db) == (ia >= db)), testNum++);
            Test(((MakeMethod<Func<long, double, bool>>.Compile(parser, LexListGet("bool Test ( long   b, double c) { return b >=  c; }")))(la, db) == (la >= db)), testNum++);



            Test(((MakeMethod<Func<int, int, bool>>.Compile(parser, LexListGet("  bool Test ( int  b, int  c) { return b ==  c; }")))(ia, ib) == (ia == ib)), testNum++);
            Test(((MakeMethod<Func<long, int, bool>>.Compile(parser, LexListGet(" bool Test ( long b, int  c) { return b ==  c; }")))(la, ib) == (la == ib)), testNum++);
            Test(((MakeMethod<Func<int, long, bool>>.Compile(parser, LexListGet(" bool Test ( int  b, long c) { return b ==  c; }")))(ia, lb) == (ia == lb)), testNum++);
            Test(((MakeMethod<Func<long, long, bool>>.Compile(parser, LexListGet("bool Test ( long b, long c) { return b ==  c; }")))(la, lb) == (la == lb)), testNum++);

            Test(((MakeMethod<Func<float, float, bool>>.Compile(parser, LexListGet("  bool Test ( float  b, float  c) { return b ==  c; }")))(fa, fb) == (fa == fb)), testNum++);
            Test(((MakeMethod<Func<double, float, bool>>.Compile(parser, LexListGet(" bool Test ( double b, float  c) { return b ==  c; }")))(da, fb) == (da == fb)), testNum++);
            Test(((MakeMethod<Func<float, double, bool>>.Compile(parser, LexListGet(" bool Test ( float  b, double c) { return b ==  c; }")))(fa, db) == (fa == db)), testNum++);
            Test(((MakeMethod<Func<double, double, bool>>.Compile(parser, LexListGet("bool Test ( double b, double c) { return b ==  c; }")))(da, db) == (da == db)), testNum++);

            Test(((MakeMethod<Func<float, int, bool>>.Compile(parser, LexListGet("  bool Test ( float  b, int  c) { return b ==  c; }")))(fa, ib) == (fa == ib)), testNum++);
            Test(((MakeMethod<Func<double, int, bool>>.Compile(parser, LexListGet(" bool Test ( double b, int  c) { return b ==  c; }")))(da, ib) == (da == ib)), testNum++);
            Test(((MakeMethod<Func<float, long, bool>>.Compile(parser, LexListGet(" bool Test ( float  b, long c) { return b ==  c; }")))(fa, lb) == (fa == lb)), testNum++);
            Test(((MakeMethod<Func<double, long, bool>>.Compile(parser, LexListGet("bool Test ( double b, long c) { return b ==  c; }")))(da, lb) == (da == lb)), testNum++);

            Test(((MakeMethod<Func<int, float, bool>>.Compile(parser, LexListGet("  bool Test ( int    b, float  c) { return b ==  c; }")))(ia, fb) == (ia == fb)), testNum++);
            Test(((MakeMethod<Func<long, float, bool>>.Compile(parser, LexListGet(" bool Test ( long   b, float  c) { return b ==  c; }")))(la, fb) == (la == fb)), testNum++);
            Test(((MakeMethod<Func<int, double, bool>>.Compile(parser, LexListGet(" bool Test ( int    b, double c) { return b ==  c; }")))(ia, db) == (ia == db)), testNum++);
            Test(((MakeMethod<Func<long, double, bool>>.Compile(parser, LexListGet("bool Test ( long   b, double c) { return b ==  c; }")))(la, db) == (la == db)), testNum++);


            Test(((MakeMethod<Func<int, int, bool>>.Compile(parser, LexListGet("  bool Test ( int  b, int  c) { return b !=  c; }")))(ia, ib) == (ia != ib)), testNum++);
            Test(((MakeMethod<Func<long, int, bool>>.Compile(parser, LexListGet(" bool Test ( long b, int  c) { return b !=  c; }")))(la, ib) == (la != ib)), testNum++);
            Test(((MakeMethod<Func<int, long, bool>>.Compile(parser, LexListGet(" bool Test ( int  b, long c) { return b !=  c; }")))(ia, lb) == (ia != lb)), testNum++);
            Test(((MakeMethod<Func<long, long, bool>>.Compile(parser, LexListGet("bool Test ( long b, long c) { return b !=  c; }")))(la, lb) == (la != lb)), testNum++);

            Test(((MakeMethod<Func<float, float, bool>>.Compile(parser, LexListGet("  bool Test ( float  b, float  c) { return b !=  c; }")))(fa, fb) == (fa != fb)), testNum++);
            Test(((MakeMethod<Func<double, float, bool>>.Compile(parser, LexListGet(" bool Test ( double b, float  c) { return b !=  c; }")))(da, fb) == (da != fb)), testNum++);
            Test(((MakeMethod<Func<float, double, bool>>.Compile(parser, LexListGet(" bool Test ( float  b, double c) { return b !=  c; }")))(fa, db) == (fa != db)), testNum++);
            Test(((MakeMethod<Func<double, double, bool>>.Compile(parser, LexListGet("bool Test ( double b, double c) { return b !=  c; }")))(da, db) == (da != db)), testNum++);

            Test(((MakeMethod<Func<float, int, bool>>.Compile(parser, LexListGet("  bool Test ( float  b, int  c) { return b !=  c; }")))(fa, ib) == (fa != ib)), testNum++);
            Test(((MakeMethod<Func<double, int, bool>>.Compile(parser, LexListGet(" bool Test ( double b, int  c) { return b !=  c; }")))(da, ib) == (da != ib)), testNum++);
            Test(((MakeMethod<Func<float, long, bool>>.Compile(parser, LexListGet(" bool Test ( float  b, long c) { return b !=  c; }")))(fa, lb) == (fa != lb)), testNum++);
            Test(((MakeMethod<Func<double, long, bool>>.Compile(parser, LexListGet("bool Test ( double b, long c) { return b !=  c; }")))(da, lb) == (da != lb)), testNum++);

            Test(((MakeMethod<Func<int, float, bool>>.Compile(parser, LexListGet("  bool Test ( int    b, float  c) { return b !=  c; }")))(ia, fb) == (ia != fb)), testNum++);
            Test(((MakeMethod<Func<long, float, bool>>.Compile(parser, LexListGet(" bool Test ( long   b, float  c) { return b !=  c; }")))(la, fb) == (la != fb)), testNum++);
            Test(((MakeMethod<Func<int, double, bool>>.Compile(parser, LexListGet(" bool Test ( int    b, double c) { return b !=  c; }")))(ia, db) == (ia != db)), testNum++);
            Test(((MakeMethod<Func<long, double, bool>>.Compile(parser, LexListGet("bool Test ( long   b, double c) { return b !=  c; }")))(la, db) == (la != db)), testNum++);

          }
        }
      }

      {
        long a = 1123123412438;
        long b = 3989988223311;
        int c = 4499983;
        int d = 2398123;

        Test(((MakeMethod<Func<int, long>>.Compile(parser, LexListGet("long Test ( int c ) { return -c; }")))(1) == -1L), testNum++);
        Test(((MakeMethod<Func<long, long, int, int>>.Compile(parser, LexListGet("int Test ( long a , long b, int c ) { return a - b + c; }")))(a, b, c) == (int)(a - b + c)), testNum++);
        Test(((MakeMethod<Func<long, long, int, long>>.Compile(parser, LexListGet("long Test ( long a , long b, int c ) { return a + -b + c; }")))(a, b, c) == (a + -b + c)), testNum++);
        Test(((MakeMethod<Func<long, long, long, long>>.Compile(parser, LexListGet("long Test ( long a , long b, long c ) { return -a + b + -c; }")))(a, b, c) == (-a + b + -c)), testNum++);

        Test(((MakeMethod<Func<int, long>>.Compile(parser, LexListGet("long Test ( int c ) { return c; }")))(1) == 1L), testNum++);
        Test(((MakeMethod<Func<long, long, int, int>>.Compile(parser, LexListGet("int Test ( long a , long b, int c ) { return -a - b + c; }")))(a, b, c) == (int)(-a - b + c)), testNum++);
        Test(((MakeMethod<Func<long, long, int, long>>.Compile(parser, LexListGet("long Test ( long a , long b, int c ) { return a + ~ - b + c; }")))(a, b, c) == (a + ~-b + c)), testNum++);
        Test(((MakeMethod<Func<long, long, long, long>>.Compile(parser, LexListGet("long Test ( long a , long b, long c ) { return ~a + b + c; }")))(a, b, c) == (~a + b + c)), testNum++);

        Test(((MakeMethod<Func<int, long>>.Compile(parser, LexListGet("long Test ( int c ) { return c; }")))(1) == 1L), testNum++);
        Test(((MakeMethod<Func<long, long, int, int>>.Compile(parser, LexListGet("int Test ( long a , long b, int c ) { return - - a + b + c; }")))(a, b, c) == (int)(- -a + b + c)), testNum++);
        Test(((MakeMethod<Func<long, long, int, long>>.Compile(parser, LexListGet("long Test ( long a , long b, int c ) { return a + b + c; }")))(a, b, c) == (a + b + c)), testNum++);
        Test(((MakeMethod<Func<long, long, long, long>>.Compile(parser, LexListGet("long Test ( long a , long b, long c ) { return a + b + c; }")))(a, b, c) == (a + b + c)), testNum++);

        Test(((MakeMethod<Func<int, long>>.Compile(parser, LexListGet("long Test ( int c ) { return c; }")))(1) == 1L), testNum++);
        Test(((MakeMethod<Func<long, long, int, int>>.Compile(parser, LexListGet("int Test ( long a , long b, int c ) { return - ~ -a + ~ b + - ~ - c; }")))(a, b, c) == (int)(-~-a + ~b + -~-c)), testNum++);
        Test(((MakeMethod<Func<long, long, int, long>>.Compile(parser, LexListGet("long Test ( long a , long b, int c ) { return ~ - ~a + - ~ - b + c; }")))(a, b, c) == (~-~a + -~-b + c)), testNum++);
        Test(((MakeMethod<Func<long, long, long, long>>.Compile(parser, LexListGet("long Test ( long a , long b, long c ) { return ~-~-a + b + -~~-~c; }")))(a, b, c) == (~-~-a + b + -~~-~c)), testNum++);

        Test(((MakeMethod<Func<int, long>>.Compile(parser, LexListGet("long Test ( int c ) { return c; }")))(1) == 1L), testNum++);
        Test(((MakeMethod<Func<long, long, int, int>>.Compile(parser, LexListGet("int Test ( long a , long b, int c ) { return a + b + c; }")))(a, b, c) == (int)(a + b + c)), testNum++);
        Test(((MakeMethod<Func<long, long, int, long>>.Compile(parser, LexListGet("long Test ( long a , long b, int c ) { return a + b + c; }")))(a, b, c) == (a + b + c)), testNum++);
        Test(((MakeMethod<Func<long, long, long, long>>.Compile(parser, LexListGet("long Test ( long a , long b, long c ) { return a + b + c; }")))(a, b, c) == (a + b + c)), testNum++);
      }

      Test(((MakeMethod<Func<int, int, long, long>>.Compile(parser, LexListGet("long Test ( int a , int b, long c ) { return a | b | c; }")))(0, 1, 0) == (0 | 1 | 0)), testNum++);
      Test(((MakeMethod<Func<long, long, int, int>>.Compile(parser, LexListGet("int Test ( long a , long b, int c ) { return a & b & c; }")))(0, 0, 1) == (0 & 0 & 1)), testNum++);

      for (long a = 0; a < 4; a++) {
        for (long b = 0; b < 4; b++) {
          for (int c = 0; c < 4; c++) {
            Test(((MakeMethod<Func<long, long, int, int>>.Compile(parser, LexListGet("int Test ( long a , long b, int c ) { return ~a | ~b | ~c; }")))(a, b, c) == (~a | ~b | ~c)), testNum++);
            Test(((MakeMethod<Func<long, long, int, int>>.Compile(parser, LexListGet("int Test ( long a , long b, int c ) { return a & -~b & ~-c; }")))(a, b, c) == (a & -~b & ~-c)), testNum++);
            Test(((MakeMethod<Func<long, long, int, int>>.Compile(parser, LexListGet("int Test ( long a , long b, int c ) { return ~a ^ -b ^ ~-c; }")))(a, b, c) == (~a ^ -b ^ ~-c)), testNum++);
          }
        }
      }

      for (long a = 0; a < 4; a++) {
        for (long b = 0; b < 4; b++) {
          for (int c = 0; c < 4; c++) {
            Test(((MakeMethod<Func<long, long, int, int>>.Compile(parser, LexListGet("int Test ( long a , long b, int c ) { return a | b | c; }")))(a, b, c) == (a | b | c)), testNum++);
            Test(((MakeMethod<Func<long, long, int, int>>.Compile(parser, LexListGet("int Test ( long a , long b, int c ) { return a & b & c; }")))(a, b, c) == (a & b & c)), testNum++);
            Test(((MakeMethod<Func<long, long, int, int>>.Compile(parser, LexListGet("int Test ( long a , long b, int c ) { return a ^ b ^ c; }")))(a, b, c) == (a ^ b ^ c)), testNum++);
          }
        }
      }

      for (int a = 0; a < 4; a++) {
        for (int b = 0; b < 4; b++) {
          for (long c = 0; c < 4; c++) {
            Test(((MakeMethod<Func<int, int, long, long>>.Compile(parser, LexListGet("long Test ( int a , int b, long c ) { return a | b | c; }")))(a, b, c) == (a | b | c)), testNum++);
            Test(((MakeMethod<Func<int, int, long, long>>.Compile(parser, LexListGet("long Test ( int a , int b, long c ) { return a & b & c; }")))(a, b, c) == (a & b & c)), testNum++);
            Test(((MakeMethod<Func<int, int, long, long>>.Compile(parser, LexListGet("long Test ( int a , int b, long c ) { return a ^ b ^ c; }")))(a, b, c) == (a ^ b ^ c)), testNum++);
          }
        }
      }

      Test(((MakeMethod<Func<int, int, int>>.Compile(parser, LexListGet("int Test ( int a , int b ) { return a | b ; }")))(24, 28) == (24 | 28)), testNum++);
      Test(((MakeMethod<Func<int, int, int>>.Compile(parser, LexListGet("int Test ( int a , int b ) { return a & b ; }")))(24, 28) == (24 & 28)), testNum++);
      Test(((MakeMethod<Func<int, int, int>>.Compile(parser, LexListGet("int Test ( int a , int b ) { return a ^ b ; }")))(24, 28) == (24 ^ 28)), testNum++);

      for (int a = 0; a < 4; a++) {
        for (int b = 0; b < 4; b++) {
          for (int c = 0; c < 4; c++) {
            Test(((MakeMethod<Func<int, int, int, int>>.Compile(parser, LexListGet("int Test ( int a , int b, int c ) { return a | b | c; }")))(a, b, c) == (a | b | c)), testNum++);
            Test(((MakeMethod<Func<int, int, int, int>>.Compile(parser, LexListGet("int Test ( int a , int b, int c ) { return a & b & c; }")))(a, b, c) == (a & b & c)), testNum++);
            Test(((MakeMethod<Func<int, int, int, int>>.Compile(parser, LexListGet("int Test ( int a , int b, int c ) { return a ^ b ^ c; }")))(a, b, c) == (a ^ b ^ c)), testNum++);
          }
        }
      }

      for (int aa = 0; aa < 2; aa++) {
        for (int bb = 0; bb < 2; bb++) {
          for (int cc = 0; cc < 2; cc++) {
            bool a = aa == 1;
            bool b = bb == 1;
            bool c = cc == 1;
            Test(((MakeMethod<Func<bool, bool, bool, bool>>.Compile(parser, LexListGet("bool Test ( bool a , bool b, bool c ) { return a | !b | !c; }")))(a, b, c) == (a | !b | !c)), testNum++);
            Test(((MakeMethod<Func<bool, bool, bool, bool>>.Compile(parser, LexListGet("bool Test ( bool a , bool b, bool c ) { return !a & b & c; }")))(a, b, c) == (!a & b & c)), testNum++);
            Test(((MakeMethod<Func<bool, bool, bool, bool>>.Compile(parser, LexListGet("bool Test ( bool a , bool b, bool c ) { return !a ^ !!b ^ !c; }")))(a, b, c) == (!a ^ !!b ^ !c)), testNum++);
          }
        }
      }

      for (int aa = 0; aa < 2; aa++) {
        for (int bb = 0; bb < 2; bb++) {
          for (int cc = 0; cc < 2; cc++) {
            bool a = aa == 1;
            bool b = bb == 1;
            bool c = cc == 1;
            Test(((MakeMethod<Func<bool, bool, bool, bool>>.Compile(parser, LexListGet("bool Test ( bool a , bool b, bool c ) { return a | b | c; }")))(a, b, c) == (a | b | c)), testNum++);
            Test(((MakeMethod<Func<bool, bool, bool, bool>>.Compile(parser, LexListGet("bool Test ( bool a , bool b, bool c ) { return a & b & c; }")))(a, b, c) == (a & b & c)), testNum++);
            Test(((MakeMethod<Func<bool, bool, bool, bool>>.Compile(parser, LexListGet("bool Test ( bool a , bool b, bool c ) { return a ^ b ^ c; }")))(a, b, c) == (a ^ b ^ c)), testNum++);
          }
        }
      }

      Func<bool, bool, bool, bool, bool> boolsFn = MakeMethod<Func<bool, bool, bool, bool, bool>>.Compile(parser, LexListGet("bool Test (bool a, bool b, bool c, bool d) { return a || b || c || d ; }"));
      for (int a = 0; a < 16; a++) {
        Test(boolsFn(((a & 1) == 1), (((a >> 2) & 1) == 1), (((a >> 3) & 1) == 1), (((a >> 4) & 1) == 1)) == (((a & 1) == 1) || (((a >> 2) & 1) == 1) || (((a >> 3) & 1) == 1) || (((a >> 4) & 1) == 1)), testNum++);
      }
      boolsFn = MakeMethod<Func<bool, bool, bool, bool, bool>>.Compile(parser, LexListGet("bool Test (bool a, bool b, bool c, bool d) { return !a || !!b || !!!c || !!!!d ; }"));
      for (int a = 0; a < 16; a++) {
        Test(boolsFn(((a & 1) == 1), (((a >> 2) & 1) == 1), (((a >> 3) & 1) == 1), (((a >> 4) & 1) == 1)) == (!((a & 1) == 1) || !!(((a >> 2) & 1) == 1) || !!!(((a >> 3) & 1) == 1) || !!!!(((a >> 4) & 1) == 1)), testNum++);
      }

      boolsFn = MakeMethod<Func<bool, bool, bool, bool, bool>>.Compile(parser, LexListGet("bool Test (bool a, bool b, bool c, bool d) { return a && b && c && d ; }"));
      for (int a = 0; a < 16; a++) {
        Test(boolsFn(((a & 1) == 1), (((a >> 2) & 1) == 1), (((a >> 3) & 1) == 1), (((a >> 4) & 1) == 1)) == (((a & 1) == 1) && (((a >> 2) & 1) == 1) && (((a >> 3) & 1) == 1) && (((a >> 4) & 1) == 1)), testNum++);
      }

      Func<int, int, int> intTwoFn = MakeMethod<Func<int, int, int>>.Compile(parser, LexListGet("int Test ( int d1, int d2) { return d1 >> d2 ; }"));
      Test(intTwoFn(8000, 4) == (8000 >> 4), testNum++);

      Func<int, int> intOneFn = MakeMethod<Func<int, int>>.Compile(parser, LexListGet("int Test ( int d2) { return d2 >> 2 ; }"));
      Test(intOneFn(9000) == (9000 >> 2), testNum++);

      intTwoFn = MakeMethod<Func<int, int, int>>.Compile(parser, LexListGet("int Test ( int d1, int d2) { return d1 << d2 ; }"));
      Test(intTwoFn(16, 4) == (16 << 4), testNum++);

      intOneFn = MakeMethod<Func<int, int>>.Compile(parser, LexListGet("int Test ( int d2) { return d2 << 2 ; }"));
      Test(intOneFn(16) == 64, testNum++);

      intTwoFn = MakeMethod<Func<int, int, int>>.Compile(parser, LexListGet("int Test ( int d1, int d2) { return d1 + d2 ; }"));
      Test(intTwoFn(17, 23) == 40, testNum++);

      doubleFn = MakeMethod<Func<double, double, double>>.Compile(parser, LexListGet("double Test ( double d1, double d2) { return d1 + d2 ; }"));
      Test(doubleFn(23, 3.4) == 26.4, testNum++);

      doubleFn = MakeMethod<Func<double, double, double>>.Compile(parser, LexListGet("double Test ( double d1, double d2) { return d1 / d2 ; }"));
      Test(doubleFn(23, 3.4) == 23 / 3.4, testNum++);

      doubleFn = MakeMethod<Func<double, double, double>>.Compile(parser, LexListGet("double Test ( double d1, double d2) { return d1 % d2 ; }"));
      Test(doubleFn(23, 3.4) == 23 % 3.4, testNum++);

      intStrFn = MakeMethod<Func<int, string, int>>.Compile(parser, LexListGet("int Test ( int in1, string in2 ) { return in1 *3 + in1* 4 + 90 ; }"));
      Test(intStrFn(13, "hello") == 181, testNum++);

      intStrFn = MakeMethod<Func<int, string, int>>.Compile(parser, LexListGet("int Test ( int in1, string in2 ) { return in1 + in1 + 90 ; }"));
      Test(intStrFn(13, "hello") == 116, testNum++);

      intStrFn = MakeMethod<Func<int, string, int>>.Compile(parser, LexListGet("int Test ( int in1, string in2 ) { return in1 + 90 ; }"));
      Test(intStrFn(13, "hello") == 103, testNum++);

      intStrFn = MakeMethod<Func<int, string, int>>.Compile(parser, LexListGet("int Test ( int in1, string in2 ) { return in1 % in2.Length ; }"));
      Test(intStrFn(13, "hello") == 3, testNum++);

      intStrFn = MakeMethod<Func<int, string, int>>.Compile(parser, LexListGet("int Test ( int in1, string in2 ) { return in1 / in2.Length ; }"));
      Test(intStrFn(12, "hello") == 2, testNum++);

      intStrFn = MakeMethod<Func<int, string, int>>.Compile(parser, LexListGet("int Test ( int in1, string in2 ) { return in1 * in2.Length ; }"));
      Test(intStrFn(12, "hello") == 60, testNum++);

      intStrFn = MakeMethod<Func<int, string, int>>.Compile(parser, LexListGet("int Test ( int in1, string in2 ) { return in1 - in2.Length ; }"));
      Test(intStrFn(12, "hello") == 7, testNum++);

      intStrFn = MakeMethod<Func<int, string, int>>.Compile(parser, LexListGet("int Test ( int in1, string in2 ) { return in1 + in2.Length ; }"));
      Test(intStrFn(12, "hello") == 17, testNum++);

      TestClass.TestClassStaticInt = 34;
      Func<int> intFn = MakeMethod<Func<int>>.Compile(parser, LexListGet("int Test (  ) { return TestClass.TestClassStaticInt ; }"));
      Test(intFn() == 34, testNum++);

      intStrFn = MakeMethod<Func<int, string, int>>.Compile(parser, LexListGet("int Test ( int in1, string in2 ) { return in2.Length ; }"));
      Test(intStrFn(12, "hello") == 5, testNum++);

      intStrFn = MakeMethod<Func<int, string, int>>.Compile(parser, LexListGet("int Test ( int in1, string in2 ) { return in1 ; }"));
      Test(intStrFn(12, "hello") == 12, testNum++);

      sw.Stop();
      double timeEach = sw.ElapsedTicks * 1000.0 / (1.0 * Stopwatch.Frequency * (testNum - startTestNum));
      double timeEachExpression = TestTimer.ExpressionSW.ElapsedTicks * 1000.0 / (1.0 * Stopwatch.Frequency * (testNum - startTestNum));


      if (MakeMethodSB.Length == 0) {
        MessageBox.Show((testNum - startTestNum).ToString() + " Eval tests OK.\n" + timeEach.ToString("#.00") + "ms each.");
      } else {
        MessageBox.Show(MakeMethodSB.ToString());
      }
    }

    public static int Fib(int i)
    {
      if (i == 0) return 0;
      if (i == 1) return 1;
      return Fib(i - 1) + Fib(i - 2); 
    }

    public static int FibIterative (int n)
    {
      if (n <= 1) return n;
      int last = 1;
      int lastlast = 0;
      for (var i = 2; i <= n; i++) {
        int temp = lastlast + last;
        lastlast = last;
        last = temp;
      }
      return last;
    }

    public static long TestTiming(int n)
    {
      long total = 0;
      for (var i = 0; i < n; i++) {
        total += i * 1000 + i / 10 + 50 * i;
      }
      return total;
    }



    [STAThread]
    public static void Main()
    {
      LexToken.ShowError = (msg, theList) =>
      {
        new LexErrorDialog()
        {
          Message = msg,
          CompilerList = theList,
        }.Show();
      };

      TypeParser parser = new TypeParser(Assembly.GetExecutingAssembly(), new List<string>()
        {
          "System" ,
          "System.Collections.Generic" ,
          "System.Linq" ,
          "System.Text" ,
          "System.Windows" ,
          "System.Windows.Shapes" ,
          "System.Windows.Controls" ,
          "System.Windows.Media" ,
          "System.IO" ,
          "System.Reflection" ,
          "System.Text.RegularExpressions" ,
          "Kamimu"
        }
      );
      TypeParser.DefaultParser = parser; 


      Directory.CreateDirectory(@"C:\KamimuCodeTemp"); 
      Persist.ReadFromFile(@"C:\KamimuCodeTemp\CsharpEvalConfiguration.xml");


      try {
        MakeMethodTest(parser, 1000);
      } catch (Exception ex) {
        MessageBox.Show(ex.Message);
      }


      REPL.Show(parser);

      Persist.WriteToFile(); 

    }
  }
}
#endif
