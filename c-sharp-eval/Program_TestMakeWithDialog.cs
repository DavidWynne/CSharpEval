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
 *   Dialog
 * into the Build/Conditional Compilation Symbols edit box for this project.
 *
 * To demonstrate the error dialog box, put in some syntax error in the 'partial class TestClass' code.
 */

#if !Class && Dialog && !Test && !Repl

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
    static LexList LexListGet(string s)
    {
      LexListBuilder lb = new LexListBuilder();
      lb.Add(s);
      return lb.ToLexList(); 
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
          "Kamimu"
        }
      );
      TypeParser.DefaultParser = parser; 



      Directory.CreateDirectory(@"C:\KamimuCodeTemp"); 
      Persist.ReadFromFile(@"C:\KamimuCodeTemp\CsharpEvalConfiguration.xml");



      try {
        {
          var s = "";
          Func<int, string> fn = MakeMethod<Func<int, string>>.Compile(parser, LexListGet(@"
          string Test (int howMany) 
          { 
            string s = '' ;
            for ( int i = howMany ; i > 0 ; i -- ) s = s + i.ToString() + `~` ;
            return s ; 
          }"));
          bool error = false; 
          if (fn(0) != "") error = true ; 
          if (fn(1) != "1~") error = true ;
          if (fn(2) != "2~1~" ) error = true ;
          if (fn(3) != "3~2~1~") error = true ;
          if (fn(-1) != "") error = true ;
          if (error) MessageBox.Show("There was an error", "Test Make with dialog"); else MessageBox.Show("Ran OK", "Test Make with dialog");
        }
      } catch (Exception ex) {
        MessageBox.Show("There was a compilation or execution error.", "Test Make with dialog");
      }

      Persist.WriteToFile();
    }
  }
}
#endif
