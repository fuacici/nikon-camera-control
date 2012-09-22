using System;
using System.Collections.Generic;
using CameraControl.Core.Classes;

namespace CameraControl.Core.Devices.Classes
{
  public class PropertyValue<T> : BaseFieldClass 
  {
    public delegate void ValueChangedEventHandler(object sender, string key, T val);
    public event ValueChangedEventHandler ValueChanged;
    
    private Dictionary<string, T> _valuesDictionary;
    private AsyncObservableCollection<T> _numericValues = new AsyncObservableCollection<T>();
    private AsyncObservableCollection<string> _values = new AsyncObservableCollection<string>();

    private string _name;
    public string Name
    {
      get { return _name; }
      set
      {
        _name = value;
        NotifyPropertyChanged("Name");
      }
    }

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
            foreach (KeyValuePair<string, T> keyValuePair in _valuesDictionary)
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

    public AsyncObservableCollection<string> Values
    {
      get { return _values; }
    }

    public AsyncObservableCollection<T> NumericValues
    {
      get { return _numericValues; }
    }

    public PropertyValue()
    {
      _valuesDictionary = new Dictionary<string, T>();
      IsEnabled = true;
    }

    public void SetValue(T o)
    {
      foreach (KeyValuePair<string, T> keyValuePair in _valuesDictionary)
      {
        if (EqualityComparer<T>.Default.Equals(keyValuePair.Value, o)) //(keyValuePair.Value== o)
        {
          Value = keyValuePair.Key;
          return;
        }
      }
      Console.WriteLine("Value not found");
    }

    public void SetValue()
    {
      Value = Value;
    }


    public void SetValue(string o)
    {
      foreach (KeyValuePair<string, T> keyValuePair in _valuesDictionary)
      {
        if (keyValuePair.Key == o)
        {
          Value = keyValuePair.Key;
          return;
        }
      }
    }

    public void SetValue(byte[] ba)
    {
      if (ba == null || ba.Length < 2)
        return;
      if (typeof(T) == typeof(int))
      {
        int val = BitConverter.ToInt16(ba, 0);
        SetValue((T)((object)val));
      }
      if (typeof(T) == typeof(long))
      {
        long val = BitConverter.ToInt32(ba, 0);
        SetValue((T)((object)val));
      }
      if (typeof(T) == typeof(uint))
      {
        uint val = BitConverter.ToUInt16(ba, 0);
        SetValue((T)((object)val));
      }
    }

    public void AddValues(string key, T value)
    {
      if (!_valuesDictionary.ContainsKey(key))
        _valuesDictionary.Add(key, value);
      _values = new AsyncObservableCollection<string>(_valuesDictionary.Keys);
      _numericValues = new AsyncObservableCollection<T>(_valuesDictionary.Values);
      NotifyPropertyChanged("Values");
    }

    public void Clear()
    {
      _valuesDictionary.Clear();
    }

  }
}
