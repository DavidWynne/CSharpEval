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

#if Dialog

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;


namespace Kamimu
{
  public abstract class RenderingPanel : ContentControl, IScrollee       
  {
    public Typeface Typeface;
    public int StartLine = 0;
    public int CursorLine = 0;
    public int CursorX = 0;
    public int StartX = 0; 
    public double LineHeight;
    public double SpaceWidth;
    public Canvas TheCanvas;
    public Brush CaretBrush;
    public Line Caret;
    public Point MouseStartLocation;
    public bool LeftMouseDown; 

    public RenderingPanel (FontFamily family, double fontSize)
    {
      FontFamily = family;
      foreach (var tf in FontFamily.GetTypefaces()) { Typeface = tf; break; }
      
      FontSize = fontSize;
      LineHeight = new FormattedText("XxypI", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface, FontSize, new SolidColorBrush(Colors.Black)).Height + 2;
      string spaces = "                    "; 
      SpaceWidth = new FormattedText(spaces, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface, FontSize, new SolidColorBrush(Colors.Black))
        .WidthIncludingTrailingWhitespace / spaces.Length;
      CursorLine = 0;
      StartLine = 0;
      SizeChanged += LexReadDialog_SizeChanged;
    }

    void LexReadDialog_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      SendScrolleeInfo();     
    }

    protected override void OnInitialized(EventArgs e)
    {
      base.OnInitialized(e);
      SendScrolleeInfo();
    }

    public void MakeCaret(Color colour, int cycleTime)
    {
      TheCanvas = new Canvas() { Visibility = Visibility.Visible };
      Content = TheCanvas;

      CaretBrush = new SolidColorBrush(colour);
      DoubleAnimation caretAnimation = new DoubleAnimation();
      caretAnimation.From = 1.0;
      caretAnimation.To = 0.0;
      caretAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(cycleTime/2));
      caretAnimation.AutoReverse = true;
      caretAnimation.RepeatBehavior = RepeatBehavior.Forever;
      CaretBrush.BeginAnimation(Brush.OpacityProperty, caretAnimation);
      Caret = new Line() { StrokeThickness = 1, Stroke = CaretBrush, Visibility = Visibility.Visible, X1 = 0, Y1 = 0, X2 = 0, Y2 = LineHeight };
      TheCanvas.Children.Add(Caret);
    }

    private void SetCaretPosition()
    {
      if (Caret != null) {
        double x = (CursorX - StartX) * SpaceWidth;
        double y = (CursorLine - StartLine) * LineHeight;
        AdjustCaretPosition(ref x, ref y);
        Canvas.SetLeft(Caret, x);
        Canvas.SetTop(Caret, y);
      }
    }

    protected virtual void AdjustCaretPosition(ref double x, ref double y) { }

    public void SendScrolleeInfo()
    {
      if (Parent is IScroller) {
        ScrollerPanel.ScrolleeEventArgs sea = new ScrollerPanel.ScrolleeEventArgs();
        SetScrolleeInfo(Orientation.Vertical, sea);
        (Parent as IScroller).ScrollerInfo(sea);
        SetScrolleeInfo(Orientation.Horizontal, sea);
        (Parent as IScroller).ScrollerInfo(sea);
      }
    }

    protected abstract void SetScrolleeInfo(Orientation orientation, ScrollerPanel.ScrolleeEventArgs sea);

    void IScrollee.ScrolleeStart(Orientation orientation, double v)
    {
      if (orientation == Orientation.Horizontal) {
        StartX = (int)v;
      } else {
        StartLine = (int)v;
      }
      SetCaretPosition(); 
      InvalidateVisual();
    }

    protected virtual void JustifyCursor(ref int line, int previousLine, ref int x , int previousX ) { } 

    void IScrollee.ScrolleeCursor(Orientation orientation, double v)
    {
      int previousLine = CursorLine;
      int previousX = CursorX; 
      if (orientation == Orientation.Vertical) {
        CursorLine = (int)v;
      } else {
        CursorX = (int)v; 
      }
      JustifyCursor(ref CursorLine, previousLine, ref CursorX, previousX);
      SetCaretPosition(); 
      InvalidateVisual();
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
      Point location = e.GetPosition(this);
      location = GetCursorFromLocation(location);
      MouseStartLocation = location; 
      SendScrolleeInfo();
      if (Parent is CursorPanel) (Parent as CursorPanel).Focus();
      InvalidateVisual();
      LeftMouseDown = true;
      SetSelectedRange(MouseStartLocation, MouseStartLocation); 
      base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
      if (LeftMouseDown) TrackMouse(e);
      Point location = e.GetPosition ( this ) ; 
      base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
      if (LeftMouseDown) TrackMouse(e);
      LeftMouseDown = false; 
      base.OnMouseUp(e);
    }

    private void TrackMouse(MouseEventArgs e)
    {
      if (LeftMouseDown) {
        Point location = e.GetPosition(this);
        location = GetCursorFromLocation(location);
        if (MouseStartLocation.X != location.X || MouseStartLocation.Y != location.Y) {
          SetSelectedRange(MouseStartLocation, location);
        }
      }
    }

    protected virtual void ShiftMove(bool isBefore) { }
    protected virtual void SetSelectedRange(Point? from, Point? to) { }
    protected virtual void InsertKey(bool isShift, bool isControl) { } 
    protected abstract Point GetCursorFromLocation(Point location);
    protected virtual void InputText(string text) { }
    void IScrollee.ScrolleeText(string text) { InputText(text); }
    void IScrollee.ScrolleeInsertKey(bool isShift, bool isControl) { InsertKey ( isShift , isControl ) ; }
    int IScrollee.ScrolleeGetLineLength() { return GetLineLength () ; }

    protected virtual int GetLineLength() { return -1; }
    void IScrollee.ScrolleeShiftMove(bool isBefore) { ShiftMove(isBefore); }
  }
}

#endif
