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
using System.Collections;
using System.Collections.ObjectModel;

namespace Kamimu
{

  public class LexListException : Exception
  {
    public string LexListSource = null ;
    public int Index = -1;
    public LexToken Token = null ;  
    public LexListException(string msg, string source, int index) : base(msg) { LexListSource = source; Index = index; }
    public LexListException(string msg, LexToken token) : base(msg) { Token = token; } 
    public LexListException(string msg) : base(msg) { }
  }

  public enum LexListNewOption { Copy , Expansions}

  public static class LexExtensions
  {
    static public LexList GetLexList(this string s)
    {
      return LexList.Get(s);
    }

    static public LexList GetLexList(this IEnumerable<LexList> ie)
    {
      return LexList.Get(ie);
    }
  }

  /// <summary>
  /// A LexList is immutable, once made it cannot be altered, except for the Index.
  /// A LexList has an Index. This ranges from 0 to the last token + 1. 
  /// </summary>
  public class LexList : IEnumerable<LexToken> , IEnumerable 
  {
    public LexList(LexListNewOption option, List<LexToken> list, int index)
    {
      TheList = list;
      TheIndex = index;
      TheTopCount = TheList.Count;
    }

    public LexList(LexListNewOption options, string s, params object[] expansions) : this(GetWithExpansions(s, expansions)) { }
    public LexList(bool unused, string s, object ob ) : this(GetWithExpansions(s, GetExpansionsFromObject(ob))) { }

    public static object[] GetExpansionsFromObject ( object ob ) 
    {
      // We go through the fields and properties of the object and extract all strings, Types and ints. We use these to make the expansions list. 
      List<object> expansions = new List<object>();
      MemberInfo[] mis = ob.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance);
      foreach (var m in mis) {
        if (m.MemberType == MemberTypes.Field) {
          FieldInfo fi = (FieldInfo)m;
          if (CanExpandUsingThisType(fi.FieldType)) {
            expansions.Add(m.Name);
            expansions.Add(fi.GetValue(ob));
          }
          continue;
        }
        if (m.MemberType == MemberTypes.Property) {
          PropertyInfo pi = (PropertyInfo)m ;
          if (CanExpandUsingThisType(pi.PropertyType)) {
            if (pi.CanRead) {
              expansions.Add(m.Name);
              expansions.Add(pi.GetValue(ob,null));
            }
          }
          continue;
        }
      }
      return expansions.ToArray(); 
    }

    private static bool CanExpandUsingThisType(Type type)
    {
      return type == typeof(string) || type == typeof(int) || type == typeof(Type) || type == typeof(LexList) ;
    }

    public LexList(object input) : this(new object[] { input }) { } 

    public LexList(params object[] inputs)
    {
      // A LexList is immutable, once made it cannot be altered. 
      foreach (var ob in inputs) if (ob != null) {
          Type theType = ob.GetType();
          if (LexToken.DirectType(theType)) {
            TheList.Add(new LexToken(ob, TheList, TheList.Count));
          } else if (theType == typeof(LexList)) {
            TheList.AddRange(((LexList)ob).TheList);
          } else if (theType == typeof(LexToken)) {
            TheList.Add((LexToken)ob);
          } else if (theType == typeof(LexToken[])) {
            TheList.AddRange((LexToken[])ob);
          } else if (theType == typeof(List<LexToken>)) {
            TheList.AddRange((List<LexToken>)ob);
          } else if (ob is Type) {
            TheList.Add(new LexToken((Type)ob, TheList, TheList.Count ));
          } else if (theType == typeof(string)) {
            TheList.AddRange(Get((string)ob));
          } else if (theType == typeof(string[])) {
            TheList.AddRange(Get((string[])ob));
          } else if (theType == typeof(List<string>)) {
            TheList.AddRange(Get((List<string>)ob));
          } else if (theType == typeof(ReadOnlyCollection<string>)) {
            TheList.AddRange(Get((ReadOnlyCollection<string>)ob)); 
          } else if (theType == typeof(IEnumerable<LexList>)) {
            foreach (var x in (IEnumerable<LexList>)ob) TheList.AddRange(x);
          } else if (theType == typeof(List<LexList>)) {
            foreach (var x in (List<LexList>)ob) TheList.AddRange(x);
          } else {
            throw new LexListException("Only LexList, string, string[], List<string>, LexToken, LexToken[] or List<LexToken> or\n" +
              "the primitive value types may be used to create a new LexList");
          }
        }
      if (TheList.Count == 0) {
        EndToken = null;
        TheTopCount = 0;
      } else {
        EndToken = new LexToken("", LexKind.End, TheList, TheList.Count - 1);
        TheTopCount = TheList.Count;
      }
    }

    public void CrosslinkBrackets()
    {
      Stack<int> stack = new Stack<int>() ; 
      if (TheList[0].Other == -1) {
        for (int i = 0; i < TheList.Count; i++) {
          string s = TheList[i].Str;
          if (s == "(" || s == "[" || s == "{") {
            stack.Push(i);
          } else if (s == ")" || s == "]" || s == "}") {
            if (stack.Count == 0) { Index = i; throw new LexListException("Unbalanced closing bracket '" + s + "'.", TheList[i]); }
            int otherIndex = stack.Pop();
            if (stack.Count == 0) {
            }
            switch (TheList[otherIndex].Str) {
            case "(": if (s != ")") { Index = i; throw new LexListException("Expected a closing ) here.", TheList[i]); }  break;
            case "[": if (s != "]")  { Index = i; throw new LexListException("Expected a closing ] here.", TheList[i]); } break;
            case "{": if (s != "}")  { Index = i; throw new LexListException("Expected a closing } here.", TheList[i]); } break;
            }
            TheList[otherIndex].Other = i;
            TheList[i].Other = otherIndex;
          } else {
            TheList[i].Other = i;
          }
        }
      }
    }

    private static void NewLine(StringBuilder sb, int indentation, ref int mark, ref int count, ref bool isHeader)
    {
      isHeader = false; 
      sb.AppendLine();
      if (mark > -1) {
        sb.Append('-', mark-count).Append("^^").AppendLine();
      }
      count = sb.Length ; 
      sb.Append(' ', indentation);
      mark = -1;
    }

    public string CodeFormat
    {
      get
      {
        return CodeFormatText(true);
      }
    }

    public string CodeFormatText(bool showWhereErrorIs)
    {
      StringBuilder sb = new StringBuilder(TheList.Count * 20);
      int braceCount = 0;
      int bracketCount = 0;
      int countToThisLine = 0;
      int markUnderThisLine = -1;
      bool currentLineIsHeader = false; // The line starts with 'public' or 'private'.

      for (int i = 0; i < TheList.Count; i++) {
        string s = TheList[i].FormattedStr;
        if (i == Index && showWhereErrorIs ) markUnderThisLine = sb.Length;
        switch (s) {
        case "{":
          braceCount++;
          if (currentLineIsHeader) {
            NewLine(sb, braceCount * 2 - 2 , ref markUnderThisLine, ref countToThisLine, ref currentLineIsHeader );
          }
          sb.Append(s);
          NewLine(sb, braceCount * 2, ref markUnderThisLine, ref countToThisLine, ref currentLineIsHeader);
          break;
        case "}":
          braceCount--;
          if (sb[sb.Length - 1] == ' ') sb.Length--;
          if (sb[sb.Length - 1] == ' ') sb.Length--;
          sb.Append(s);
          NewLine(sb, braceCount * 2, ref markUnderThisLine, ref countToThisLine, ref currentLineIsHeader);
          break;
        case "(":
          bracketCount++;
          sb.Append(s);
          break;
        case ")":
          bracketCount--;
          sb.Append(s);
          break;
        case ";":
          sb.Append(s);
          if (bracketCount == 0) NewLine(sb, braceCount * 2, ref markUnderThisLine, ref countToThisLine, ref currentLineIsHeader);
          break;
        default:
          if (i > 0 && TheList[i - 1].Kind != LexKind.Delimiter && TheList[i].Kind != LexKind.Delimiter) sb.Append(' ');
          if (LastLineIsBlank(sb) && (s == "public" || s == "private" || s == "partial" )) currentLineIsHeader = true ; 
          sb.Append(s);
          break;
        }
      }
      return sb.ToString();
    }

    private bool LastLineIsBlank(StringBuilder sb)
    {
      for (int i = sb.Length - 1; i >= 0; i--) {
        if (sb[i] == '\n' || sb[i] == '\r') return true;
        if (sb[i] != ' ') return false;
      }
      return true; 
    }

    public static LexList Get(params object[] inputs)
    {
      return new LexList(inputs);
    }

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder(TheList.Count * 30); 
      bool first = true ;
      foreach (LexToken tok in TheList) {
        if (!first) sb.Append(" ");
        first = false;
        sb.Append(tok.FormattedStr);
      }
      return sb.ToString(); 
    }

    /// <summary>
    /// Returns the token at the specified index. 
    /// If the index is to one past the last token, it returns the EndToken.
    /// </summary>
    public LexToken this[int ix]
    {
      get
      {
        if (ix >= TheTopCount || ix < 0) return EndToken;
        return TheList[ix];
      }
    }

    /// <summary>
    /// Returns the LexKind of the current token. 
    /// If Index is pointing to one past the last token, it returns the LexKind.End.
    /// </summary>
    public LexKind Kind
    {
      get
      {
        if (Index >= TheTopCount) return LexKind.End;
        return TheList[Index].Kind;
      }
    }

    /// <summary>
    /// Returns the current token. 
    /// If Index is pointing to one past the last token, it returns the EndToken.
    /// It then advances to the next token.
    /// </summary>
    public LexToken GetToken()
    {
      if (Index >= TheTopCount) return EndToken;
      return TheList[Index++];
    }

    /// <summary>
    /// Gets the current index. 
    /// Sets the current index. 
    /// An attempt to set it to below zero will set it to zero. 
    /// An attempt to set it to above the last token will set it to one above the last token.
    /// </summary>
    public int Index
    {
      get { return TheIndex; }
      set
      {
        if (value > TheTopCount) TheIndex = TheTopCount;
        else if (value < 0) TheIndex = 0;
        else TheIndex = value;
      }
    }

    public int Count { get { return TheTopCount; } }

    /// <summary>
    /// Advances the Index by one. If the index was at the last token or above, it is set to one above the last token. 
    /// </summary>
    public LexList Next()
    {
      if (TheIndex < TheTopCount) TheIndex++;
      return this;
    }

    /// <summary>
    /// Decrements the Index by one. If the Index is 0, it stays at 0.
    /// </summary>
    public LexList Prev() { if (TheIndex > 0) TheIndex--; return this; }

    /// <summary>
    /// Checks that the LexKind of the current token agrees with the parameter.
    /// If the Index is at one past the last token, then the LexKind of that non-existent token is assumed to be LexKind.End.
    /// </summary>
    public void CheckKind(LexKind kind)
    {
      if (Index >= TheTopCount) {
        if (kind != LexKind.End) TheList[TheTopCount - 1].ThrowException("Expected a token of kind '" + kind + "' here.", this);
      } else if (kind == LexKind.End && Index < TheTopCount) {
        TheList[Index].ThrowException("Expected the end of text here.", this);
      } else {
        if (kind != TheList[Index].Kind) TheList[Index].ThrowException("Expected a token of kind '" + kind + "' here.", this );
      }
    }

    public LexToken CurrentToken()
    {
      if (Index < TheTopCount) return TheList[Index]; else return EndToken;
    }

    public LexToken LookAtToken(int offset)
    {
      int ix = Index + offset;
      if (ix < 0 || ix >= TheTopCount) return EndToken; else return TheList[ix];
    }

    /// <summary>
    /// Checks that the current token is an LexKind.Identifier.
    /// IT returns with the identifier's string contents, then advances the Index.
    /// </summary>
    public string GetIdentifier()
    {
      CheckKind(LexKind.Identifier);
      return TheList[Index++].Str;
    }
    public string GetIdentifier(string errorMsg)
    {
      if (Kind != LexKind.Identifier) ThrowException(errorMsg);
      return TheList[Index++].Str;
    }

    public void CheckStrAndAdvance(string str, string errorMsg)
    {
      if (Str != str) ThrowException(errorMsg);
      Next();
    }

    /// <summary>
    /// Gets the string contents of the current token. 
    /// If the Index is pointing to one past the last token, it returns an empty string.
    /// </summary>
    public string Str
    {
      get
      {
        if (Index < TheTopCount) return TheList[Index].Str;
        return "";
      }
    }

    /// <summary>
    /// Checks that the current token is a LexKind.String. 
    /// Returns with the string contents, minus the surrounding string quotes. 
    /// It advances the Index to the next Token ; 
    /// </summary>
    public string GetStringValue()
    {
      CheckKind(LexKind.String);
      string str = TheList[Index++].Str;
      if (str.Length <= 2) return "";
      if (str[0] != '"') return "";
      return str.Substring(1, str.Length - 2);
    }

    public void ThrowException(string msg)
    {
      if (Index >= TheList.Count) TheList[TheList.Count - 1].ThrowException(msg, this);
      TheList[Index].ThrowException(msg, this);
    }

    public bool AtEnd()
    {
      return (Index >= TheTopCount);
    }

    public bool NextIsAtEnd()
    {
      return (Index + 1 >= TheTopCount);
    }

    public void TemporaryEndAt(int temp)
    {
      TheTopCount = temp + 1;
      if (TheTopCount > TheList.Count) TheTopCount = TheList.Count;
    }

    public void TemporaryEndAt()
    {
      TheTopCount = TheList.Count;
    }

    public void CheckStr(string str, string errorMsg)
    {
      if (Str != str) ThrowException(errorMsg);
    }                                           

    public IEnumerator<LexToken> GetEnumerator()
    {
      foreach (var lt in TheList) yield return lt;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      foreach (var lt in TheList) yield return lt;
    }

    public LexList Clone()
    {
      return new LexList(LexListNewOption.Copy, TheList, TheIndex); 
    }

    public void SkipToAfterClosingBracket()
    {
      if (TheList[Index].Other == -1) throw new LexListException("The brackets are unbalanced.", TheList[Index]);
                                      // Could also be caused by the program error of not crosslinking the lex list.
      Index = TheList[Index].Other; 
    }

    public List<LexToken> CopyList()
    {
      return new List<LexToken>(TheList); 
    }

    private static List<LexToken> GetWithExpansions(string source, object[] expansions)
    {
      Lexer lexer = new Lexer(source, expansions);
      return lexer.List;
    }

    private readonly List<LexToken> TheList = new List<LexToken>();
    private readonly LexToken EndToken;
    private int TheIndex = 0; // Index is in the range 0..List.Count. So the max value of the index is one past the top.
    private int TheTopCount;

    private static List<LexToken> Get(string source)
    {
      Lexer lexer = new Lexer(source);
      return lexer.List;
    }

    private static List<LexToken> Get(string[] source)
    {
      List<LexToken> list = new List<LexToken>(source.Length);
      foreach (var s in source) list.AddRange(LexList.Get(s));
      return list;
    }

    private static List<LexToken> Get(List<string> source)
    {
      List<LexToken> list = new List<LexToken>(source.Count);
      foreach (var s in source) list.AddRange(LexList.Get(s));
      return list;
    }

    private static List<LexToken> Get(ReadOnlyCollection<string> source)
    {
      List<LexToken> list = new List<LexToken>(source.Count);
      foreach (var s in source) list.AddRange(LexList.Get(s));
      return list;
    }
  }
}