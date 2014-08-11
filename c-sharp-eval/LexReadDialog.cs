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
//FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THEDialog

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

namespace Kamimu
{
  public class LexReadDialog : RenderingPanel 
  {
    private LexList TheList = null ;
    private string Source = null;
    private int Index = -1; 

    private int[] LineStarts;
    private int[] LineLengths; 
    private int[] LineIndentations;

    private SolidColorBrush BackBrush, ForeBrush, BackHighBrush, ForeHighBrush, BackCursorBrush ;

    public LexReadDialog(LexList theList, FontFamily family, double fontSize)
      : base(family, fontSize)
    {
      TheList = new LexList(theList);
      TheList.Index = theList.Index;
      SetUpLineStarts_LexList();
    }

    public LexReadDialog(string source, int index, FontFamily family, double fontSize)
      : base(family, fontSize)
    {
      Source = source;
      Index = index;
      SetUpLineStarts_String();
    }

    private void SetUpLineStarts_String()
    {
      List<int> starts = new List<int>();
      List<int> lengths = new List<int>(); 
      starts.Add(0);
      int len = 0; 
      
      for (int i = 0; i < Source.Length; i++) { // Loop index is modified internally.
        if (Source[i] == '\n') {
          if (i + 1 < Source.Length && Source[i + 1] == '\r') i++; // <-- Modifying for loop index
          if (i + 1 < Source.Length) {
            starts.Add(i + 1);
            lengths.Add(len);
          }
          len = 0;
        } else if (Source[i] == '\r') {
          if (i + 1 < Source.Length && Source[i + 1] == '\n') i++; // <-- Modifying for loop index
          if (i + 1 < Source.Length) {
            starts.Add(i + 1);
            lengths.Add(len);
          }
          len = 0;
        } else {
          len++;
        }
      }
      lengths.Add(len); 
      LineStarts = starts.ToArray();
      LineLengths = lengths.ToArray(); 
      LineIndentations = new int[LineStarts.Length]; // The indentations just remain at zero.
    }

    private void SetUpLineStarts_LexList()
    {
      List<int> starts = new List<int>();
      List<int> indentations = new List<int>();

      int braceCount = 0;
      int bracketCount = 0;
      starts.Add(0);
      indentations.Add(2);
      for (int i = 0; i < TheList.Count; i++) {
        string s = TheList[i].Str;
        switch (s) {
        case "{":
          braceCount++;
          starts.Add(i + 1);
          indentations.Add(2 + braceCount * 2);
          break;
        case "}":
          braceCount--;
          indentations[indentations.Count - 1] -= 2;
          if (indentations[indentations.Count - 1] < 0) indentations[indentations.Count - 1] = 0;
          starts.Add(i + 1);
          indentations.Add(2 + braceCount * 2);
          break;
        case "(":
          bracketCount++;
          break;
        case ")":
          bracketCount--;
          break;
        case ";":
          if (bracketCount == 0) {
            starts.Add(i + 1);
            indentations.Add(2 + braceCount * 2);
          }
          break;
        }
      }
      LineStarts = starts.ToArray();
      LineIndentations = indentations.ToArray();
    }

    protected override Point GetCursorFromLocation(Point location)
    {
      CursorLine = StartLine + (int)(location.Y / LineHeight);
      return location;
    }

    protected override void OnRender(System.Windows.Media.DrawingContext dc)
    {
      BackBrush = new SolidColorBrush(Colors.AntiqueWhite);
      BackCursorBrush = new SolidColorBrush(Colors.Yellow);
      ForeBrush = new SolidColorBrush(Colors.Chocolate);
      BackHighBrush = new SolidColorBrush(Colors.Red);
      ForeHighBrush = new SolidColorBrush(Colors.Yellow);

      dc.DrawRectangle(BackBrush, null, new Rect(0, 0, Width, Height));
      dc.PushClip(new RectangleGeometry(new Rect(0, 0, Width, Height)));

      for (int lineNumber = StartLine; lineNumber < LineStarts.Length && lineNumber <= StartLine + Height / LineHeight; lineNumber++) {
        double Y = (lineNumber - StartLine) * LineHeight + 2;
        if (lineNumber == CursorLine) {
          dc.DrawRectangle(BackCursorBrush, null, new Rect(0, Y, Width, LineHeight));
        }

        if (TheList != null) {
          double X = LineIndentations[lineNumber] * SpaceWidth + 2;
          int startToken = LineStarts[lineNumber];
          int finishToken = (lineNumber < LineStarts.Length - 1) ? (LineStarts[lineNumber + 1] - 1) : (LineStarts.Length - 1);

          for (int i = startToken; i <= finishToken; i++) {
            if (i > 0 && TheList[i - 1].Kind != LexKind.Delimiter && TheList[i].Kind != LexKind.Delimiter) X += SpaceWidth;
            DrawTextUsingLexList(dc, i, ref X, Y);
          }
        } else if (Source != null) {
          DrawTextUsingString(dc, LineStarts[lineNumber], LineLengths[lineNumber] , Y);
        }
      }
    }

    protected override void SetScrolleeInfo(Orientation orientation, ScrollerPanel.ScrolleeEventArgs sea)
    {
      if (orientation == Orientation.Horizontal) return; 
      int verticalSpan = LineStarts.Length;

      sea.Orientation = Orientation.Vertical;
      sea.SmallChange = 1;
      sea.LargeChange = verticalSpan;
      sea.StartValue = StartLine;
      sea.CursorValue = CursorLine;
      sea.Minimum = 0;
      sea.Maximum = (verticalSpan > 0) ? verticalSpan - 1 : verticalSpan;
      sea.ViewportSize = verticalSpan;
      sea.ViewableSpan = ActualHeight / LineHeight - 1 ;
    }

    private void DrawTextUsingString(DrawingContext dc, int start, int length, double y)
    {
      if (dc != null) {
        if (Index >= start && Index < start + length + 2) {
          FormattedText text1 = new FormattedText(Source.Substring(start, Index - start), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface, FontSize, ForeBrush);
          FormattedText text2 = new FormattedText(Source.Substring(Index, 1), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface, FontSize, ForeHighBrush);
          FormattedText text3 = new FormattedText(Source.Substring(Index + 1, length - (Index - start + 1)), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface, FontSize, ForeBrush);
          double x = 0;
          dc.DrawText(text1, new Point(x, y));
          x += text1.Width;
          dc.DrawRectangle(BackHighBrush, null, new Rect(x, y, text2.WidthIncludingTrailingWhitespace, text2.Height));
          dc.DrawText(text2, new Point(x, y));
          x += text2.WidthIncludingTrailingWhitespace ;
          dc.DrawText(text3, new Point(x, y)); 
        } else {
          FormattedText text = new FormattedText(Source.Substring(start, length), CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface, FontSize, ForeBrush);
          dc.DrawText(text, new Point(0, y));
        }
      }
    }

    private void DrawTextUsingLexList(DrawingContext dc, int i, ref double x, double y)
    {
      FormattedText text = new FormattedText(
        TheList[i].FormattedStr, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface, FontSize, (i == TheList.Index) ? ForeHighBrush : ForeBrush);
      if (dc != null) {
        if (i == TheList.Index) dc.DrawRectangle(BackHighBrush, null, new Rect(x, y, text.Width, text.Height));
        dc.DrawText(text, new Point(x, y));
      }
      x += text.Width; 
    }

  }
}

#endif
