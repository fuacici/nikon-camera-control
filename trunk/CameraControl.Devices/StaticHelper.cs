using System;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices
{
  public class StaticHelper : BaseFieldClass
  {
    private static StaticHelper _instance;

    public static StaticHelper Instance
    {
      get
      {
        if (_instance == null)
          _instance = new StaticHelper();
        return _instance;
      }
      set { _instance = value; }
    }

    public StaticHelper()
    {
      SystemMessage = "";
    }

    private string _systemMessage;
    public string SystemMessage
    {
      get { return _systemMessage; }
      set
      {
        _systemMessage = value;
        NotifyPropertyChanged("SystemMessage");
      }
    }

    public static bool GetBit(Int32 b, int bitNumber)
    {
      return (b & (1 << bitNumber)) != 0;
    }

    /// <summary>
    /// Return serial number component from a pnp id string
    /// </summary>
    /// <param name="pnpstring"></param>
    /// <returns></returns>
    public static string GetSerial(string pnpstring)
    {
      if (pnpstring == null)
        return "";
      string ret = "";
      if (pnpstring.Contains("#"))
      {
        string[] s = pnpstring.Split('#');
        if (s.Length > 2)
        {
          ret = s[2];
        }
      }
      return ret;
    }
  }
}
