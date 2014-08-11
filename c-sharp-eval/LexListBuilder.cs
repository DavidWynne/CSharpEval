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

namespace Kamimu
{
  public class LexListBuilder 
  {
    private List<LexList> List = new List<LexList>() ;
    public LexListBuilder Add(LexListBuilder other) { List.AddRange(other.List); return this;  }
    public LexListBuilder Add(List<LexList> other) { List.AddRange(other); return this; }
    public void Clear() { List.Clear(); }
    public LexListBuilder AddAndPromoteQuotes(string s, params object[] expansions) { List.Add(new LexList(LexListNewOption.Expansions, PromoteQuotes(s), expansions)); return this; }
    //public LexListBuilder AddAndPromoteQuotes(string s, object ob) { List.Add(new LexList(true, PromoteQuotes(s), ob)); return this; }
    public LexListBuilder Add(string s, params object[] expansions) { List.Add(new LexList(LexListNewOption.Expansions, s.Replace('`','\'') , expansions)); return this; }
    public LexList ToLexList() { return new LexList(List); }
    private string PromoteQuotes(string s)
    {
      return s.Replace('\'', '"').Replace('`', '\''); 
    }
  }
}