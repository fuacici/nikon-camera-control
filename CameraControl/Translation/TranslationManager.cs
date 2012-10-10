using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;

namespace CameraControl.Translation
{
  static class TranslationManager
  {
    private static Dictionary<string, string> _translations;
    private static readonly string _path = string.Empty;
    private static readonly DateTimeFormatInfo _info;


    static TranslationManager()
    {
      _path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);  
    }

    /// <summary>
    /// Gets the translated strings collection in the active language
    /// </summary>
    public static Dictionary<string, string> Strings
    {
      get
      {
        if (_translations == null)
        {
          _translations = new Dictionary<string, string>();
          Type transType = typeof(TranslationStrings);
          FieldInfo[] fields = transType.GetFields(BindingFlags.Public | BindingFlags.Static);
          foreach (FieldInfo field in fields)
          {
            _translations.Add(field.Name, field.GetValue(transType).ToString());
          }
        }
        return _translations;
      }
    }

    static public  int LoadLanguage(string lang_code)
    {

      XmlDocument doc = new XmlDocument();
      Dictionary<string, string> TranslatedStrings = new Dictionary<string, string>();
      string langPath = "";
      try
      {
        langPath = Path.Combine(_path, "Languages", lang_code, "strings.xml");
        doc.Load(langPath);
      }
      catch (Exception e)
      {
        if (lang_code == "en-US")
          return 0; // otherwise we are in an endless loop!
        return LoadLanguage("en-US");
      }
      foreach (XmlNode stringEntry in doc.DocumentElement.ChildNodes)
      {
        if (stringEntry.NodeType == XmlNodeType.Element)
          try
          {
            TranslatedStrings.Add(stringEntry.Attributes.GetNamedItem("Field").Value, stringEntry.InnerText);
          }
          catch (Exception ex)
          {
            //Log.Error("Error in Translation Engine");
            //Log.Error(ex);
          }
      }

      Type TransType = typeof(TranslationStrings);
      FieldInfo[] fieldInfos = TransType.GetFields(BindingFlags.Public | BindingFlags.Static);
      foreach (FieldInfo fi in fieldInfos)
      {
        if (TranslatedStrings != null && TranslatedStrings.ContainsKey(fi.Name))
          TransType.InvokeMember(fi.Name, BindingFlags.SetField, null, TransType, new object[] { TranslatedStrings[fi.Name] });
        //else
        //  Log.Info("Translation not found for field: {0}.  Using hard-coded English default.", fi.Name);
      }
      return TranslatedStrings.Count;
    }
  }
}
