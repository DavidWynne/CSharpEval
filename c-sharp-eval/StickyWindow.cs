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
  public class StickyWindow : Window
  {
    private Action<PersistRect> SavePosition;
    private Func<PersistRect> GetPosition;
    public StickyWindow(Action<PersistRect> savePosition, Func<PersistRect> getPosition)
      : base()
    {
      SavePosition = savePosition;
      GetPosition = getPosition;

      if (GetPosition != null) {
        PersistRect r = GetPosition();
        Width = r.Width;
        Height = r.Height;
        Left = r.X;
        Top = r.Y;
      }
    }

    protected override void OnLocationChanged(EventArgs e)
    {
      base.OnLocationChanged(e);
      if (SavePosition != null ) SavePosition(new PersistRect(Left, Top, ActualWidth, ActualHeight));
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);
      if (SavePosition != null) SavePosition(new PersistRect(Left, Top, ActualWidth, ActualHeight));
    }
  }

}
#endif
