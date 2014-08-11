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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;

namespace Kamimu
{
  public class PersistException : Exception { public PersistException(string msg) : base(msg) { } }

  public abstract class PersistValue
  {
    public abstract void Write(XmlWriter xw, string name);
    public abstract void Read(XElement xe, string name);
  }

  public class PersistRect : PersistValue 
  {
    public double X, Y, Width, Height; 
    public PersistRect ( double x , double y , double width , double height ) { X = x ; Y = y ; Width = width ; Height = height ; }
    public PersistRect() { } 
    public override void Write(XmlWriter xw, string name)
    {
      XElement xe = new XElement ( name , 
        new XElement ( "X", X), 
        new XElement ( "Y", Y),
        new XElement ( "Width", Width) , 
        new XElement ( "Height", Height) ) ;
      xe.WriteTo(xw); 
    }
    public override void Read(XElement xe, string name)
    {
      X = (double)xe.Element("X");
      Y = (double)xe.Element("Y");
      Width = (double)xe.Element("Width");
      Height = (double)xe.Element("Height"); 
    }
  }

  public static class Persist
  {
    private static Dictionary<string, Type> PersistValueDictionary;
    private static void CreatePersistValueDictionary()
    {
      PersistValueDictionary = new Dictionary<string, Type>();
      foreach (var t in (from t in Assembly.GetExecutingAssembly().GetTypes() where t.IsSubclassOf(typeof(PersistValue)) select (t))) PersistValueDictionary.Add(t.Name, t);
    }

    private static string StorageFileName;
    public static string FileName { get { return StorageFileName; } }
    
    private static Dictionary<string, PersistValue> Storage;
    public static void ReadFromFile(string filename)
    {
      if (PersistValueDictionary == null) CreatePersistValueDictionary();
      if (Storage != null) throw new PersistException("Persist.ReadFromFile has already been called");
      StorageFileName = filename;
      Storage = new Dictionary<string, PersistValue>();
      if (File.Exists(StorageFileName)) ReadFromFile(); 
    }


    private static void Error() { if (Storage == null) throw new PersistException("Persist.ReadFromFile has not yet been called."); }

    public static void WriteToFile()
    {
      XmlWriterSettings settings = new XmlWriterSettings() { Indent = true } ;
      using (XmlWriter xw = XmlWriter.Create(StorageFileName, settings)) {
        xw.WriteStartElement("Configuration");
        foreach (var s in Storage) {
          xw.WriteStartElement("Item");
          xw.WriteElementString("Key", s.Key);
          string name = s.Value.GetType().Name; 
          ((PersistValue)s.Value).Write(xw, name);
          xw.WriteEndElement(); 
        }
        xw.WriteEndElement(); 
      }
    }

    private static PersistValue ReadPersistValue(XmlReader xr)
    {
      Type type;
      PersistValue pv = null; 
      string name = xr.Name ; 
      if (PersistValueDictionary.TryGetValue(name, out type)) {
        pv = (PersistValue)type.GetConstructor(new Type[0]).Invoke(null); 
        XElement element = (XElement)XNode.ReadFrom(xr);
        pv.Read(element, name); 
      } else {
        XNode.ReadFrom(xr); 
      }
      return pv; 
    }

    public static void ReadFromFile()
    {
      try {
        XmlReaderSettings settings = new XmlReaderSettings() { IgnoreWhitespace = true };
        using (XmlReader xr = XmlReader.Create(StorageFileName, settings)) {
          xr.MoveToContent();
          xr.ReadStartElement("Configuration");
          Dictionary<string, PersistValue> newStorage = new Dictionary<string, PersistValue>();
          while (xr.NodeType == XmlNodeType.Element) {
            xr.ReadStartElement("Item");
            string key = xr.ReadElementContentAsString("Key", "");
            PersistValue pvalue = ReadPersistValue(xr);
            newStorage.Add(key, pvalue);
            xr.ReadEndElement();
          }
          xr.ReadEndElement();
          Storage = newStorage; 
        }
      } catch {}
    }

    public static void Put<T>(string key, T item) where T : PersistValue 
    {
      PersistValue pv ;
      if (Storage == null) throw new PersistException("Need to call Persist.ReadFromFile ('filename') first."); 
      if (Storage.TryGetValue(key, out pv)) {
        if (pv.GetType() != item.GetType()) {
          throw new PersistException("Persist has already stored an item of type " + pv.GetType().Name + " for key '" + key + "' but the new item is of type " + item.GetType().Name + ".");
        }
        Storage.Remove(key); 
      }
      Storage.Add(key, item); 
    }

    public static T Get<T>(string key, T defaultValue) where T : PersistValue
    {
      PersistValue item;
      if (Storage == null) throw new PersistException("Need to call Persist.ReadFromFile ('filename') first.");
      if (Storage.TryGetValue(key, out item)) {
        if (item.GetType() != typeof(T)) {
          throw new PersistException("Persist expected a type of " + typeof(T).Name + " for key '" + key + "' but instead found a type of " + item.GetType().Name + ".");
        }
        return (T)item;
      }
      return defaultValue;  
    }
  }

}
#endif
