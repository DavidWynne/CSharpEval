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
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Kamimu
{
  public interface IScrollee
  {
    void ScrolleeStart( Orientation orientation , double v);
    void ScrolleeCursor(Orientation orientation, double v);
    void ScrolleeText(string text);
    void ScrolleeInsertKey(bool isShift, bool isControl);
    int ScrolleeGetLineLength();
    void ScrolleeShiftMove(bool isBefore);
  }

  public interface IScroller
  {
    void ScrollerInfo(ScrollerPanel.ScrolleeEventArgs sea);
  }

  public class ScrollerPanel : UserControl , IScroller 
  {
    private ScrollBar[] ScrollBars = new ScrollBar[2] ;  // [0] - Horizontal, [1] - Vertical
    private bool ScrollBarsEnabled ;
    private Canvas TheCanvas;
    private FrameworkElement TheContent; 
 
    public delegate void ScrolleeEventHandler ( object sender , ScrolleeEventArgs ea ) ;

    public class ScrolleeEventArgs 
    {
      public ScrolleeEventArgs() : base() { }
      public Orientation Orientation;
      public double StartValue;
      public double CursorValue;
      public double Minimum;
      public double Maximum;
      public double SmallChange;
      public double LargeChange;
      public double ViewportSize;
      public double ViewableSpan;

      public ScrolleeEventArgs Clone() { return (ScrolleeEventArgs)MemberwiseClone(); }

      internal void Copy(ScrolleeEventArgs Sizes)
      {
        Orientation = Sizes.Orientation;
        StartValue = Sizes.StartValue;
        CursorValue = Sizes.CursorValue;
        Minimum = Sizes.Minimum;
        Maximum = Sizes.Maximum;
        SmallChange = Sizes.SmallChange;
        LargeChange = Sizes.LargeChange;
        ViewportSize = Sizes.ViewportSize;
        ViewableSpan = Sizes.ViewableSpan; 
      }
    }

    public new FrameworkElement Content 
    {
      get { return TheContent; }
      set
      {
        ScrollBarsEnabled = false ; 
        if (TheContent != null) TheCanvas.Children.Remove(TheContent); 
        TheContent = value;
        UpdateScrollBars(null,null);
        if (TheContent != null) TheCanvas.Children.Add(TheContent); 
        ScrollBarsEnabled = true ;
      }
    }

    public ScrollerPanel()
    {
      SizeChanged += UpdateScrollBars;
      TheCanvas = new Canvas();
      base.Content = TheCanvas;
    }

    private ScrollBar VertScrollBar { get { return ScrollBars[(int)(Orientation.Vertical)]; } }
    private ScrollBar HorScrollBar { get { return ScrollBars[(int)(Orientation.Horizontal)]; } }
    private void UpdateScrollBars(object sender, SizeChangedEventArgs e)
    {
      if (VertScrollBar != null) {
        Canvas.SetRight(VertScrollBar, 0 );
        Canvas.SetTop(VertScrollBar, 0);
        if (HorScrollBar != null && this.ActualHeight - HorScrollBar.Height > 0) {
          VertScrollBar.Height = this.ActualHeight - HorScrollBar.Height;
        } else {
          VertScrollBar.Height = this.ActualHeight;
        }
      }
      if (HorScrollBar != null) {
        Canvas.SetBottom(HorScrollBar, 0);
        Canvas.SetLeft(HorScrollBar, 0);
        if (VertScrollBar != null && this.ActualWidth - VertScrollBar.Width > 0) {
          HorScrollBar.Width = this.ActualWidth - VertScrollBar.Width;
        } else {
          HorScrollBar.Width = this.ActualWidth;
        }
      }
      if (TheContent != null) {
        Canvas.SetLeft(TheContent, 0);
        Canvas.SetTop(TheContent, 0);
        
        TheContent.Width = Floor ( ActualWidth - ((VertScrollBar == null) ? 0 : VertScrollBar.Width));
        TheContent.Height = Floor( ActualHeight- ((HorScrollBar == null) ? 0 : HorScrollBar.Height));
      }
    }

    private double Floor(double v)
    {
      if (v >= 0) return v;
      return 0;
    }

    private void CreateScrollBar ( Orientation orientation ) 
    {
      if (ScrollBars[(int)orientation] == null) {
        ScrollBar scrl = new ScrollBar() { Orientation = orientation, Visibility = Visibility.Visible };
        ScrollBars[(int)orientation] = scrl;
        scrl.ValueChanged += ScrollBar_ValueChanged;
        TheCanvas.Children.Add(scrl);
        UpdateScrollBars(null,null); 
      }
    }

    private void ScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      if (ScrollBarsEnabled && TheContent is IScrollee) {
        ScrollBar scrl = (ScrollBar)sender;
        if (sender is ScrollBar) ((IScrollee)TheContent).ScrolleeStart(scrl.Orientation, scrl.Value);
      }
    }

    private void SetScrollBarValues(ScrolleeEventArgs sea)
    {
      ScrollBar scrollBar = ScrollBars[(int)sea.Orientation]; 
      if (scrollBar != null) {
        ScrollBarsEnabled = false;
        scrollBar.Value = sea.StartValue;
        scrollBar.Minimum = sea.Minimum;
        scrollBar.Maximum = sea.Maximum;
        scrollBar.SmallChange = sea.SmallChange;
        scrollBar.LargeChange = sea.LargeChange;
        scrollBar.ViewportSize = sea.ViewportSize; 
        ScrollBarsEnabled = true;
      }
    }

    void IScroller.ScrollerInfo(ScrollerPanel.ScrolleeEventArgs sea)
    {
      CreateScrollBar(sea.Orientation);
      SetScrollBarValues(sea);
    }
  }
}
#endif
