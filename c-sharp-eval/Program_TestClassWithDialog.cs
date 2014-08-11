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
 * To demonstrate the error dialog box, put in some syntax error in the 'partial class TestClass' code.
 */ 

#if Class && Dialog && !Test && !Repl

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
  public class TestClass
  {
    public List<object> Fields;
  }

  public static class TheProgram
  {

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
          "Kamimu"
        }
      );
      TypeParser.DefaultParser = parser; 



      Directory.CreateDirectory(@"C:\KamimuCodeTemp"); 
      Persist.ReadFromFile(@"C:\KamimuCodeTemp\CsharpEvalConfiguration.xml");

      try {
        {
          Func<TestClass, int> getLength;
          Action<TestClass, int[]> setArray;
          Action<TestClass> actInit;
          MakeClass mc = new MakeClass(parser, LexList.Get(@"
          partial class TestClass 
          {
            public int[] LocalInt ;
            public void SetArray ( int[] input ) 
            {
              LocalInt = input ; 
            }   
            public int GetLength () { return LocalInt.Length ; }
          }")).
              GetFunc<TestClass, int>("GetLength", out getLength).
              GetAction<TestClass, int[]>("SetArray", out setArray).
              GetAction<TestClass>("FieldsInitialiser", out actInit);
          TestClass tc = new TestClass();
          actInit(tc);
          int[] thearray = new int[300];
          setArray(tc, thearray);
          if (getLength(tc) != thearray.Length) MessageBox.Show("There was an error", "Test class with dialog"); else MessageBox.Show("Ran OK", "Test class with dialog");
        }
      } catch (Exception ex) {
        MessageBox.Show("There was a compilation or execution error.", "Test class with dialog");
      }

      Persist.WriteToFile();
    }
  }
}
#endif
