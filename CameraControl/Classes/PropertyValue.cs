﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraControl.Classes
{
  public class PropertyValue:BaseFieldClass
  {
    public delegate void ValueChangedEventHandler(object sender, string key, int val);
    public event ValueChangedEventHandler ValueChanged;
    
    private Dictionary<string, int> _valuesDictionary;

    private string _value;
    public string Value
    {
      get { return _value; }
      set
      {
        //if (_value != value)
        //{
          _value = value;
          if(ValueChanged != null)
          {
            foreach (KeyValuePair<string, int> keyValuePair in _valuesDictionary)
            {
              if (keyValuePair.Key == _value)
                ValueChanged(this, _value, keyValuePair.Value);
            }
          }
        //}
        NotifyPropertyChanged("Value");
      }
    }

    private bool _isEnabled;
    public bool IsEnabled
    {
      get
      {
        //if (Values == null || Values.Count==0)
        //  return false;
        return _isEnabled;
      }
      set
      {
        _isEnabled = value;
        NotifyPropertyChanged("IsEnabled");
      }
    }

    public AsyncObservableCollection<object> Values
    {
      get { return new AsyncObservableCollection<object>(_valuesDictionary.Keys); }
    }

    public PropertyValue()
    {
      _valuesDictionary = new Dictionary<string, int>();
      IsEnabled = true;
    }

    public void SetValue(int o)
    {
      foreach (KeyValuePair<string, int> keyValuePair in _valuesDictionary)
      {
        if (keyValuePair.Value== o)
        {
          _value = keyValuePair.Key;
          NotifyPropertyChanged("Value");
        }
      }
    }

    public void AddValues(string key, int value)
    {
      if (!_valuesDictionary.ContainsKey(key))
        _valuesDictionary.Add(key, value);
      NotifyPropertyChanged("Values");
    }


  }
}
