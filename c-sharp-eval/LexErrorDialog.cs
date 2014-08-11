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
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kamimu
{

  public class LexErrorDialog
  {
    public string Message;
    //public LexList TokenList;
    public LexList CompilerList;
    public int CompilerIndex;
    public string Source = null;
    public int Index = -1;

    public void Show()
    {
      var win = new StickyWindow((r) => Persist.Put<PersistRect>("LexError", r), () => Persist.Get<PersistRect>("LexError", new PersistRect(100,100,500,300))) { Title = "Compiler Error", };

      var dock = new DockPanel();
      win.Content = dock;

      TabControl tabbie = new TabControl() { TabStripPlacement = Dock.Bottom };

      Label Lab;
      dock.Children.Add(Lab = new Label() { Content = Message });
      DockPanel.SetDock(Lab, Dock.Top);

      dock.Children.Add(tabbie);

      bool firstFocus = true;

      if (CompilerList != null) {
        var dialog = new LexReadDialog(CompilerList, new FontFamily("Lucida Console"), 10);
        MakeDialogItem(tabbie, ref firstFocus, dialog, "Compiler");
      }

      if (Source != null) {
        var dialog = new LexReadDialog(Source, Index, new FontFamily("Lucida Console"), 10);
        MakeDialogItem(tabbie, ref firstFocus, dialog, "Source" );
      }
      win.ShowDialog();
    }

    private static void MakeDialogItem(TabControl tabbie, ref bool firstFocus, LexReadDialog dialog, string header)
    {
      TabItem item = null;
      item = new TabItem() { Header = header + " Context" };
      tabbie.Items.Add(item);
      var scroller = new ScrollerPanel();
      item.Content = scroller;

      var cursor = new CursorPanel();
      scroller.Content = cursor;

      cursor.Content = dialog;
      if (firstFocus) cursor.Focus();
      firstFocus = false;

      item.MouseLeftButtonUp += (sender, e) => cursor.Focus();
    }
  }
}
#endif
