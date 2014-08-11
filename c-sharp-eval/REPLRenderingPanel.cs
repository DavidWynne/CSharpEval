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

#if Repl
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
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;


namespace Kamimu
{


  public class REPLRenderingPanel : RenderingPanel
  {
    private int MaxCharsWide = 250; 
    private REPLData TheData = null;
    private List<string> Lines = new List<string>();
    private List<REPLData.Kind> LineKinds = new List<REPLData.Kind>();

    // Each individual edit box in the REPL window corresponds to a single REPLData.Packet and is comprises two different areas, the User entered text and the Repl generated Reply text. 
    private List<REPLData.Packet> LineToUserPacket = new List<REPLData.Packet>();  // If the cursor is on the user entered text part of the edit box, the Packet is returned, else null. 
    private List<REPLData.Packet> LineToReplyPacket = new List<REPLData.Packet>(); // If the cursor is on the reply generated part of the edit box, the Packet is returned, else null.
    private List<REPLData.Packet> LineToPacket = new List<REPLData.Packet>(); // If the cursor is on either part, the Packet is returned. 

    private List<int> LineToPacketLine = new List<int>(); 
    private double bottomOffset;
    private double topOffset;
    private SolidColorBrush BackBrush, ForeBrush, BackHighBrush, ForeHighBrush, BackCursorBrush, InsideBrush, SelectedForeBrush , SelectedBackBrush , ErrorReplyBrush , NonErrorReplyBrush ;
    private Pen BorderPen;
    private int SelectionStartLine, SelectionStartX, SelectionFinishLine, SelectionFinishX;
    private int UnsortedSelectionStartLine, UnsortedSelectionStartX, UnsortedSelectionFinishLine, UnsortedSelectionFinishX;
    private bool SelectionActive;
    private double OriginalCaretX = 0 , OriginalCaretY = 0; 
    private Func<ReadOnlyCollection<string>, string> Command;
    private bool DoingInsert = false; 

    public REPLRenderingPanel(REPLData theData , Func<ReadOnlyCollection<string>,string> command, FontFamily family, double fontSize)
      : base(family, fontSize)
    {
      TheData = theData; 
      SetUpLines();
      CursorLine = 1;
      CursorX = 2 ;
      Command = command; 
      MakeCaret(Colors.Black, 600);
    }

    public void ChangeData(List<REPLData.Packet> list)
    {
      if (TheData == null) {
        TheData = new REPLData();
      }
      TheData.Packets = list;
      SetUpLines(); 
      SetCursorToFirstLineOfLastPacket();
      SendScrolleeInfo();
      InvalidateVisual(); 
    }

    protected override void AdjustCaretPosition(ref double x, ref double y) 
    {
      if (OriginalCaretX != x || OriginalCaretY != y) {
        if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0) {
          // Shift key is off. So turn off any selection.
          SelectionActive = false;
          InvalidateVisual();
        }
      }
      OriginalCaretX = x;
      OriginalCaretY = y; 
    }

    protected override void ShiftMove(bool isBefore)
    {
      if (isBefore && !SelectionActive) {
        SelectionActive = true;
        SelectionStartLine = UnsortedSelectionStartLine = CursorLine;
        SelectionStartX = UnsortedSelectionStartX = CursorX-2;
        SelectionFinishLine = UnsortedSelectionFinishLine = CursorLine;
        SelectionFinishX = UnsortedSelectionFinishX = CursorX-2;
        InvalidateVisual () ; 
      } else if (!isBefore && SelectionActive) {
        SelectionStartLine = UnsortedSelectionStartLine;
        SelectionStartX = UnsortedSelectionStartX; 
        SelectionFinishLine = UnsortedSelectionFinishLine = CursorLine ; 
        SelectionFinishX = UnsortedSelectionFinishX = CursorX-2 ;
        JustifySelectionRange();
        InvalidateVisual(); 
      }
    }

    private void SetUpLines()
    {
      Lines.Clear();
      LineKinds.Clear();
      LineToUserPacket.Clear();
      LineToPacket.Clear();
      LineToReplyPacket.Clear(); 
      LineToPacketLine.Clear(); 
      foreach (var pac in TheData.Packets) {
        if (Lines.Count == 0) {
          Lines.Add("");
          LineKinds.Add(REPLData.Kind.FirstGap);
          LineToUserPacket.Add(null);
          LineToPacket.Add(null);
          LineToReplyPacket.Add(null); 
          LineToPacketLine.Add(-1); 
        } else {
          Lines.Add("");
          LineKinds.Add(REPLData.Kind.MiddleGap);
          LineToUserPacket.Add(null);
          LineToPacket.Add(null);
          LineToReplyPacket.Add(null); 
          LineToPacketLine.Add(-1);
        }
        int i = 0; 
        foreach (var s in pac.User) {
          Lines.Add(s);
          LineKinds.Add(REPLData.Kind.User);
          LineToUserPacket.Add(pac);
          LineToPacket.Add(pac); 
          LineToReplyPacket.Add(null); 
          LineToPacketLine.Add(i++);
        }
        foreach (var s in pac.Reply) {
          Lines.Add(s);
          LineKinds.Add(pac.ReplyKind);
          LineToUserPacket.Add(null);
          LineToPacket.Add(pac);
          LineToReplyPacket.Add(pac); 
          LineToPacketLine.Add(-1);
        }
      }
      Lines.Add("");
      LineKinds.Add(REPLData.Kind.LastGap);
      LineToUserPacket.Add(null);
      LineToReplyPacket.Add(null);
      LineToPacket.Add(null);
      LineToPacketLine.Add(-1);
    }

    protected override Point GetCursorFromLocation(Point location)
    {
      int previousLine = CursorLine;
      int previousX = CursorX;                                                                                                                                                                 
      CursorLine = StartLine + (int)(location.Y / LineHeight) ;
      CursorX = (StartX + (int)(location.X / SpaceWidth)  ) ; 
      JustifyCursor(ref CursorLine, previousLine, ref CursorX, previousX ); 
      return location;
    }

    protected override void JustifyCursor(ref int line, int previousLine, ref int x , int previousX)
    {
      int originalLine = line;
      int originalX = x; 
      if (line >= LineKinds.Count || REPLData.KindIsGap(LineKinds[line])) {
        if (previousLine < originalLine) line++; else line--;
        if (line >= Lines.Count-1) line = Lines.Count-2 ; else if (line < 1) line = 1;
      }
      if (x < 2) x = 2;
      int originalStartX = StartX;
      if (x - StartX < 2) StartX = x - 2;
      if (StartX < 0) StartX = 0; 
      if (x == 2) StartX = 0; 
      if (originalLine != line || originalX != x || originalStartX != StartX ) SendScrolleeInfo(); 
    }

    protected override void InputText(string text)
    {
      for (int i = 0; i < text.Length; i++) { // <-- Index is modified inside loop
        if (text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n') {
          InputChar('\n');
          i++;
        } else if (text[i] == '\n' && i + 1 < text.Length && text[i + 1] == '\r') {
          InputChar('\n');
          i++;
        } else if (text[i] == '\n' || text[i] == '\r') {
          InputChar('\n');
        } else {
          InputChar(text[i]);
        }
      }
      DoingInsert = false; 
    }

    private bool MakeSureEmptyPacketExists()
    {
      bool changed = false ; 
      if (TheData != null) {
        if (TheData.Packets.Count == 0) {
          TheData.Packets.Add ( new REPLData.Packet () { User = new List<string> () { "" } } ) ; 
          changed = true ; 
        } else if (TheData.Packets[TheData.Packets.Count-1].User.Count != 1 || TheData.Packets[TheData.Packets.Count-1].User[0] != "") {
          TheData.Packets.Add ( new REPLData.Packet () { User = new List<string> () { "" } } ) ; 
          changed = true ; 
        }
      } else {
        TheData = new REPLData() {
          Packets = new List<REPLData.Packet>() {
            new REPLData.Packet () { User = new List<string>() { "" } } 
          }
        } ; 
        changed = true ; 
      }
      return changed ; 
    }

    private void SetCursorToFirstLineOfNextPacket_IfItIsTheLastPacket()
    {
      REPLData.Packet lastPacket = null;
      for (int i = LineToUserPacket.Count - 1; i >= 0; i--) {
        if (LineToUserPacket[i] != null) {
          lastPacket = LineToUserPacket[i];
          break;
        }
      }
      REPLData.Packet thisPacket = LineToUserPacket[CursorLine];
      for (int i = CursorLine + 1; i < LineToUserPacket.Count; i++) {
        if (LineToUserPacket[i] != null && LineToUserPacket[i] != thisPacket && LineKinds[i] == REPLData.Kind.User) {
          if (LineToUserPacket[i] == lastPacket) {
            CursorLine = i;
            CursorX = 2;
          }
          break;
        }
      }
    }

    private void SetCursorToFirstLineOfLastPacket()
    {
      REPLData.Packet lastPacket = null;
      for (int i = LineToUserPacket.Count - 1; i >= 0; i--) {
        if (LineToUserPacket[i] != null) {
          lastPacket = LineToUserPacket[i];
          int j = i; 
          while (j > 0 && LineToUserPacket[j - 1] == lastPacket) j--;
          CursorLine = j;
          CursorX = 2; 
          break;
        }
      }
    }

    private void InputChar(char ch)
    {
      if (SelectionActive && ( SelectionStartLine != SelectionFinishLine || SelectionStartX != SelectionFinishX ) ) {
        DeleteSelection();
        if (ch == '\b' || ch == '\x7f') return ; 
      }
      SelectionActive = false; 

      REPLData.Packet packet = LineToPacket[CursorLine];
      int packetLine = LineToPacketLine[CursorLine];
      if (packetLine < 0) packetLine = 1; 

      bool isShift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0; 
      if (ch == '\n' && isShift && !DoingInsert ) {
        //if (packetLine +1 == packet.User.Count ) {
          string output = Command(packet.User.AsReadOnly());
          if (output != "") {
            char first = output[0];
            output = output.Substring(1);
            packet.Reply = output.SplitIntoLines();
            packet.ReplyKind = (first == '+') ? REPLData.Kind.Reply : REPLData.Kind.Error;
            MakeSureEmptyPacketExists();
            SetUpLines();
            if (first == '+') SetCursorToFirstLineOfNextPacket_IfItIsTheLastPacket();
            SendScrolleeInfo();
            InvalidateVisual();
            return;
          }
        //}
      }

      if (LineKinds[CursorLine] == REPLData.Kind.User) {
        switch (ch) {
        case '\n':
          string line1, line2;
          Lines[CursorLine].SplitIntoTwo(CursorX - 2, out line1, out line2);
          packet.User[packetLine] = line1 ;
          packet.User.Insert(packetLine + 1 ,line2) ; 
          CursorLine++ ; 
          CursorX = 2;
          SetUpLines();
          break ; 
        case '\b' :
            if (CursorX == 2) {
              // Join line to previous, if there is a previous
              if (packetLine == 0 || CursorLine == 0) return; // There is no previous line.
              CursorX = Lines[CursorLine - 1].Length + 2;
              CursorLine--;
              packet.User[packetLine - 1] += packet.User[packetLine];
              packet.User.RemoveAt(packetLine);
              SetUpLines();
            } else {
              // Just delete one character.
              CursorX--;
              packet.User[packetLine] = Lines[CursorLine] = Lines[CursorLine].RemoveCharAt(CursorX - 2);
            }
          break;
        case '\x7f' :
          if (CursorX - 2 >= Lines[CursorLine].Length) {
            // At end of the line, so merge in the following line (if there is one).
            if (CursorLine + 1 < LineKinds.Count && LineKinds[CursorLine + 1] == REPLData.Kind.User) {
              packet.User[packetLine] = Lines[CursorLine] = Lines[CursorLine].PadRight(CursorX - 2) + Lines[CursorLine + 1];
              packet.User.RemoveAt(packetLine + 1);
              SetUpLines();
            }
          } else {
            packet.User[packetLine] = Lines[CursorLine] = Lines[CursorLine].RemoveCharAt(CursorX - 2);
          }
          break ; 
        default :
          packet.User[packetLine] = Lines[CursorLine] = Lines[CursorLine].InsertChar(CursorX - 2, ch);
          CursorX++;
          break; 
        }
        SendScrolleeInfo(); 
        InvalidateVisual();
      }
    }

    private void DeleteSelection()
    {
      REPLData.Packet packet = LineToUserPacket[CursorLine];
      if (packet != null) {
        int startLine = LineToPacketLine[SelectionStartLine];
        int finishLine = LineToPacketLine[SelectionFinishLine];
        packet.User.DeleteRange(startLine, SelectionStartX, finishLine, SelectionFinishX);
        CursorLine = SelectionStartLine;
        CursorX = SelectionStartX + 2;
        SetUpLines();
        SelectionActive = false;
        SendScrolleeInfo();
        InvalidateVisual();
      }
    }

    protected override int GetLineLength ()
    {
      if (CursorLine >= 0 && CursorLine < Lines.Count) {
        return Lines[CursorLine].Length + 2;
      } else {
        return -1; 
      }
    }

    protected void SetSelectedRange(Point location)
    {
      if (!SelectionActive) return;
      SetSelectedRange(null, location); return; 
    }

    protected override void SetSelectedRange(Point? from, Point? to)
    {
      SelectionActive = false;
      if (from != null) {
        UnsortedSelectionStartLine = StartLine + ((int)(from.Value.Y / LineHeight)).LimitRange(0, Lines.Count - 1);
        UnsortedSelectionStartX = StartX + ((int)(from.Value.X / SpaceWidth) - 2).LimitFloor(0);
      }
      SelectionStartLine = UnsortedSelectionStartLine; 
      SelectionStartX = UnsortedSelectionStartX ; 

      if (to != null) {
        UnsortedSelectionFinishLine = StartLine + ((int)(to.Value.Y / LineHeight)).LimitRange(0, Lines.Count - 1);
        UnsortedSelectionFinishX = StartX + ((int)(to.Value.X / SpaceWidth) - 2).LimitFloor(0);
      }
      CursorLine = SelectionFinishLine = UnsortedSelectionFinishLine;
      CursorX = ( SelectionFinishX = UnsortedSelectionFinishX ) + 2 ;
      SendScrolleeInfo(); 

      JustifySelectionRange();
      InvalidateVisual();
    }

    private void JustifySelectionRange()
    {
      if (SelectionStartLine > SelectionFinishLine || (SelectionStartLine == SelectionFinishLine) && (SelectionStartX > SelectionFinishX)) {
        int temp = SelectionStartLine; SelectionStartLine = SelectionFinishLine; SelectionFinishLine = temp;
        temp = SelectionStartX; SelectionStartX = SelectionFinishX; SelectionFinishX = temp;
      }
      if (SelectionStartLine >= 0 && SelectionStartLine < Lines.Count && SelectionFinishLine >= 0 && SelectionFinishLine < Lines.Count) {
        SelectionActive =
          (LineToUserPacket[SelectionStartLine] == LineToUserPacket[SelectionFinishLine] && LineToUserPacket[SelectionFinishLine] != null) ||
          (LineToReplyPacket[SelectionStartLine] == LineToReplyPacket[SelectionFinishLine] && LineToReplyPacket[SelectionFinishLine] != null);
      }
    }

    protected override void InsertKey(bool isShift, bool isControl) 
    {
      if (isShift && !isControl) {
        if (Clipboard.ContainsText()) {
          DoingInsert = true;
          InputText(Clipboard.GetText());
          DoingInsert = false;
        }
      } else if (!isShift && isControl) {
        Clipboard.SetDataObject(GetActiveSelectionText() );
      }
    }

    private string GetActiveSelectionText()
    {
      if (!SelectionActive || (SelectionStartLine == SelectionFinishLine && SelectionStartX == SelectionFinishX)) return "";
      StringBuilder sb = new StringBuilder();
      for (int lineNumber = SelectionStartLine; lineNumber <= SelectionFinishLine; lineNumber++) {
        string line = Lines[lineNumber];
        if (lineNumber > SelectionFinishLine || lineNumber < SelectionStartLine) {
          // This line is outside the selection range of lines, or there is no selection
        } else if (lineNumber < SelectionFinishLine && lineNumber > SelectionStartLine) {
          // This line is between the from and the to selection line ends.
          sb.AppendLine(line);
        } else if (lineNumber == SelectionFinishLine && lineNumber == SelectionStartLine) {
          // The selection range is just one line, and this line is on it.
          sb.Append(line.SubstringVirtual(SelectionStartX, SelectionFinishX - SelectionStartX));
        } else if (lineNumber == SelectionStartLine) {
          // The selection range starts from this line number.
          sb.AppendLine(line.SubstringVirtual(SelectionStartX));
        } else {
          // The selection range finishes at this line number.
          sb.Append(line.SubstringVirtual(0, SelectionFinishX));
        }
      }
      return sb.ToString(); 
    }

    protected override void OnRender(System.Windows.Media.DrawingContext dc)
    {
      if (Double.IsNaN(Width) || Double.IsNaN(Height)) return;
      BackBrush = new SolidColorBrush(Colors.AntiqueWhite);
      BackCursorBrush = new SolidColorBrush(Colors.Yellow);
      ForeBrush = new SolidColorBrush(Colors.Black);
      BorderBrush = new SolidColorBrush(Colors.Chocolate); 
      BackHighBrush = new SolidColorBrush(Colors.Red);
      ForeHighBrush = new SolidColorBrush(Colors.Yellow);
      BorderPen = new Pen(BorderBrush, 1);
      InsideBrush = new SolidColorBrush(Colors.Beige);
      SelectedForeBrush = new SolidColorBrush(Colors.White);
      SelectedBackBrush = new SolidColorBrush(Colors.DarkBlue);
      NonErrorReplyBrush = new SolidColorBrush(Colors.Chocolate);
      ErrorReplyBrush = new SolidColorBrush(Colors.Red); 

      bottomOffset = LineHeight / 3;
      topOffset = 2 * LineHeight / 3;

      dc.DrawRectangle(BackBrush, null, new Rect(0, 0, Width, Height));
      DrawBorders(dc);
      DrawText(dc);
    }

    private void DrawText(System.Windows.Media.DrawingContext dc)
    {
      dc.PushClip(new RectangleGeometry(new Rect(10, 0, Width - 20, Height)));

      for (int lineNumber = StartLine; lineNumber < Lines.Count && lineNumber <= StartLine + Height / LineHeight; lineNumber++) {
        double Y = (lineNumber - StartLine) * LineHeight + 2;
        if (lineNumber == CursorLine) {
          dc.DrawRectangle(BackCursorBrush, null, new Rect(0, Y, Width, LineHeight));
        }
        DrawTextUsingString(dc, lineNumber, Y);
      }
      dc.Pop();
    }

    private void DrawBorders(System.Windows.Media.DrawingContext dc)
    {
      dc.PushClip(new RectangleGeometry(new Rect(0, 0, Width, Height)));

      int lineNumber = StartLine ;
      if (lineNumber > 0) lineNumber--; 
      while (lineNumber > 0 && !REPLData.KindIsGap(LineKinds[lineNumber])) lineNumber-- ;
      while(true) {
        double topY = (lineNumber - StartLine) * LineHeight + 2;
        lineNumber++;
        while (lineNumber < Lines.Count && !REPLData.KindIsGap(LineKinds[lineNumber])) lineNumber++;
        double bottomY = (lineNumber - StartLine) * LineHeight + 2;
        dc.DrawRoundedRectangle(InsideBrush, BorderPen, new Rect(SpaceWidth / 2, topY + topOffset, Width - SpaceWidth, bottomY - topY - LineHeight / 3), SpaceWidth , SpaceWidth );
        if (lineNumber>= Lines.Count-1 || lineNumber > StartLine + Height / LineHeight ) break ; 
      };
      dc.Pop(); 
    }

    protected override void SetScrolleeInfo(Orientation orientation,  ScrollerPanel.ScrolleeEventArgs sea)
    {
      int verticalSpan = Lines.Count;
      int horizontalSpan = MaxCharsWide; 

      sea.Orientation = orientation; 
      sea.SmallChange = 1;
      if (orientation == Orientation.Vertical) {
        sea.LargeChange = verticalSpan;
        sea.StartValue = StartLine;
        sea.CursorValue = CursorLine;
        sea.Minimum = 0;
        sea.Maximum = (verticalSpan > 0) ? verticalSpan - 1 : verticalSpan;
        sea.ViewportSize = verticalSpan;
        sea.ViewableSpan = ActualHeight / LineHeight - 1;
      } else {
        sea.LargeChange = horizontalSpan ;
        sea.StartValue = StartX;
        sea.CursorValue = CursorX;
        sea.Minimum = 0;
        sea.Maximum = (horizontalSpan > 0) ? horizontalSpan - 1 : horizontalSpan;
        sea.ViewportSize = horizontalSpan;
        sea.ViewableSpan = ActualWidth / SpaceWidth - 1;
      }
    }

    private double DrawTextToDC(DrawingContext dc, Brush foreground, Brush background, string textStr, double x, double y)
    {
      FormattedText text = new FormattedText(textStr, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, Typeface, FontSize, foreground);
      double textWidth = text.WidthIncludingTrailingWhitespace; 
      if (background != null) dc.DrawRectangle(background, null, new Rect(x, y, textWidth, LineHeight));
      dc.DrawText(text, new Point ( x , y ) ) ; 
      return textWidth ; 
    }

    private void DrawTextUsingString(DrawingContext dc, int lineNumber, double y)
    {
      if (dc != null) {
        string line = Lines[lineNumber] ;
        double x = SpaceWidth * 2 ; 
        if (line.Length > StartX) {
          line = line.Substring(StartX);
          int selStartX = SelectionStartX - StartX;
          int selFinishX = SelectionFinishX - StartX; 
          if (!SelectionActive || lineNumber > SelectionFinishLine || lineNumber < SelectionStartLine) {
            // This line is outside the selection range of lines, or there is no selection
            Brush thisBrush ;
            switch (LineKinds[lineNumber]) {
            case REPLData.Kind.Error: thisBrush = ErrorReplyBrush; break;
            case REPLData.Kind.Reply: thisBrush = NonErrorReplyBrush; break;
            default: thisBrush = ForeBrush; break;
            }
            DrawTextToDC(dc, thisBrush , null, line, x, y); 
          } else if (lineNumber < SelectionFinishLine && lineNumber > SelectionStartLine) {
            // This line is between the from and the to selection line ends.
            DrawTextToDC(dc, SelectedForeBrush, SelectedBackBrush, line, x, y); 
          } else if (lineNumber == SelectionFinishLine && lineNumber == SelectionStartLine) {
            // The selection range is just one line, and this line is on it.
            string before = line.SubstringVirtual(0, selStartX);
            string middle = line.SubstringVirtual(selStartX, selFinishX - selStartX );
            string after = line.SubstringVirtual(selFinishX );
            x += DrawTextToDC(dc, ForeBrush, null, before, x, y);
            x += DrawTextToDC(dc, SelectedForeBrush, SelectedBackBrush, middle, x, y);
            DrawTextToDC(dc, ForeBrush, null, after, x, y); 
          } else if (lineNumber == SelectionStartLine) {
            // The selection range starts from this line number.
            string before = line.SubstringVirtual(0, selStartX);
            string after = line.SubstringVirtual(selStartX);
            x += DrawTextToDC(dc, ForeBrush, null, before, x, y);
            DrawTextToDC(dc, SelectedForeBrush, SelectedBackBrush, after, x, y); 
          } else {
            // The selection range finishes at this line number.
            string before = line.SubstringVirtual(0, selFinishX);
            string after = line.SubstringVirtual(selFinishX);
            x += DrawTextToDC(dc, SelectedForeBrush, SelectedBackBrush, before, x, y);
            DrawTextToDC(dc, ForeBrush, null, after, x, y);
          }
        }
      }
    }
  }
}

#endif
