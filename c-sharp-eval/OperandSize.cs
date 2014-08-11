//Copyright (c) 2009-2010 David Wynne.
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


using System.Text;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using System.Diagnostics;
using System.Collections.ObjectModel;
namespace Kamimu
{
  public partial class CompileOneMethod
  {

    enum OperandSizes { Bool = 1, Integer = 2, Real = 4 }
    class OperandSize
    {
      public Type StartType;
      public OperandSizes Sizes;
      public LexList List;
      public Emit E;

      public OperandSize(LexList list, ExpState state, OperandSizes sizes, Emit e) { StartType = state.ResultType; Sizes = sizes; List = list; E = e; }

      public void CheckBitArithmetical(ExpState state)
      {
        if (state.ResultType.IsEnum) return;
        CheckArithmetical(state);
      }

      public void CheckArithmetical(ExpState state)
      {
        if (state == null) {
          if ((List.Str == "-" || List.Str == "+") && List.Prev().Str == List.Str) List.ThrowException("Sorry, " + List.Str + List.Str + " is not implemented.");
          List.ThrowException("Missing operand here.");
        }
        if (state.ResultType == typeof(char) && ( StartType == typeof(int) || StartType == typeof(long))) return; 
        if (state.ResultType == typeof(bool) && ((Sizes & OperandSizes.Bool) == OperandSizes.Bool)) return;
        if ((state.ResultType == typeof(int) || state.ResultType == typeof(long)) && ((Sizes & OperandSizes.Integer) == OperandSizes.Integer)) return;
        if ((state.ResultType == typeof(float) || state.ResultType == typeof(double)) && ((Sizes & OperandSizes.Real) == OperandSizes.Real)) return;
        string s = "";
        if (((Sizes & OperandSizes.Bool) == OperandSizes.Bool)) s += "bool ";
        if (((Sizes & OperandSizes.Integer) == OperandSizes.Integer)) { if (s != "") s += "or "; s += "integer "; }
        if (((Sizes & OperandSizes.Real) == OperandSizes.Real)) { if (s != "") s += "or "; s += "real "; }
        List.ThrowException("Expected an operand of type '" + s + "'.");
      }

      /// <summary>
      /// Converts the second from the top of stack value to a double. 
      /// Assumes that the Top-of-stack value is double. 
      /// </summary>
      private void ConvertNonTopToDouble()
      {
        LocalBuilder temp = E.DeclareAnonymousLocal(typeof(double), List);
        E.IL.Emit(OpCodes.Stloc, temp);
        E.IL.Emit(OpCodes.Conv_R8);
        E.IL.Emit(OpCodes.Ldloc, temp);
      }

      /// <summary>
      /// Converts the second from the top of stack value to a long. 
      /// Assumes that the Top-of-stack value is long. 
      /// </summary>
      private void ConvertNonTopToLong()
      {
        LocalBuilder temp = E.DeclareAnonymousLocal(typeof(long), List);
        E.IL.Emit(OpCodes.Stloc, temp);
        E.IL.Emit(OpCodes.Conv_I8);
        E.IL.Emit(OpCodes.Ldloc, temp);
      }

      private void ConvertTopToLong()
      {
        E.IL.Emit(OpCodes.Conv_I8);
      }

      private void ConvertTopToDouble()
      {
        E.IL.Emit(OpCodes.Conv_R8);
      }

      public ExpState NextComparison(ExpState state)
      {
        if (state.IsNull && StartType == null) return state;
        if (state.IsNull && !StartType.IsValueType) return new ExpState(StartType);
        if (state.ResultType == StartType) return state;
        if (StartType == null && !state.ResultType.IsValueType) return state;
        return Next(state);
      }

      public ExpState NextBitArithmetical(ExpState state)
      {
        if (state.ResultType.IsEnum) {
          if (state.ResultType == StartType) return state;
          List.ThrowException("Enum types not the same");
          return null;
        } else {
          return Next(state);
        }
      }

      public ExpState Next(ExpState state)
      {
        CheckArithmetical(state);
        Type firstType = StartType;
        Type secondType = state.ResultType;
        if (firstType == secondType) return state;
        if (firstType == typeof(bool) || secondType == typeof(bool)) List.ThrowException("Mixture of bool and non bool operands.");
        switch (GetTypePair(firstType, secondType)) {
        case Char_Char: 
        case Char_Int: 
        case Int_Char: 
        case Int_Int:
        case Long_Long:
        case Float_Float:
        case Double_Double:
          break;
        case Char_Long: 
        case Int_Long:
          ConvertNonTopToLong();
          break;
        case Long_Int:
        case Long_Char:
          ConvertTopToLong();
          state = new ExpState(typeof(long));
          break;
        case Char_Double: 
        case Int_Double:
        case Long_Double:
        case Float_Double:
          ConvertNonTopToDouble();
          break;
        case Double_Int:
        case Double_Char: 
        case Double_Long:
        case Double_Float:
          ConvertTopToDouble();
          state = new ExpState(typeof(double));
          break;
        case Char_Float:
        case Int_Float:
        case Long_Float:
        case Float_Int:
        case Float_Char: 
        case Float_Long:
          ConvertTopToDouble();
          ConvertNonTopToDouble();
          state = new ExpState(typeof(double));
          break;
        }
        return state;
      }
    }
  }
}