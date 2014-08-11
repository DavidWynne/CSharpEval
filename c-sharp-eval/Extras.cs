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
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Globalization;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace Kamimu
{
  public delegate F Func<A, B, C, D, E, F>(A a, B b, C c, D d, E e);
  public delegate G Func<A, B, C, D, E, F, G>(A a, B b, C c, D d, E e, F f);
  public delegate H Func<A, B, C, D, E, F, G, H>(A a, B b, C c, D d, E e, F f, G g);

  public delegate void Action<A, B, C, D, E>(A a, B b, C c, D d, E e);
  public delegate void Action<A, B, C, D, E, F>(A a, B b, C c, D d, E e, F f);
  public delegate void Action<A, B, C, D, E, F, G>(A a, B b, C c, D d, E e, F f, G g);

  public class Memoizer<Key, Env, Value>
  {
    private Func<Key, Env, Value> GetValue;
    public Memoizer(Func<Key, Env, Value> getValue) { GetValue = getValue; }
    private Dictionary<Key, Value> Store = new Dictionary<Key, Value>();
    public void Add(Key key, Value value) { Store.Add(key, value); }
    public Value Get(Key key, Env env)
    {
      Value value;
      if (Store.TryGetValue(key, out value)) return value;
      value = GetValue(key, env);
      Store.Add(key, value);
      return value;
    }
  }

  public static class ExtraExtensions
  {
    public static bool IsDerivedFrom(this Type t, Type baseType)
    {
      if (baseType == typeof(object)) return true;
      while (t != typeof(object)) {
        if (t == baseType) return true;
        t = t.BaseType;
      }
      return false;
    }

    public static T[] GetSubArray<T>(this T[] input, int start)
    {
      if (input == null) return new T[0];
      if (start >= input.Length) return new T[0];
      int len = input.Length - start;
      return input.GetSubArray(start, len); 
    }

    public static T[] GetSubArray<T>(this T[] input, int start, int length )
    {
      if (input == null) return new T[0];
      if (start >= input.Length) return new T[0];
      int maxLen = input.Length - start;
      if (length > maxLen) length = maxLen; 
      T[] output = new T[length] ;
      for (int i = start; i < start + length; i++) {
        output[i - start] = input[i];
      }
      return output; 
    }

    public static string StringOf ( this char ch , int len ) 
    {
      Char[] charArray = null ; 
      charArray = new Char[len];
      for (int i = 0; i < charArray.Length; i++) charArray[i] = ch;
      return new String(charArray); 
    }

    public static string SubstringVirtual(this string s, int start)
    {
      if (s == null || s.Length <= start) return "";
      if (start < 0) return s; 
      return s.Substring(start); 
    }

    public static int LimitFloor(this int i, int floor) { if (i < floor) return floor; return i; }
    public static int LimitRange(this int i, int floor, int ceiling) { if (i < floor) return floor; if (i > ceiling) return ceiling; return i; }

    static public string ConcatStrings(this IEnumerable<string> ie)
    {
      StringBuilder sb = new StringBuilder(40);
      foreach (var s in ie) sb.Append(s);
      return sb.ToString();
    }

    public static string SubstringVirtual(this string s, int start, int len)
    {
      if (start < 0) { len += start; start = 0; }
      if (len < 0) return "";
      if (s == null || s.Length <= start) return ' '.StringOf(len);
      int availableLength = s.Length - start;
      if (availableLength >= len) return s.Substring(start, len);
      return s.Substring(start, availableLength) + ' '.StringOf(len - availableLength); 
    }

    public static void SplitIntoTwo(this string s, int x, out string one, out string two)
    {
      one = "" ; 
      two = "" ;
      if (s == null || s.Length == 0) return;
      if (s.Length <= x) {
        one = s;
        return;
      }
      one = s.Substring(0, x);
      two = s.Substring(x);
      return; 
    }

    public static void DeleteRange(this List<string> list, int startLine, int startX, int finishLine, int finishX)
    {
      if (startLine < 0 && finishLine < 0) return;
      if (startLine >= list.Count && finishLine >= list.Count) return;
      if (startLine > finishLine) {
        int temp = startLine; startLine = finishLine; finishLine = temp;
        temp = startX; startX = finishX; finishX = temp;
      }
      if (startLine == finishLine) {
        if (startX > finishX) {
          int temp = startX; startX = finishX; finishX = temp;
        }
        if (startX < 0 && finishX < 0) return;
        string s = list[startLine];
        if (startX >= s.Length && finishX >= s.Length) return;
        if (finishX >= s.Length) finishX = s.Length - 1;
        if (startX == 0 && finishX == s.Length - 1) {
          list[startLine] = ""; 
          return;
        }
        if (startX == 0) {
          s = s.Substring(finishX + 1);
        } else if (finishX == s.Length - 1) {
          s = s.Substring(0, startX);
        } else {
          s = s.Substring(0, startX) + s.Substring(finishX);
        }
        list[startLine] = s;
      } else {
        string sStart = list[startLine];
        string sFinish = list[finishLine];
        if (startX >= sStart.Length) startX = sStart.Length - 1;
        if (finishX >= sFinish.Length) finishX = sFinish.Length - 1;
        if (startX > 0) sStart = sStart.Substring(0, startX); else sStart = "";
        if (finishX < sFinish.Length - 1) sFinish = sFinish.Substring(finishX); else sFinish = "" ; 
        list[startLine] = sStart + sFinish;
        list.RemoveRange(startLine + 1, finishLine - startLine); 
      }
    }

    public static string InsertChar(this string s, int x, char c)
    {
      if (s == null) {
        return ' '.StringOf ( x) + c ;
      } else if (s.Length <= x) {
        return s + ' '.StringOf(x - s.Length) + c;
      } else {
        return s.Substring(0, x) + c + s.Substring(x);
      }
    }

    public static List<string> SplitIntoLines(this string s)
    {
      List<string> output = new List<string>() ; 
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < s.Length; i++) {        // <-- Loop index modifed inside loop
        if ((s[i] == '\n' || s[i] == '\r') && i +1 < s.Length && (s[i+1] == '\n' || s[i+1] == '\r') && ( s[i] != s[i+1] )) {
          i++ ; 
          output.Add ( sb.ToString() ) ; 
          sb.Length = 0 ;
        } else if (s[i] == '\n' || s[i] == '\r') {
          output.Add(sb.ToString());
          sb.Length = 0;
        } else {
          sb.Append(s[i]);
        }
      }
      output.Add(sb.ToString());
      return output ; 
    }

    public static string RemoveCharAt(this string s, int x)
    {
      if (s == null) return ""; 
      if (s.Length <= x) return s;
      return s.Remove(x, 1); 
    }
  }

  public static class NumberConversions
  {
    // Use our own string to number conversions: no exceptions if there are any formatting errors.
    static public double StrToDouble(this string s)
    {
      double n = 0;
      double factor = 1.0;
      double fraction = 0;
      bool dot = false;
      for (int i = 0; i < s.Length; i++) {
        if (s[i] == '.') {
          dot = true;
        } else if (s[i] >= '0' && s[i] <= '9') {
          if (dot) {
            fraction = fraction * 10.0 + (s[i] - '0');
            factor *= 10.0;
          } else {
            n = n * 10.0 + (s[i] - '0');
          }
        }
      }
      return n + fraction / factor;
    }

    static public float StrToFloat(this string s)
    {
      return (float)StrToDouble(s);
    }

    static public long StrToLong(this string s)
    {
      long num = 0;
      for (int i = 0; i < s.Length; i++) {
        if (s[i] >= '0' && s[i] <= '9') {
          num = num * 10 + (s[i] - '0');
        }
      }
      return num;
    }

    static public long HexStrToLong(this string s)
    {
      long num = 0;
      for (int i = 0; i < s.Length; i++) {
        if (s[i] >= '0' && s[i] <= '9') {
          num = num * 16 + (s[i] - '0');
        } else if (s[i] >= 'a' && s[i] <= 'f') {
          num = num * 16 + (s[i] - 'a' + 10);
        } else if (s[i] >= 'A' && s[i] <= 'F') {
          num = num * 16 + (s[i] - 'A' + 10);
        }
      }
      return num; 
    }

    static public int StrToInt(this string s)
    {
      int num = 0;
      for (int i = 0; i < s.Length; i++) {
        if (s[i] >= '0' && s[i] <= '9') {
          num = num * 10 + (s[i] - '0');
        }
      }
      return num;
    }
  }
}