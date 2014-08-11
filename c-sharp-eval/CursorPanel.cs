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

#if Dialog

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

namespace Kamimu
{
  public class CursorPanel : UserControl , IScrollee, IScroller 
  {
    private ScrollerPanel.ScrolleeEventArgs Sizes_Line;
    private ScrollerPanel.ScrolleeEventArgs Sizes_X;

    public CursorPanel()
      : base()
    {
      SizeChanged += CursorPanel_SizeChanged;
      PreviewKeyDown += CursorPanel_PreviewKeyDown;
      PreviewTextInput += CursorPanel_PreviewTextInput ;

      Sizes_Line = new ScrollerPanel.ScrolleeEventArgs() ;
      Sizes_X = new ScrollerPanel.ScrolleeEventArgs(); 

      Focusable = true;
      //Focus(); // Uncomment this and the program will goto into an infinite loop under the appropriate cirsumstances.
      //IsHitTestVisible = true; 
    }

    void CursorPanel_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      if (Content is IScrollee) (Content as IScrollee).ScrolleeText(e.Text);
    }

    public void CursorPanel_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      double originalCursorLine = Sizes_Line.CursorValue;
      double originalStartLine = Sizes_Line.StartValue;
      double originalCursorX = Sizes_X.CursorValue;
      double originalStartX = Sizes_X.StartValue;

      bool isControl = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
      bool isShift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;

      if (isShift) {
        switch (e.Key) {
        case Key.Down:
        case Key.Up:
        case Key.Left:
        case Key.Right:
        case Key.PageUp:
        case Key.PageDown:
        case Key.Home:
        case Key.End: if (Content is IScrollee) (Content as IScrollee).ScrolleeShiftMove(true); break; 
        }
      }

      switch (e.Key) {
      case Key.CapsLock: 
      case Key.LeftCtrl:
      case Key.RightCtrl:
      case Key.LeftAlt:
      case Key.RightAlt:
        return; 
      case Key.LeftShift:
      case Key.RightShift:
        return; 
      case Key.Down:
        Sizes_Line.CursorValue++;
        e.Handled = true;
        break;
      case Key.Delete :
        if (Content is IScrollee) (Content as IScrollee).ScrolleeText('\x7f'.ToString());
        return; 
      case Key.Up:
        Sizes_Line.CursorValue--;
        e.Handled = true;
        break;
      case Key.Left:
        Sizes_X.CursorValue--;
        e.Handled = true;
        break;
      case Key.Insert:
        if (Content is IScrollee) (Content as IScrollee).ScrolleeInsertKey(isShift , isControl) ; 
        return; 
      case Key.Right:
        Sizes_X.CursorValue++;
        e.Handled = true;
        break;
      case Key.PageUp:
        Sizes_Line.CursorValue -= Sizes_Line.ViewableSpan;
        Sizes_Line.StartValue -= Sizes_Line.ViewableSpan;
        e.Handled = true;
        break;
      case Key.PageDown:
        Sizes_Line.CursorValue += Sizes_Line.ViewableSpan;
        if (Sizes_Line.CursorValue > Sizes_Line.Maximum) {
          Sizes_Line.CursorValue = Sizes_Line.Maximum;
        } else {
          Sizes_Line.StartValue += Sizes_Line.ViewableSpan;
        }
        e.Handled = true;
        break;
      case Key.Home:
        if (isControl) {
          Sizes_Line.CursorValue = 0;
        } else {
          Sizes_X.CursorValue = 0;
        }
        e.Handled = true;
        break;
      case Key.End:
        if (isControl) {
          Sizes_Line.CursorValue = Sizes_Line.Maximum;
        } else {
          Sizes_X.CursorValue = EndOfLineX();  
        }
        e.Handled = true;
        break;
      }
      JustifyStartLineAroundCursor();
      JustifyStartXAroundCursor();

      AdjustOtherPanelsIfAnyChanges_Line(false, originalCursorLine, originalStartLine);
      AdjustOtherPanelsIfAnyChanges_X(false, originalCursorX, originalStartX);

      if (isShift) {
        switch (e.Key) {
        case Key.Down:
        case Key.Up:
        case Key.Left:
        case Key.Right:
        case Key.PageUp:
        case Key.PageDown:
        case Key.Home:
        case Key.End: if (Content is IScrollee) (Content as IScrollee).ScrolleeShiftMove(false); break; 
        }
      }

      base.OnPreviewKeyDown(e);
    }

    protected int EndOfLineX()
    {
      int value = -1; 
      if (Content is IScrollee) value = (Content as IScrollee).ScrolleeGetLineLength(); 
      if (value <= 0) value = (int)Sizes_Line.Maximum;
      return value; 
    }

    private void AdjustOtherPanelsIfAnyChanges_Line ( bool always , double originalCursor, double originalStart )
    {
      if (always || originalCursor != Sizes_Line.CursorValue || originalStart != Sizes_Line.StartValue) {
        if (GetParentScroller() != null) {
          ScrollerPanel.ScrolleeEventArgs sea = new ScrollerPanel.ScrolleeEventArgs();
          sea.Copy(Sizes_Line);
          GetParentScroller().ScrollerInfo(sea);
        }
        if (Content is IScrollee) {
          (Content as IScrollee).ScrolleeStart(Sizes_Line.Orientation, Sizes_Line.StartValue);
          (Content as IScrollee).ScrolleeCursor(Sizes_Line.Orientation, Sizes_Line.CursorValue);
        }
      }
    }

    private void AdjustOtherPanelsIfAnyChanges_X(bool always, double originalCursor, double originalStart) 
    {
      if (always || originalCursor != Sizes_X.CursorValue || originalStart != Sizes_X.StartValue) {
        if (GetParentScroller() != null) {
          ScrollerPanel.ScrolleeEventArgs sea = new ScrollerPanel.ScrolleeEventArgs();
          sea.Copy(Sizes_X);
          GetParentScroller().ScrollerInfo(sea);
        }
        if (Content is IScrollee) {
          (Content as IScrollee).ScrolleeStart(Sizes_X.Orientation, Sizes_X.StartValue);
          (Content as IScrollee).ScrolleeCursor(Sizes_X.Orientation, Sizes_X.CursorValue);
        }
      }
    }

    private IScroller GetParentScroller()
    {
      if (Parent is IScroller) return Parent as IScroller;
      if (Parent is FrameworkElement && ((FrameworkElement)Parent).Parent is IScroller) return (((FrameworkElement)Parent).Parent as IScroller);
      return null;
    }

    private void JustifyStartLineAroundCursor()
    {
      if (Sizes_Line.CursorValue > Sizes_Line.Maximum) Sizes_Line.CursorValue = Sizes_Line.Maximum;
      if (Sizes_Line.CursorValue < 0) Sizes_Line.CursorValue = 0;
      if (Sizes_Line.StartValue > Sizes_Line.CursorValue) Sizes_Line.StartValue = Sizes_Line.CursorValue;
      if (Sizes_Line.StartValue + Sizes_Line.ViewableSpan - 1 < Sizes_Line.CursorValue) Sizes_Line.StartValue = Sizes_Line.CursorValue - Sizes_Line.ViewableSpan + 1;
      if (Sizes_Line.StartValue < 0) Sizes_Line.StartValue = 0;
    }

    private void JustifyStartXAroundCursor()
    {
      if (Sizes_X.CursorValue > Sizes_X.Maximum) Sizes_X.CursorValue = Sizes_X.Maximum;
      if (Sizes_X.CursorValue < 0) Sizes_X.CursorValue = 0;
      if (Sizes_X.StartValue > Sizes_X.CursorValue) Sizes_X.StartValue = Sizes_X.CursorValue;
      if (Sizes_X.StartValue + Sizes_X.ViewableSpan - 1 < Sizes_X.CursorValue) Sizes_X.StartValue = Sizes_X.CursorValue - Sizes_X.ViewableSpan + 1;
      if (Sizes_X.StartValue < 0) Sizes_X.StartValue = 0;
    }

    void CursorPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (Content != null && Content is FrameworkElement) {
        FrameworkElement fe = (FrameworkElement)Content;
        fe.Width = ActualWidth;
        fe.Height = ActualHeight;
      }
    }

    void IScrollee.ScrolleeStart(Orientation orientation, double v)
    {
      if (orientation == Orientation.Horizontal) {
        Sizes_X.StartValue = v; 
      } else {
        Sizes_Line.StartValue = v; 
      }
      if (Content is IScrollee) (Content as IScrollee).ScrolleeStart(orientation, v);
    }

    void IScrollee.ScrolleeCursor(Orientation orientation, double v)
    {
      if (Content is IScrollee) (Content as IScrollee).ScrolleeCursor(orientation, v);
    }

    void IScroller.ScrollerInfo(ScrollerPanel.ScrolleeEventArgs sea)
    {
      bool always = Sizes_Line == null;
      if (sea.Orientation == Orientation.Vertical) {
        double originalCursorLine = (always||Sizes_Line== null) ? -1 : Sizes_Line.CursorValue;
        double originalStartLine = (always || Sizes_Line == null) ? -1 : Sizes_Line.StartValue;
        Sizes_Line = sea.Clone();
        JustifyStartLineAroundCursor();
        AdjustOtherPanelsIfAnyChanges_Line(always, originalCursorLine, originalStartLine);
        if (GetParentScroller() != null) GetParentScroller().ScrollerInfo(Sizes_Line);
      } else {
        double originalCursorX = (always || Sizes_X == null) ? -1 : Sizes_X.CursorValue;
        double originalStartX = (always || Sizes_X == null) ? -1 : Sizes_X.StartValue;
        Sizes_X = sea.Clone();
        JustifyStartXAroundCursor();
        AdjustOtherPanelsIfAnyChanges_X(always, originalCursorX, originalStartX);
        if (GetParentScroller() != null) GetParentScroller().ScrollerInfo(Sizes_X);
      }
    }

    void IScrollee.ScrolleeText(string text) {}
    void IScrollee.ScrolleeInsertKey(bool isShift, bool isControl) { }
    int IScrollee.ScrolleeGetLineLength() { return -1; }
    void IScrollee.ScrolleeShiftMove(bool isBefore) {} 
  }
}

#endif