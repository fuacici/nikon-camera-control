using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraControl.Classes
{
  public class PropertyValue:BaseFieldClass
  {
    public delegate void ValueChangedEventHandler(object sender, object e);
    public event ValueChangedEventHandler ValueChanged;
    
    private Dictionary<object, object> _valuesDictionary;

    private object _value;
    public object Value
    {
      get { return _value; }
      set
      {
        if (_value != value && ValueChanged != null)
          ValueChanged(this, _value);

        _value = value;
        NotifyPropertyChanged("Value");
      }
    }

    public AsyncObservableCollection<object> Values
    {
      get { return new AsyncObservableCollection<object>(_valuesDictionary.Keys); }
    }

    public PropertyValue()
    {
      _valuesDictionary = new Dictionary<object, object>();
    }

    public void SetValue(object o)
    {
      foreach (KeyValuePair<object, object> keyValuePair in _valuesDictionary)
      {
        if(keyValuePair.Value==o)
        {
          _value = keyValuePair.Key;
          NotifyPropertyChanged("Value");
        }
      }
    }

    public void AddValues(object key, object value)
    {
      if (!_valuesDictionary.ContainsKey(key))
        _valuesDictionary.Add(key, value);
    }


  }
}
