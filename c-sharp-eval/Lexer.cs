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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Kamimu
{

  /// <summary>
  /// Does the lexical analysis
  /// </summary>
  class Lexer
  {
    private string Source; // Holds the source string.
    private int Ix; // The current location in this string
    public List<LexToken> List; // The generated output list
    private bool List_GlueNextTokenToLastToken = false; 
    private int SourceLine, SourceColumn; // Keeps track of the line and column number of the current location.
    private Dictionary<string, object> Expansions = new Dictionary<string, object>(); 

    private void RegisterExpansions(object[] expansions)
    {
      if (expansions == null) return;
      for (int i = 0; i < expansions.Length; i++) {
        if (expansions[i] is string) {
        } else {
          throw new LexListException("The " + i + "th element in the expansions array is not a string."); 
        }
        string s = "'" + expansions[i] as string;
        i++;
        if (i >= expansions.Length) throw new LexListException("The expansions array is not a multiple of two long."); 
        if (Expansions.ContainsKey ( s ) ) throw new LexListException ( "The expansions array already has a '" + s + "' in it." ) ;
        Expansions.Add(s, expansions[i]); 
      }
    }

    private static HashSet<int> DoubleDelimitersSet;
    private static HashSet<char> SingleDelimitersSet;
    private static void MakeDelimiterSets()
    {
      if (DoubleDelimitersSet == null) {
        DoubleDelimitersSet = new HashSet<int>();
        AddDoubleDelimiters("<<", ">>", "<=", ">=", "&&", "||", "??", "==", "!=", "+=" , "-=" , "++" , "--" , "+=" , "-=" , "*=" , "/=" );
        SingleDelimitersSet = new HashSet<char>();
        AddSingleDelimiters(@"&!|?=.,:;^%+-*/<>(){}[]~@");
      }
    }
    private static void AddDoubleDelimiters(params string[] delms)
    {
      foreach (var dl in delms) DoubleDelimitersSet.Add((int)dl[0] + (((int)dl[1]) << 16));
    }
    private static void AddSingleDelimiters(string delms)
    {
      foreach (var dl in delms) SingleDelimitersSet.Add(dl);
    }
    internal static bool IsDoubleDelimiter(string s)
    {
      if (s == null || s.Length != 2) return false;
      return DoubleDelimitersSet.Contains((int)s[0] + (((int)s[1]) << 16));
    }
    internal static bool IsSingleDelimiter(string s)
    {
      if (s == null || s.Length != 1) return false;
      return SingleDelimitersSet.Contains(s[0]);
    }

    public Lexer ( string source  ) : this ( source , null ) {}  

    public Lexer(string source, object[] expansions)
    {
      MakeDelimiterSets();
      Source = source;
      SourceLine = 1;
      SourceColumn = 0;
      List = new List<LexToken>();
      Ix = 0;
      if (Source == null || Source.Length == 0) return;
      RegisterExpansions ( expansions) ; 
      Scan();
    }

    private void ListAdd(string str, LexKind kind)
    {
      if (kind >= LexKind.Symbol00 && kind <= LexKind.Symbol11 && Expansions.ContainsKey(str)) {
        object o;
        Expansions.TryGetValue(str, out o);
        if (o is string) {
          string so = o as string;
          o = LexList.Get(so);
        }
        if (o is LexList) {
          for (int i = 0; i < ((LexList)o).Count; i++) {
            LexToken tok = ((LexList)o)[i];
            bool toPrevious = (kind == LexKind.Symbol10 || kind == LexKind.Symbol11) && i == 0;
            bool toNext = (kind == LexKind.Symbol01 || kind == LexKind.Symbol11) && i == ((LexList)o).Count-1; 
            ListAddToken(toPrevious, new LexToken(tok), toNext );
          }
        } else {
          LexToken token = new LexToken ( o , List , List.Count ) ; 
          List.Add(token);
        }
      } else {
        ListAddToken(kind == LexKind.Symbol10 || kind == LexKind.Symbol11, new LexToken(str, kind, List, List.Count) , kind == LexKind.Symbol01 || kind == LexKind.Symbol11 ); 
      }
    }

    private void ListAddToken(bool toPrevious, LexToken tok, bool toNext)
    {
      bool glueOn = List_GlueNextTokenToLastToken || toPrevious; 
      List_GlueNextTokenToLastToken = toNext ;
      LexToken previousToken = (List.Count > 0) ? List[List.Count - 1] : null;
      if (glueOn && previousToken != null && previousToken.Kind == LexKind.Identifier ) {
        string so = previousToken.Str + tok.Str; // glue this token onto the previous token
        List.RemoveAt(List.Count - 1); // remove the previous token
        List.Add ( new LexToken ( so , LexKind.Identifier , List , List.Count ) ) ; 
      } else {
        List.Add(tok); 
      }
    }

    // Scans through the characters, generating the tokens.  
    private void Scan()
    {
      StringBuilder sb = new StringBuilder(); // Used to build one identifier or string token.
      while (Ix < Source.Length) {
        char ch = Source[Ix++];
        if (ch == (char)8211) ch = '-';
        else if (ch == (char)8220) ch = '"'; 


        if (ch == '/') {
        }
        if (ch <= ' ') {
          if (ch == '\n') {
            if (Ix < Source.Length && Source[Ix] == '\r') Ix++;
            SourceLine++;
            SourceColumn = 0;
          } else if (ch == '\r') {
            if (Ix < Source.Length && Source[Ix] == '\n') Ix++;
            SourceLine++;
            SourceColumn = 0;
          } else {
            SourceColumn++;
          }
        } else if (ch == '\'') {
          CharOrSymbolToken(sb);
        } else if (ch == '"') {
          StringToken(sb, false );
        } else if (ch == '@' && Ix < Source.Length && Source[Ix] == '"') {
          Ix++;
          SourceColumn++;
          StringToken(sb, true);
        } else if (ch == '@' && Ix + 1 < Source.Length && Source[Ix] == '@' && Source[Ix+1] == '"') {
          SourceColumn++;
          Ix++; 
          ListAdd("@", LexKind.Delimiter); 
          Ix++;
          SourceColumn++;
          StringToken(sb, true);
        } else if (ch >= '0' && ch <= '9') {
          NumberToken(sb);
        } else if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || (ch == '_')) {
          SourceColumn++;
          IdentifierToken(sb, ch);
        } else if (ch == '/' && Ix < Source.Length && Source[Ix] == '/') {
          ch = EndOfLineComment();
        } else if (Ix < Source.Length && DoubleDelimitersSet.Contains((int)ch + (((int)(Source[Ix])) << 16))) {
          SourceColumn += 2;
          ListAdd(ch.ToString() + Source[Ix++].ToString(), LexKind.Delimiter);
        } else if (SingleDelimitersSet.Contains(ch)) {
          SourceColumn++;
          ListAdd(ch.ToString(), LexKind.Delimiter);
        } else {
          string surround = "" ; 
          int start = Ix-1 ; 
          int finish = Ix ; 
          while ( start > 0 && Source[start] != '\n' && Source[start] != '\r' && start > Ix-10) start-- ; 
          while ( finish < Source.Length && Source[finish] != '\n' && Source[finish] != '\r' && finish < Ix + 10 ) finish++ ; 
          for ( int a = start ; a <= finish ; a++ ) surround += Source[a] ;  

          throw new LexListException("Invalid character here '" + ch.ToString() + "', unicode=" + (int)ch + ".\nSurrounding characters are:\n" + surround , Source , Ix-1 );
        }
      }
    }

    private char EndOfLineComment()
    {
      SourceColumn++;
      if (Ix >= Source.Length) throw new LexListException("Unexpected end to text source", Source , Ix-1 ) ; 
      char ch = Source[Ix++];
      while (Ix < Source.Length && Source[Ix] != '\n' && Source[Ix] != '\r') Ix++;
      return ch;
    }

    private void IdentifierToken(StringBuilder sb, char ch)
    {
      sb.Length = 0;
      sb.Append(ch);
      while (Ix < Source.Length &&
         ((Source[Ix] >= 'A' && Source[Ix] <= 'Z') || (Source[Ix] >= 'a' && Source[Ix] <= 'z') || Source[Ix] == '_' || (Source[Ix] >= '0' && Source[Ix] <= '9'))
      ) {
        sb.Append(Source[Ix++]);
        SourceColumn++;
      }
      ListAdd(sb.ToString(), LexKind.Identifier);
    }

    private void NumberToken(StringBuilder sb)
    {
      SourceColumn++;
      sb.Length = 0;
      sb.Append(Source[Ix - 1]);
      while (Ix < Source.Length && Source[Ix] >= '0' && Source[Ix] <= '9') {
        sb.Append(Source[Ix++]);
      }
      if (Ix >= Source.Length || Source[Ix] != '.') {
        // Is an integer
        if (Ix < Source.Length) {
          switch (Source[Ix]) {
          case 'L':
          case 'l':
            Ix++;
            SourceColumn++;
            ListAdd(sb.ToString(), LexKind.Long);
            break;
          case 'D':
          case 'd':
            Ix++;
            SourceColumn++;
            ListAdd(sb.ToString(), LexKind.Double);
            break;
          case 'F':
          case 'f':
            Ix++;
            SourceColumn++;
            ListAdd(sb.ToString(), LexKind.Float);
            break;
          default:
            ListAdd(sb.ToString(), LexKind.Int);
            break;
          }
        } else {
          ListAdd(sb.ToString(), LexKind.Int);
        }
      } else {
        // Is a double or single number with a '.' in it.
        sb.Append('.');
        Ix++;
        SourceColumn++;
        while (Ix < Source.Length && Source[Ix] >= '0' && Source[Ix] <= '9') {
          sb.Append(Source[Ix++]);
        }
        if (Ix < Source.Length) {
          switch (Source[Ix]) {
          case 'D':
          case 'd':
            Ix++;
            SourceColumn++;
            ListAdd(sb.ToString(), LexKind.Double);
            break;
          case 'F':
          case 'f':
            Ix++;
            SourceColumn++;
            ListAdd(sb.ToString(), LexKind.Float);
            break;
          default:
            ListAdd(sb.ToString(), LexKind.Double);
            break;
          }
        } else {
          ListAdd(sb.ToString(), LexKind.Double);
        }
      }
    }

    // A ' is used to represent the single quote character.
    // A 'x' is a single character constant, for any character x.
    // A '\D' is used to represent the '"' character constant.
    // A 'identifier is a symbol and is stored as such in the LexToken.
    // One character symbols are not allowed.
    // A '' is used to terminate a previous symbol and specify that when the symbol is expanded it is attached to the following symbol or identifier.
    private void CharOrSymbolToken(StringBuilder sb)
    {
      int beforeIndex = Ix - 2; 
      SourceColumn++;
      sb.Length = 0;
      sb.Append("'");
      SourceColumn++;
      if (Source[Ix] == '\n' || Source[Ix] == '\r') {
        throw new LexListException("Char runs to end of line with no terminating ' character", Source , beforeIndex+1 ) ; 
      }
      // a character like '\D' represents the double quote character '"'.
      if (Ix + 1 < Source.Length && Source[Ix] == '\\' && Source[Ix + 1] == 'D') {
        Ix += 2;
        sb.Append('"');
        if (Ix >= Source.Length || Source[Ix] != '\'') {
          throw new LexListException("Char runs to end of text with no terminating ' character", Source , beforeIndex+1 ) ; 
        }
        sb.Append("'");
        Ix++;
        SourceColumn++;
        ListAdd(sb.ToString(), LexKind.Char);
      } else {
        sb.Append(Source[Ix++]);
        if (Ix >= Source.Length || Source[Ix] == '\n' || Source[Ix] == '\r') {
          throw new LexListException("Char runs to end of text with no terminating ' character", Source, beforeIndex+1 ) ; 
        }
        if (Source[Ix] == '\'') {
          sb.Append("'");
          Ix++;
          SourceColumn++;
          ListAdd(sb.ToString(), LexKind.Char);
        } else {
          if ((Source[Ix] >= 'a' && Source[Ix] <= 'z') || (Source[Ix] >= 'A' && Source[Ix] <= 'Z')) {
            while (Ix < Source.Length &&
              (Source[Ix] == '_' ||
              (Source[Ix] >= '0' && Source[Ix] <= '9') ||
              (Source[Ix] >= 'a' && Source[Ix] <= 'z') ||
              (Source[Ix] >= 'A' && Source[Ix] <= 'Z'))
            ) {
              sb.Append(Source[Ix]);
              Ix++;
              SourceColumn++; 
            }
            bool before = beforeIndex >= 0 && (Char.IsLetterOrDigit(Source[beforeIndex]) || Source[beforeIndex] == '_') ; 
            bool after = false ; 
            if (Ix < Source.Length && Source[Ix] == '\'') {
              after = true ;
              if (Ix + 1 < Source.Length && Source[Ix + 1] == '\'') {
                // The symbol is terminated with two ', so just consume them.
                Ix += 2;
              }
            } 
            LexKind newKind ;
            if (before) {
              if (after) newKind = LexKind.Symbol11; else newKind = LexKind.Symbol10;
            } else {
              if (after) newKind = LexKind.Symbol01; else newKind = LexKind.Symbol00;
            }

            ListAdd(sb.ToString(), newKind); 

          } else {
            throw new LexListException("This '" + sb.ToString() + "' is neither a Char constant nor a Symbol", Source , beforeIndex+1 ) ; 
          }
        }
      }
    }

    private void StringToken(StringBuilder sb, bool multiline)
    {
      int errorReportIndex = Ix-1;
      SourceColumn++;
      sb.Length = 0;
      sb.Append("\"");
      while (Ix < Source.Length) {
        SourceColumn++;
        if ((Source[Ix] == '\n' || Source[Ix] == '\r') && !multiline ) {
          throw new LexListException("String runs to end of line with no terminating string quote character", Source , errorReportIndex ) ;
        } else if (Source[Ix] == '\\' && Ix < Source.Length - 1 && (Source[Ix + 1] == '"' || Source[Ix + 1] == (char)8221) && !multiline) {
          sb.Append('"');
          Ix += 2;
          SourceColumn++;
        } else if (Source[Ix] == '"' && Ix < Source.Length - 1 && (Source[Ix + 1] == '"' || Source[Ix + 1] == (char)8221) && multiline) {
          sb.Append('"');
          Ix += 2;
          SourceColumn++; 
        } else {
          if (Source[Ix] == '"' || Source[Ix] == (char)8221 ) break; 
          sb.Append(Source[Ix++]);
        }
      }
      if (Ix >= Source.Length || !(Source[Ix] == '"' || Source[Ix] == '”')) {
        throw new LexListException("String runs to end of text with no terminating ' character", Source , errorReportIndex );
      }
      sb.Append("\"");
      Ix++;
      SourceColumn++;
      string ss = sb.ToString() ; 
      if (!multiline) ss = ss.Replace(@"\n", "\n").Replace(@"\r", "r");
      ListAdd(ss, LexKind.String);
    }
  }
}