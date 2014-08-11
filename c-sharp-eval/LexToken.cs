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
  /// The lexical token kinds.
  /// The symbol kinds are represented in the source text by a ` followed by an identifier. 
  /// They are expanded to identifiers as the list of lextokens is constructed.
  /// The symbol of kind Symbol00 is to be expanded into its own LexToken.
  /// The symbol of kind Symbol01 is to be expanded and then pre-appended to the next identifier along. 
  /// The symbol of kind Symbol10 is to be expanded and appended to the previous identifier. 
  /// The symbol of kind Symbol11 is to be expanded and appended to the previous identifier and this whole lot to be pre-appended to the next identifier along. 
  /// The Lexer processes the source text to decide which symbol kind to use in each case. 
  /// </summary>
  public enum LexKind { String, Double, Int, Float, Long, Identifier, Delimiter, Char, End , Type, Bool  , Symbol00, Symbol01, Symbol10, Symbol11 }

  public class LexTokenException : Exception { public LexTokenException(string msg) : base(msg) { } }

  public class LexToken
  {
    public static Action<string,LexList> ShowError = null ; 

    public readonly string Str;
    public readonly LexKind Kind;
    public string ErrorLocation; 
    public readonly object ActualObject ; 

    private int LinkOther = -1; // Can only set the LinkOther value once. LinkOther points to the other bracket for matching brackets. If the 
                                // curren LexToken is not a bracket, it points to itself. 
    public int Other { get { return LinkOther; } set { if (LinkOther == -1) LinkOther = value; else throw new LexTokenException("Attempt to reset LexToken.LinkOther"); } }

    public LexToken(LexToken tok)
    {
      Str = tok.Str;
      Kind = tok.Kind;
      ErrorLocation = "";
      ActualObject = tok.ActualObject;
      LinkOther = -1; 
    }

    public string FormattedStr
    {
      get
      {
        string s = Str;
        switch (Kind) {
        case LexKind.Float: s += "F"; break;
        case LexKind.Double: s += "D"; break;
        case LexKind.Long: s += "L"; break;
        case LexKind.String: s = s.Replace ( "\n" , @"\n").Replace ( "\r", @"\r") ; break; 
        }
        return s;
      }
    }

    public LexToken(object value, List<LexToken> list , int index )
    {
      if (value == null) throw new LexTokenException("LexToken called with a null value."); 
      Str = value.ToString() ;
      ActualObject = value;
      Type theType = value.GetType() ;
      if (theType == typeof(double)) {
        Kind = LexKind.Double;
      } else if (theType == typeof(int)) {
        Kind = LexKind.Int;
      } else if (theType == typeof(float)) {
        Kind = LexKind.Float;
      } else if (theType == typeof(long)) {
        Kind = LexKind.Long;
      } else if (theType == typeof(bool)) {
        Kind = LexKind.Bool;
      } else if (theType == typeof(char)) {
        Kind = LexKind.Char;
        Str = "'" + Str + "'";
      } else if (value is Type) {
        Kind = LexKind.Type;
        Str = ((Type)value).Name; 
      } else if (theType.IsEnum) {
        Kind = LexKind.Identifier;
      } else {
        throw new LexTokenException("LexToken cannot be called with the type of '" + value.GetType().Name + "'");
      }
    }

    public LexToken(string str, LexKind kind, LexList list) : this ( str , kind , list.CopyList() , list.Index ) {} 

    public LexToken(string str, LexKind kind, List<LexToken> list, int index)
    {
      Str = str;
      Kind = kind;
    }

    public void ThrowException(string msg, LexList theList )
    {
      ErrorLocation = msg; 
      if (LexToken.ShowError != null) LexToken.ShowError ( msg , theList ) ; 
      throw new LexListException (ErrorLocation , this);                                                                           
    }
    public override string ToString()
    {
      return "'" + Str + "' " + Kind.ToString() ;
    }

    /// <summary>
    /// Types that can be converted directly to a LexToken without any more fuss.
    /// These are basically the primitive value types, excluding the string type.
    /// </summary>
    public static bool DirectType(Type theType)
    {
      return
        theType == typeof(double) ||
        theType == typeof(int) ||
        theType == typeof(float) ||
        theType == typeof(long) ||
        theType == typeof(bool) ||
        theType == typeof(char) ||
        theType.IsEnum;
    }
  }
}