using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Core.Classes;
using CameraControl.Devices.Classes;

namespace CameraControl.Core
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

  }
}
