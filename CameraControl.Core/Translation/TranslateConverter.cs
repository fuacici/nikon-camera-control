using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace CameraControl.Core.Translation
{
  public class TranslateConverter : IValueConverter    
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string val = value as string;
      if (val != null)
      {
        string key = val.Replace(" ", "_").Replace("(","_").Replace(")","_");
        if (!TranslationManager.Strings.ContainsKey(key))
          return "?" + val;
        return TranslationManager.Strings[key];
      }
      return parameter;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return null;
    }

    #endregion
  }
}
