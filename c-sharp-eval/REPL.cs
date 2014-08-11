//Copyright (C) 2009-2010 David Wynne.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
//files (the 'Software'), to deal in the Software without restriction, including without limitation the rights to use,
//copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
//Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED 'AS IS', WITHOUT WARRANTY OF ANY KIND,EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
//OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
//HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
//FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#if Repl

using System.Text;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Diagnostics;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows;
using System.IO;
using System.Text.RegularExpressions;

namespace Kamimu
{

  public class REPLData
  {
    public enum Kind { User, Reply, Error, FirstGap, MiddleGap, LastGap } // The Gap enums must be last, with FirstGap the lowest, because of the following method.
    public static bool KindIsGap(Kind kind) { return kind >= Kind.FirstGap; }

    public class Packet
    {
      public List<string> User = new List<string>();
      public List<string> Reply = new List<string>();
      public Kind ReplyKind;
    }

    public List<Packet> Packets = new List<Packet>() ;
  }

  public class REPL
  {
    public REPLData DisplayData ;
    public TypeParser Parser;
    public MakeClass Maker ;
    public bool StopwatchOn;
    public REPLRenderingPanel ReplPanel; 
    public Stopwatch TheStopwatch = new Stopwatch() ;
    public List<object> Fields;

    private REPL(TypeParser parser) 
    {
      DisplayData = new REPLData()
      {
        Packets = {
          new REPLData.Packet () { User = { "" } } 
        }
      };
      Parser = parser;
      Maker = new MakeClass(Parser); 
    }

    public string SaveCode ( string filename ) 
    {
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("partial class REPL {");
      for (int phase = 0; phase <= 1; phase++) {
        foreach (var pac in DisplayData.Packets) {
          switch (ExamineUserInput(pac.User.AsReadOnly())) {
          case PacketAction.Method:
            if (phase == 1) foreach (string s in pac.User) sb.AppendLine(s);
            break;
          case PacketAction.Field:
            if (phase == 0) foreach (string s in pac.User) sb.AppendLine(s);
            break;
          }
        }
      }
      sb.AppendLine("}");

      //LexList ll = LexList.Get(sb.ToString());
      //string outputString = ll.CodeFormatText(false);  
      string outputString = sb.ToString(); 

      try {
        using (StreamWriter sw = new StreamWriter(CodeTextFileName(filename))) {
          sw.WriteLine(outputString); 
        }
      } catch ( Exception ex ) {
        return "Failed: " + ex.Message ; 
      }
      Line ( "Saved to '" + CodeTextFileName(filename) + "'" ) ;
      return "";
    }

    private void WriteCodeTextLine(StreamWriter sw, string line)
    {
      if (line.Length > 0 && line[0] == '=') sw.Write('=');
      sw.WriteLine(line); 
    }

    public string SaveImage(string filename)
    {
      try {
        using (StreamWriter sw = new StreamWriter(CodeTextFileName(filename))) {
          foreach (var pac in DisplayData.Packets) {
            foreach (string line in pac.User) WriteCodeTextLine(sw, line);
            sw.WriteLine("=");
            sw.WriteLine(pac.ReplyKind.ToString()); 
            foreach (string line in pac.Reply) WriteCodeTextLine(sw, line);
            sw.WriteLine("="); 
          }
        }
      } catch ( Exception ex ) {
        return "Failed: " + ex.Message ; 
      }
      Line("Saved to '" + CodeTextFileName(filename) + "'");
      return ""; // "Saved to '" + CodeTextFileName(filename) + "'";
    }

    private static string CodeTextFileName(string filename)
    {
      if (filename == "") filename = "REPLSavedFile" ; 
      string fn = Path.GetDirectoryName(Persist.FileName) + "\\" + filename + ".cs" ;
      return fn; 
    }

    public string RestoreImage(string filename)
    {
      LexListBuilder allMethodsAndFields = new LexListBuilder();
      allMethodsAndFields.Add("partial class `Type {", "Type", typeof(REPL));
      if (filename == "") {
        foreach (var s in Directory.GetFiles(Path.GetDirectoryName(Persist.FileName), "*.cs")) Line(Path.GetFileNameWithoutExtension(s));
        return "" ; 
      } else {
        SaveImage("LastDeleted");
        Maker.Clear();
        Fields = new List<object>(); 

        List<REPLData.Packet> list = new List<REPLData.Packet>();
        try {
          using (StreamReader sr = new StreamReader(CodeTextFileName(filename))) {
            while (true) {
              if (sr.EndOfStream) break;
              List<string> user = ReadBlockOfLines(sr);
              if (sr.EndOfStream) break;
              string id = sr.ReadLine();
              REPLData.Kind kind = REPLData.Kind.Error;
              switch (id) {
              case "Reply": kind = REPLData.Kind.Reply; break;
              case "User": kind = REPLData.Kind.User; break;
              }
              if (sr.EndOfStream) break;
              List<string> reply = ReadBlockOfLines(sr);
              REPLData.Packet packet = new REPLData.Packet() { Reply = reply, User = user, ReplyKind = kind };
              try {
                switch (ExamineUserInput(user.AsReadOnly())) {
                case PacketAction.Method:
                case PacketAction.Field:
                  foreach (var s in user) allMethodsAndFields.Add(s); 
                  break;
                }
              } catch ( Exception ex ) {
                packet.Reply = new List<string>() { ex.Message };
              }
              list.Add(packet);
            }
          }
        } catch (Exception ex) {
          return "Failed: " + ex.Message;
        }
        ReplPanel.ChangeData(list);
        allMethodsAndFields.Add("}");
        Maker.AddMethodsAndFields(allMethodsAndFields.ToLexList(), true);

        MessageBox.Show(
          "The image from '" + filename + "' has been restored.\n\n" +
          "Please remember that only the methods and fields have been restored,\n" +
          "the contents of the fields have not.\n" +
          "So all fields are null or 0.", "NOTE!");
        return "" ;
      }
    }

    private static List<string> ReadBlockOfLines(StreamReader sr)
    {
      List<string> lines = new List<string>(); 
      string line = sr.ReadLine();
      while (line != "=") {
        if (line.Length > 0 && line[0] == '=') line = line.Substring(1);
        lines.Add(line);
        if (sr.EndOfStream) break;
        line = sr.ReadLine();
      }
      return lines; 
    }

    private StringBuilder DisplayText = new StringBuilder();
    public void Line(object o) { DisplayText.Append(ObjectToString(o)).AppendLine(); }
    public void Disp(object o) { DisplayText.Append(ObjectToString(o)); }

    private static string ObjectToString(object o)
    {
      string s ;
      try {
        if (o == null) s = "NULL"; else s = o.ToString();
      } catch (Exception ex) {
        s = "`" + ex.Message + "`";
      }
      return s;
    }

    private enum PacketAction { Empty, Expression , Statement, Method , IncompleteMethod , Field}
    private PacketAction ExamineUserInput(ReadOnlyCollection<string> input)
    {
      LexList ll = LexList.Get(input);
      if (ll.Count == 0) return PacketAction.Empty; 
      if (ll[0].Kind != LexKind.Delimiter && ll.Count == 1) return PacketAction.Expression; 
      Stack<char> nesting = new Stack<char>();
      foreach (LexToken tok in ll) {
        if (tok.Str == "(" || tok.Str == "[" || tok.Str == "{") {
          nesting.Push(tok.Str[0]);
        } else if (tok.Str == ")") {
          if (nesting.Count == 0 || nesting.Pop() != '(') return PacketAction.Empty;
        } else if (tok.Str == "]") {
          if (nesting.Count == 0 || nesting.Pop() != '[') return PacketAction.Empty;
        } else if (tok.Str == "}") {
          if (nesting.Count == 0 || nesting.Pop() != '{') return PacketAction.Empty;
        }
      }
      if (nesting.Count != 0) return PacketAction.Empty;
      if (ll.Count < 2) return PacketAction.Empty; 
      string first = ll[0].Str ; 
      string last = ll[ll.Count-1].Str ; 

      if (first == "(" && last == ")") return PacketAction.Expression;
      if (first == "{" && last == "}") return PacketAction.Statement;
      if (CompileOneMethod.IsVarDeclaration(ll) && last == ";") return PacketAction.Field;
      if (first == "public" || first == "private" || first == "[" ) {
        if (last == "}") return PacketAction.Method;
        return PacketAction.Empty;
      }
      if (last == ";") return PacketAction.Statement;
      if (last == "+" || last == "-" || last == "*" || last == "/" || last == "||" || last == "&&" || last == "!" || last == "^") return PacketAction.Empty; 
      return PacketAction.Expression; 
    }

    private string Command(ReadOnlyCollection<string> input)
    {
      DisplayText.Length = 0; 
      string errorMessage = ""; 
      TheStopwatch.Reset();
      TheStopwatch.Start();
      bool error = false;
      string output = "";
      try {
        PacketAction pAction = ExamineUserInput(input);
        switch ( pAction ) {
        case PacketAction.Method:
          AddMethod(input);
          output = "Method " + Maker.MostRecentNameAdded;
          break;
        case PacketAction.Field:
          AddMethod(input);
          output = "Field " + Maker.MostRecentNameAdded;
          break;
        case PacketAction.Statement:
          DoFieldsInitialisationIfOne();
          Maker.DoStatement<REPL>(this, input, false);
          output = "Statement";
          break; 
        case PacketAction.Expression :
          DoFieldsInitialisationIfOne();
          output = "Expression\n" + Maker.DoExpression<REPL, object>(this, input, false).ToString();
          break;
        default: return ""; 
        }
      } catch (Exception ex) {
        output = ex.Message;
        error = true;
      }
      if (StopwatchOn) {
        TheStopwatch.Stop();
        output = "time " + ((long)((1000000.0 * TheStopwatch.ElapsedTicks / Stopwatch.Frequency))).ToString() + "us\n" + output;
      }
      if (error) output = "-" + output; else output = "+" + output;
      if (DisplayText.Length != 0) output += "\n" + DisplayText; 
      return output;
    }

    private void DoFieldsInitialisationIfOne()
    {
      if (Maker.HasMethod("FieldsInitialiser")) {
        Action<REPL> action;
        Maker.GetAction<REPL>("FieldsInitialiser", out action);
        if (action != null) action(this);
      }
    }

    private void AddMethod(ReadOnlyCollection<string> input)
    {
      LexListBuilder ll = new LexListBuilder();
      ll.Add("partial class `Type {", "Type", typeof(REPL));
      foreach (string line in input) ll.Add(line);
      ll.Add("}");
      Maker.AddMethodsAndFields(ll.ToLexList(), true);
    }

    private static void ExamineCurly(string line, ref int nesting, ref bool foundCurly)
    {
      LexList list = LexList.Get(line);
      foreach (var tok in list) {
        if (tok.Str == "{") {
          nesting++;
          foundCurly = true;
        } else if (tok.Str == "}") {
          nesting--;
          foundCurly = true;
        }
      }
    }

    public static void Show(TypeParser parser)
    {
      REPL repl = new REPL(parser);
      REPLRenderingPanel replPanel = new REPLRenderingPanel(repl.DisplayData, repl.Command , new FontFamily("Lucida Console"), 10);
      repl.ReplPanel = replPanel; 
      var win = new StickyWindow((r) => Persist.Put<PersistRect>("REPL",r) , () => Persist.Get<PersistRect>("REPL", new PersistRect (150,150,500,400) ) )
      {
        Title = "C# REPL by KamimuCode",
        Content = new ScrollerPanel()
        {
          Content = new CursorPanel() { Content = replPanel }
        }
      } ;
      replPanel.Focus(); 
      
      win.ShowDialog();
    }
  }
}

#endif
