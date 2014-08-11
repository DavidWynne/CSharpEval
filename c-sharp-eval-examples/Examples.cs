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
//FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWAR




/* To run this test program, set the conditional symbols 
 *   Class;Dialog
 * into the Build/Conditional Compilation Symbols edit box for this project.
 *
 * Then place this file into the same project as the other CSharpEval files and compile and run. 
 * 
 * From the Repl command line, invoke each test method, eg Examples.TestDoStatement() ; 
 */ 

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

  public class Examples
  {
    //Setting up error handling.
#if Dialog
    public static void ErrorHandlingUsingLexErrorDialog()
    {
      LexToken.ShowError = (msg, theList) =>
      {
        new LexErrorDialog()
        {
          Message = msg,
          CompilerList = theList,
        }.Show();
      };
      LexList.Get("MoreText '");  // will produce an error here.
    }
#endif

    public static void ErrorHandlingUsingMessageBox()
    {
      LexToken.ShowError = (msg, theList) =>
      {
        MessageBox.Show(
          msg + "\n" + theList.CodeFormat,
         "Error found");
      };
      LexList.Get("MoreText '");  // will produce an error here.
    }

    // Creating a TypeParser
    public static TypeParser parser; // Used by the examples

    public static void SetUpTypeParser()
    {
      parser = new TypeParser(
        Assembly.GetExecutingAssembly(),
        new List<string>()
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
          "Kamimu"
        }
      );

      TypeParser.DefaultParser = parser;
    }


    // Immediate compilation and execution of an expression or statement
    public static string AStr = "A string";

    public static void TestDoExpression()
    {
      SetUpTypeParser();
      Double d = MakeMethod.DoExpression<double>("23.45 / 32");
      if (d != (23.45 / 32)) MessageBox.Show("Error in TestDoExpression");

      string s2 = MakeMethod.DoExpression<string>("Examples.AStr + Examples.AStr ");
      if (s2 != "A stringA string") MessageBox.Show("Error in TestDoExpression");
    }

    public static void TestDoStatement()
    {
      SetUpTypeParser();
      MakeMethod.DoStatement("Examples.AStr = \"Some string contents\" ; ");
      if (AStr != "Some string contents") MessageBox.Show("Error in TestDoStatement");
    }

    public static int Counter;
    public static List<string> ColoursList = new List<string>() { 
      "red" , "green" , "red" , "orange" , 
      "yellow" , "red" , "green" };
    public static void TestDoStatement2()
    {
      SetUpTypeParser(); 
      MakeMethod.DoStatement(@"
        Examples.Counter = 0 ; 
        foreach ( var s in Examples.ColoursList ) {
          if (s == ""red"") Examples.Counter = Examples.Counter + 1 ; 
        }");
      //Note, I have only implemented the ++ and -- operator after a single identifier. X++ is ok, Y.X++ is not. ++X and ++X.Y and not implemented.
      if (Counter != 3) MessageBox.Show("Error in TestDoStatement2");
    }

    //Compiling a single method to produce a delegate

    public static void TestMakeMethod1()
    {
      SetUpTypeParser(); 
      Func<int, string> fn = MakeMethod<Func<int, string>>.
        Compile(parser, LexList.Get(@"
          public string TheMethod ( int i ) 
          {
            return i.ToString() ;
          }"
      ));

      string s = fn(123);
      if (s != "123") MessageBox.Show("Error in TestMakeMethod1");

      List<int> listOfIntegers = new List<int>() { 99, 120, 4, 134, 18, 19, 200 };
      List<string> list =
        (from i
         in listOfIntegers
         where i > 100
         select fn(i)
        ).ToList();
      if (list.Count != 3 ||
          list[0] != "120" ||
          list[1] != "134" ||
          list[2] != "200") MessageBox.Show("Error 2 in TestMakeMethod1"); 
    }

    public static string StrProperty { get ; set ; }  
    public static void TestMakeMethod2()
    {
      SetUpTypeParser(); 
      Action<int,double> act = MakeMethod<Action<int,double>>.
        Compile ( parser , LexList.Get ( @"
          public void TheMethod ( int i , double d ) 
          {
            if (Examples.StrProperty != null && Examples.StrProperty != """") {
              Examples.StrProperty = Examples.StrProperty + "","" ; 
            }
            Examples.StrProperty = Examples.StrProperty + i.ToString() + "","" + d.ToString() ; 
          }" 
        )) ;

      StrProperty = "" ; 
      act ( 1 , 23.4) ; 
      act ( 2 , 45.23) ; 
      act ( 99 , 1.23) ;
      if (StrProperty != "1,23.4,2,45.23,99,1.23") MessageBox.Show("Error in TestMakeMethod2"); 
    }

    //Different ways of generating a LexList

    public static void TestLexList1()
    {
      LexList lex1 = LexList.Get ( @"
         public str Convert ( int i ) 
         { 
           return i.ToString() ; 
         }" 
       ) ;

      string s1 = lex1.CodeFormatText(false); 

      string[] lines = new string[] { 
         "public str Convert ( int i )" , 
         "{" ,
         "  return i.ToString() ; ",
         "}" } ;
       LexList lex2 = LexList.Get ( lines ) ;

       string s3 = "public str Convert(int i)\r\n{\r\n  return i.ToString();\r\n}\r\n";

       string s2 = lex2.CodeFormatText(false);
       if (s1 != s2 || s3 != s1) {
         MessageBox.Show("Error in TesetLexList1");
       }

    }

    static public LexList MakeTheMethod2(string nameA)
    {
      LexListBuilder llb = new LexListBuilder();
      llb.Add("public int GetValue()");
      llb.Add("{ return Examples." + nameA + "; }");
      return llb.ToLexList();
    }


    public static void TestLexList2()
    {
      LexList lex1 = MakeTheMethod2( "A" ) ;
      LexList lex2 = LexList.Get(@"public int GetValue ()
        { return Examples.A ; }");
      string s1 = lex1.CodeFormatText(false);
      string s2 = lex2.CodeFormatText(false); 
      if (s1 != s2 || s1 != "public int GetValue()\r\n{\r\n  return Examples.A;\r\n}\r\n") MessageBox.Show("Error in TestLexList2"); 
    }



    static public LexList MakeTheMethod3 (string nameA)
    {
      LexListBuilder llb = new LexListBuilder();
      llb.Add("public int GetValue()");
      llb.Add("{ return Examples.'AA ; }",
        "AA", nameA);
      return llb.ToLexList();
    }

    public static void TestLexList3()
    {
      LexList lex1 = MakeTheMethod3("Var1");
      LexList lex2 = LexList.Get(@"public int GetValue ()
        { return Examples.Var1 ; }");
      string s1 = lex1.CodeFormatText(false);
      string s2 = lex2.CodeFormatText(false);
      if (s1 != s2 || s1 != "public int GetValue()\r\n{\r\n  return Examples.Var1;\r\n}\r\n") MessageBox.Show("Error in TestLexList2");
    }




    static public LexList MakeTheMethod4 ( 
      string TheName , string TheOtherName )
   { 
     LexListBuilder llb = new LexListBuilder () ; 
     llb.Add (
       @"public int GetValue () 
         { 
           return Alpha.'TheName + Alpha.'TheOtherName + Alpha.'TheName ; 
         }" , 
       "TheName" , TheName , 
       "TheOtherName" , TheOtherName ) ;
     return llb.ToLexList() ; 
   }

    public static void TestLexList4()
    {
      LexList lex1 = MakeTheMethod4("A" , "B" );
      LexList lex2 = LexList.Get(
        @"public int GetValue () { 
            return Alpha.A + Alpha.B + Alpha.A ; }");
      string s1 = lex1.CodeFormatText(false);
      string s2 = lex2.CodeFormatText(false);
      if (s1 != s2 || s1 != "public int GetValue()\r\n{\r\n  return Alpha.A+Alpha.B+Alpha.A;\r\n}\r\n") MessageBox.Show("Error in TestLexList2");
    }


    static public void TokenPasting1 ()
    {
      LexListBuilder llb = new LexListBuilder();
      llb.Add ( "return First'TheName ;" , "TheName" , "Value" ) ;
      string s = llb.ToLexList().CodeFormatText(false); 
      if (s != "return FirstValue;\r\n") MessageBox.Show("Error in TokenPasting1" ) ; 
    }

    static public void TokenPasting2()
    {
      LexListBuilder llb = new LexListBuilder();
      llb.Add("return 'TheName''Last ; ", "TheName", "Value");
      string s = llb.ToLexList().CodeFormatText(false);
      if (s != "return ValueLast;\r\n") MessageBox.Show("Error in TokenPasting2");
    }

    static public void TokenPasting3()
    {
      LexListBuilder llb = new LexListBuilder();
      llb.Add("return 'NameOne'NameTwo ; ", "NameOne", "One", "NameTwo", "Two");
      string s = llb.ToLexList().CodeFormatText(false);
      if (s != "return OneTwo;\r\n") MessageBox.Show("Error in TokenPasting3");
    }

    static public void TokenPasting4()
    {
      LexListBuilder llb = new LexListBuilder();
      llb.Add("return First'NameOne'NameTwo'NameThree''Last ; ",
         "NameOne", "One",
         "NameTwo", "Two",
         "NameThree", "Three");
      string s = llb.ToLexList().CodeFormatText(false);
      if (s != "return FirstOneTwoThreeLast;\r\n") MessageBox.Show("Error in TokenPasting4");
    }


    public class MyClassType
    {
      public int TheInt;
    }

    static public void TokenPasting5()
    {
      LexListBuilder llb = new LexListBuilder();
      llb.Add ( "return typeof(`Type) ;" , "Type" , typeof(MyClassType) ) ;
      LexList lex = llb.ToLexList() ; 
      string s = lex.CodeFormatText(false);
      if (s != "return typeof(MyClassType);\r\n" || lex[3].ActualObject != typeof(MyClassType)) MessageBox.Show("Error in TokenPasting5");
    }

    static public void QuotePromotion()
    {
      string ActualValue = "VarOne"; 
      LexListBuilder llb = new LexListBuilder();
      llb.AddAndPromoteQuotes(
        "string s = 'The string contents' + `Variable ; ",
        "Variable", ActualValue);
      llb.AddAndPromoteQuotes(
        "string anotherStr = 'Another string contents' ;");
      llb.AddAndPromoteQuotes("char ch = `c` ; ");
      LexList lex = llb.ToLexList();
      string s = lex.CodeFormatText(false);
      if (s != "string s=\"The string contents\"+VarOne;\r\nstring anotherStr=\"Another string contents\";\r\nchar ch='c';\r\n") MessageBox.Show("Error in QuotePromotion"); 
    }

    //Comiling methods and fields and attaching them to a class instance

    public class TestClass
    {
      public int TheInt;
      public string TheString;
      public int GetTheInt()
      {
        return TheInt;
      }
      public List<object> Fields; 
    }

    static public void MakeClass1()
    {
      SetUpTypeParser();
      MakeClass mc = new MakeClass(parser, LexList.Get(@"
       partial class Examples.TestClass   
       {
         public int GetTwoTimesTheInt () 
         {
           return TheInt * 2 ;
         }

         public void SetTheString ( string s )
         {
           TheString = s ; 
         }
       }"));
      Func<TestClass, int> GetTwoTimesTheInt;
      Action<TestClass, string> SetTheString;

      mc.GetFunc<TestClass, int>(
        "GetTwoTimesTheInt", out GetTwoTimesTheInt);
      mc.GetAction<TestClass, string>(
        "SetTheString", out SetTheString);

      TestClass tc = new TestClass();
      tc.TheInt = 34;
      int i = GetTwoTimesTheInt(tc);
      if (i != 68) MessageBox.Show("MakeClass1 error 1");
      SetTheString(tc, "New string value");
      if (tc.TheString != "New string value") MessageBox.Show("MakeClass1 error 2");
    }

    static public void MakeClass2()
    {
      SetUpTypeParser();
      Func<TestClass, int> GetTwoTimesTheInt;
      Action<TestClass, string> SetTheString;
      MakeClass mc = new MakeClass(parser, LexList.Get(@"
       partial class Examples.TestClass   
       {
         public int GetTwoTimesTheInt () 
         {
           return TheInt * 2 ;
         }

         public void SetTheString ( string s )
         {
           TheString = s ; 
         }
       }")).
      GetFunc<TestClass, int>(
        "GetTwoTimesTheInt", out GetTwoTimesTheInt).
      GetAction<TestClass, string>(
        "SetTheString", out SetTheString);

      TestClass tc = new TestClass();
      tc.TheInt = 34;
      int i = GetTwoTimesTheInt(tc);
      if (i != 68) MessageBox.Show("MakeClass2 error 1");
      SetTheString(tc, "New string value");
      if (tc.TheString != "New string value") MessageBox.Show("MakeClass2 error 2");
    }

    static public void MakeClass3()
    {
      SetUpTypeParser();
      Func<TestClass, int> GetTwoTimesTheInt;
      Action<TestClass, string> SetTheString;
      MakeClass mc = new MakeClass(parser, LexList.Get(@"
       partial class Examples.TestClass   
       {
         public int GetTwoTimesTheInt () 
         {
           return TheInt * 2 ;
         }

         public void SetTheString ( string s )
         {
           TheString = s ; 
         }
       }")).
      GetFunc<TestClass, int>(
        "GetTwoTimesTheInt", out GetTwoTimesTheInt).
      GetAction<TestClass, string>(
        "SetTheString", out SetTheString);

      TestClass tc = new TestClass();
      tc.TheInt = 34;
      int i = GetTwoTimesTheInt(tc);
      if (i != 68) MessageBox.Show("MakeClass1 error 1");
      SetTheString(tc, "New string value");
      if (tc.TheString != "New string value") MessageBox.Show("MakeClass3 error 2");


      Action<TestClass, string, int> SetStringAndInt;
      mc.AddMethodsAndFields(LexList.Get(@"
        partial class Examples.TestClass   
        {
          public void SetStringAndInt ( string s , int i ) 
          {
            TheInt = i ;
            SetTheString ( s ) ; 
          }
        }"), true).
       GetAction<TestClass, string, int>(
         "SetStringAndInt", out SetStringAndInt);

      SetStringAndInt(tc, "Hello", 777);
      if (tc.TheString != "Hello" || tc.TheInt != 777) MessageBox.Show("MakeClass3 error 3"); 


      mc.AddMethodsAndFields(LexList.Get(@"
         partial class Examples.TestClass   
         {
           public void SetStringAndInt ( string s , int i ) 
           {
             TheInt = i * 100 ;
             SetTheString ( s ) ; 
           }
         }"), true);

      SetStringAndInt(tc, "Goodbye", 11);
      if (tc.TheString != "Goodbye" || tc.TheInt != 1100) MessageBox.Show("MakeClass3 error 4"); 

    }

    static public void MakeClass4()
    {
      SetUpTypeParser();
      Func<TestClass, int> GetIntValue;
      Action<TestClass> Init;
      MakeClass mc = new MakeClass(parser, LexList.Get(@"
       partial class Examples.TestClass   
       {
         public int LastIntValue  ; 
         public int GetIntValue ()  
         {
           LastIntValue = TheInt ; 
           return TheInt ; 
         }
       }")).
       GetFunc<TestClass, int>("GetIntValue", out GetIntValue).
       GetAction<TestClass>("FieldsInitialiser", out Init);

      TestClass tc1 = new TestClass();
      tc1.TheInt = 22;
      Init(tc1);
      int i = GetIntValue(tc1);
      // i is 22 and so is LastIntValue
      if (i != 22 || tc1.Fields == null || tc1.Fields.Count < 1 || tc1.Fields[0] == null || !(tc1.Fields[0] is int) || ((int)(tc1.Fields[0])) != 22) {
        MessageBox.Show("Error 1 in makeClass4 ");
      }
      tc1.TheInt = 33;
      int j = GetIntValue(tc1);
      // j is 33 and so is LastIntValue
      if (j != 33 || tc1.Fields == null || tc1.Fields.Count < 1 || tc1.Fields[0] == null || !(tc1.Fields[0] is int) || ((int)(tc1.Fields[0])) != 33) {
        MessageBox.Show("Error 2 in makeClass4 ");
      }
      TestClass tc2 = new TestClass();
      Init(tc2);
      tc2.TheInt = 100;
      int k = GetIntValue(tc2);
      // k is 100 and so is LastIntValue
      if (k != 100 || tc2.Fields == null || tc2.Fields.Count < 1 || tc2.Fields[0] == null || !(tc2.Fields[0] is int) || ((int)(tc2.Fields[0])) != 100) {
        MessageBox.Show("Error 3 in makeClass4 ");
      }

      Action<TestClass, string> AddString;
      mc.AddMethodsAndFields(LexList.Get(@"
        partial class Examples.TestClass 
        {
          public List<string> ListOfStrings ; 
          public void AddString ( string s ) 
          { 
            if (ListOfStrings == null) 
              ListOfStrings = new List<string> () ; 
            ListOfStrings.Add ( s ) ; 
          }
        }"), true).
       GetAction<TestClass, string>("AddString", out AddString);

      Init(tc1);
      AddString(tc1, "String One");
      AddString(tc1, "String Two");
      if (tc1.Fields == null || tc1.Fields.Count < 2 || tc1.Fields[1] == null || !(tc1.Fields[1] is List<string>) || 
        ((List<string>)(tc1.Fields[1])).Count != 2 ||
        ((List<string>)(tc1.Fields[1]))[0] != "String One" ||
        ((List<string>)(tc1.Fields[1]))[1] != "String Two" 
        ) {
        MessageBox.Show("Error 4 in makeClass4 ");
      }
    }
  }
}
