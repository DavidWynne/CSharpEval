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

#if Class

using System.Text;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Diagnostics;

namespace Kamimu
{
  public partial class MakeClass
  {


    public MakeClass GetAction<I>(string methodName, out Action<I> action)
    {
      action = (instance) => ((Action<I, List<Delegate>>)MethodTable[FindCompiledMethod(methodName, typeof(Action<I>)).Index])(instance, MethodTable);
      return this;
    }
    public void InvokeAction<I>(string methodName, I i) { Action<I> action; GetAction<I>(methodName, out action); if (action != null)  action(i); }


    public MakeClass GetAction<I, A>(string methodName, out Action<I, A> action)
    {
      int index = FindCompiledMethod(methodName, typeof(Action<I, A>)).Index;
      action = (instance, a) => ((Action<I, List<Delegate>, A>)MethodTable[index])(instance, MethodTable, a);   
      return this;
    }
    public void InvokeAction<I, A>(string methodName, I i, A a) { Action<I, A> action; GetAction<I, A>(methodName, out action); if (action != null)  action(i, a); }


    public MakeClass GetAction<I, A, B>(string methodName, out Action<I, A, B> action)
    {
      action = (instance, a, b) => ((Action<I, List<Delegate>, A, B>)MethodTable[FindCompiledMethod(methodName, typeof(Action<I, A, B>)).Index])(instance, MethodTable, a, b);
      return this;
    }
    public void InvokeAction<I, A, B>(string methodName, I i, A a, B b) { Action<I, A, B> action; GetAction<I, A, B>(methodName, out action); if (action != null)  action(i, a, b); }


    public MakeClass GetAction<I, A, B, C>(string methodName, out Action<I, A, B, C> action)
    {
      action = (instance, a, b, c) => ((Action<I, List<Delegate>, A, B, C>)MethodTable[FindCompiledMethod(methodName, typeof(Action<I, A, B, C>)).Index])(instance, MethodTable, a, b, c);
      return this;
    }
    public void InvokeAction<I, A, B, C>(string methodName, I i, A a, B b, C c) { Action<I, A, B, C> action; GetAction<I, A, B, C>(methodName, out action); if (action != null)  action(i, a, b, c); }


    public MakeClass GetAction<I, A, B, C, D>(string methodName, out Action<I, A, B, C, D> action)
    {
      action = (instance, a, b, c, d) => ((Action<I, List<Delegate>, A, B, C, D>)MethodTable[FindCompiledMethod(methodName, typeof(Action<I, A, B, C, D>)).Index])(instance, MethodTable, a, b, c, d);
      return this;
    }
    public void InvokeAction<I, A, B, C, D>(string methodName, I i, A a, B b, C c, D d) { Action<I, A, B, C, D> action; GetAction<I, A, B, C, D>(methodName, out action); if (action != null)  action(i, a, b, c, d); }


    public MakeClass GetAction<I, A, B, C, D, E>(string methodName, out Action<I, A, B, C, D, E> action)
    {
      action = (instance, a, b, c, d, e) => ((Action<I, List<Delegate>, A, B, C, D, E>)MethodTable[FindCompiledMethod(methodName, typeof(Action<I, A, B, C, D, E>)).Index])(instance, MethodTable, a, b, c, d, e);
      return this;
    }
    public void InvokeAction<I, A, B, C, D, E>(string methodName, I i, A a, B b, C c, D d, E e) { Action<I, A, B, C, D, E> action; GetAction<I, A, B, C, D, E>(methodName, out action); if (action != null)  action(i, a, b, c, d, e); }


    public MakeClass GetFunc<I, TResult>(string methodName, out Func<I, TResult> func)
    {
      func = (instance) => ((Func<I, List<Delegate>, TResult>)MethodTable[FindCompiledMethod(methodName, typeof(Func<I, TResult>)).Index])(instance, MethodTable);
      return this;
    }
    public TResult InvokeFunc<I, TResult>(string methodName, I i) { Func<I, TResult> action; GetFunc<I, TResult>(methodName, out action); if (action != null) return action(i); return default(TResult); }


    public MakeClass GetFunc<I, A, TResult>(string methodName, out Func<I, A, TResult> func)
    {
      func = (instance, a) => ((Func<I, List<Delegate>, A, TResult>)MethodTable[FindCompiledMethod(methodName, typeof(Func<I, A, TResult>)).Index])(instance, MethodTable, a);
      return this;
    }
    public TResult InvokeFunc<I, A, TResult>(string methodName, I i, A a) { Func<I, A, TResult> action; GetFunc<I, A, TResult>(methodName, out action); if (action != null) return action(i, a); return default(TResult); }


    public MakeClass GetFunc<I, A, B, TResult>(string methodName, out Func<I, A, B, TResult> func)
    {
      func = (instance, a, b) => ((Func<I, List<Delegate>, A, B, TResult>)MethodTable[FindCompiledMethod(methodName, typeof(Func<I, A, B, TResult>)).Index])(instance, MethodTable, a, b);
      return this;
    }
    public TResult InvokeFunc<I, A, B, TResult>(string methodName, I i, A a, B b) { Func<I, A, B, TResult> action; GetFunc<I, A, B, TResult>(methodName, out action); if (action != null) return action(i, a, b); return default(TResult); }


    public MakeClass GetFunc<I, A, B, C, TResult>(string methodName, out Func<I, A, B, C, TResult> func)
    {
      func = (instance, a, b, c) => ((Func<I, List<Delegate>, A, B, C, TResult>)MethodTable[FindCompiledMethod(methodName, typeof(Func<I, A, B, C, TResult>)).Index])(instance, MethodTable, a, b, c);
      return this;
    }
    public TResult InvokeFunc<I, A, B, C, TResult>(string methodName, I i, A a, B b, C c) { Func<I, A, B, C, TResult> action; GetFunc<I, A, B, C, TResult>(methodName, out action); if (action != null) return action(i, a, b, c); return default(TResult); }


    public MakeClass GetFunc<I, A, B, C, D, TResult>(string methodName, out Func<I, A, B, C, D, TResult> func)
    {
      func = (instance, a, b, c, d) => ((Func<I, List<Delegate>, A, B, C, D, TResult>)MethodTable[FindCompiledMethod(methodName, typeof(Func<I, A, B, C, D, TResult>)).Index])(instance, MethodTable, a, b, c, d);
      return this;
    }
    public TResult InvokeFunc<I, A, B, C, D, TResult>(string methodName, I i, A a, B b, C c, D d) { Func<I, A, B, C, D, TResult> action; GetFunc<I, A, B, C, D, TResult>(methodName, out action); if (action != null) return action(i, a, b, c, d); return default(TResult); }


    public MakeClass GetFunc<I, A, B, C, D, E, TResult>(string methodName, out Func<I, A, B, C, D, E, TResult> func)
    {
      func = (instance, a, b, c, d, e) => ((Func<I, List<Delegate>, A, B, C, D, E, TResult>)MethodTable[FindCompiledMethod(methodName, typeof(Func<I, A, B, C, D, E, TResult>)).Index])(instance, MethodTable, a, b, c, d, e);
      return this;
    }
    public TResult InvokeFunc<I, A, B, C, D, E, TResult>(string methodName, I i, A a, B b, C c, D d, E e) { Func<I, A, B, C, D, E, TResult> action; GetFunc<I, A, B, C, D, E, TResult>(methodName, out action); if (action != null) return action(i, a, b, c, d, e); return default(TResult); }

  }
}

#endif
