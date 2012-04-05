using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WIA;

namespace CameraControl.Classes
{
  public class WIAManager : INotifyPropertyChanged
  {
    public DeviceManager DeviceManager { get; set; }
    public Device Device { get; set; }
    public string DeviceId { get; set; }

    private string _deviceName;

    public string DeviceName
    {
      get { return _deviceName; }
      set
      {
        _deviceName = value;
        NotifyPropertyChanged("DeviceName");
      }
    }

    private string _manufacturer;

    public string Manufacturer
    {
      get { return _manufacturer; }
      set
      {
        _manufacturer = value;
        NotifyPropertyChanged("Manufacturer");
      }
    }

    private bool _isConected;

    public bool IsConected
    {
      get { return _isConected; }
      set
      {
        _isConected = value;
        NotifyPropertyChanged("IsConected");
      }
    }

    private bool _apertureCanBeSet;

    public bool ApertureCanBeSet
    {
      get { return _apertureCanBeSet; }
      set
      {
        _apertureCanBeSet = value;
        NotifyPropertyChanged("ApertureCanBeSet");
      }
    }

    private bool _sutterCanBeSet;

    public bool SutterCanBeSet
    {
      get { return _sutterCanBeSet; }
      set
      {
        _sutterCanBeSet = value;
        NotifyPropertyChanged("SutterCanBeSet");
      }
    }

    private string _mode;

    public ObservableCollection<string> ExposureModeList
    {
      get { return new ObservableCollection<string>(ExposureModeTable.Values.ToList()); }
    }

    public ObservableCollection<string> WBModeList
    {
      get { return new ObservableCollection<string>(WbTable.Values.ToList()); }
    }

    private string _whiteBalance;

    public string WhiteBalance
    {
      get { return _whiteBalance; }
      set
      {
        foreach (KeyValuePair<int, string> keyValuePair in WbTable)
        {
          if (keyValuePair.Value == value)
          {
            if (SetProperty("White Balance", keyValuePair.Key))
              _whiteBalance = value;
          }
        }
        NotifyPropertyChanged("WhiteBalance");
      }
    }

    public bool SetProperty(string name, object val)
    {
      try
      {
        Device.Properties[name].set_Value(val);
        return true;
      }
      catch (Exception)
      {


      }
      return false;
    }

    public string ExposureMode
    {
      get { return _mode; }
      set
      {
        _mode = value;
        NotifyPropertyChanged("ExposureMode");
        switch (_mode)
        {
          case "M":
            SutterCanBeSet = true;
            ApertureCanBeSet = true;
            break;
          case "P":
            SutterCanBeSet = false;
            ApertureCanBeSet = false;
            break;
          case "A":
            SutterCanBeSet = false;
            ApertureCanBeSet = true;
            break;
          case "S":
            SutterCanBeSet = true;
            ApertureCanBeSet = false;
            break;
        }
      }
    }


    public Dictionary<int, string> ShutterTable;
    public Dictionary<int, string> ExposureModeTable;
    public Dictionary<int, string> WbTable;
    public Dictionary<string, long> ECTable;
    public Dictionary<string, long> FTable;

    public WIAManager()
    {
      DeviceManager = new DeviceManager();
      DeviceManager.RegisterEvent(Conts.wiaEventDeviceConnected, "*");
      DeviceManager.RegisterEvent(Conts.wiaEventDeviceDisconnected, "*");
      DeviceManager.RegisterEvent(Conts.wiaEventItemCreated, "*");
      DeviceManager.OnEvent += DeviceManager_OnEvent;
      DeviceName = "";
      Manufacturer = "";

      ECTable = new Dictionary<string, long>();
      FTable=new Dictionary<string, long>();
      ShutterTable = new Dictionary<int, string>
                       {
                         {1, "6400"},
                         {2, "4000"},
                         {3, "3200"},
                         {4, "2500"},
                         {5, "2000"},
                         {6, "1600"},
                         {8, "1250"},
                         {10, "1000"},
                         {12, "800"},
                         {15, "640"},
                         {20, "500"},
                         {25, "400"},
                         {31, "320"},
                         {40, "250"},
                         {50, "200"},
                         {62, "160"},
                         {80, "125"},
                         {100, "100"},
                         {125, "80"},
                         {166, "60"},
                         {200, "50"},
                         {250, "40"},
                         {333, "30"},
                         {400, "25"},
                         {500, "20"},
                         {666, "15"},
                         {769, "13"},
                         {1000, "10"},
                         {1250, "8"},
                         {1666, "6"},
                         {2000, "5"},
                         {2500, "4"},
                         {3333, "3"},
                         {4000, "2.5"},
                         {5000, "2"},
                         {6250, "1.6"},
                         {7692, "1.3"},
                         {10000, "1s"},
                         {13000, "1.3s"},
                         {16000, "1.6s"},
                         {20000, "2s"},
                         {25000, "2.5s"},
                         {30000, "3s"},
                         {40000, "4s"},
                         {50000, "5s"},
                         {60000, "6s"},
                         {80000, "8s"},
                         {100000, "10s"},
                         {130000, "13s"},
                         {150000, "15s"},
                         {200000, "20s"},
                         {250000, "25s"},
                         {300000, "30s"}
                       };
      ExposureModeTable = new Dictionary<int, string>()
                            {
                              {1, "M"},
                              {2, "P"},
                              {3, "A"},
                              {4, "S"},
                            };

      WbTable = new Dictionary<int, string>()
                  {
                    {2, "Auto"},
                    {4, "Daylight"},
                    {5, "Fluorescent "},
                    {6, "Incandescent"},
                    {7, "Flash"},
                    {32784, "Cloudy"},
                    {32785, "Shade"},
                    {32786, "Kelvin"},
                    {32787, "Custom"}
                  };
    }

    private void DeviceManager_OnEvent(string eventId, string deviceId, string itemId)
    {
      if (eventId == Conts.wiaEventDeviceConnected)
      {
        ConnectToCamera();
      }
      else if (eventId == Conts.wiaEventDeviceDisconnected && deviceId == DeviceId)
      {
        DisconnectCamera();
      }
      else if (eventId == Conts.wiaEventItemCreated && deviceId == DeviceId)
      {
        Item item = Device.GetItem(itemId);
        if (PhotoTaked != null)
          PhotoTaked(item);
      }
    }

    public void DisconnectCamera()
    {
      IsConected = false;
      DeviceName = "";
      Manufacturer = "";
      if (Device != null)
        Marshal.ReleaseComObject(Device);
      Device = null;
    }

    public int GetShutterValue(string s)
    {
      foreach (KeyValuePair<int, string> keyValuePair in ShutterTable)
      {
        if (keyValuePair.Value == s)
          return keyValuePair.Key;
      }
      return -1;
    }

    public bool Syncronize()
    {
      if (ConnectToCamera())
      {
        try
        {
          Device.ExecuteCommand("{9B26B7B2-ACAD-11D2-A093-00C04F72DC3C}");
        }
        catch (Exception)
        {

          return false;
        }
      }
      return false;
    }

    public bool TakePicture()
    {
      //14 4d545058-1a2e-4106-a357-771e0819fc56
      Device.ExecuteCommand(Conts.wiaCommandTakePicture);
      return false;
    }

    public bool ConnectToCamera()
    {
      try
      {
        // Device is already connected
        if (Device != null)
          return true;

        foreach (IDeviceInfo DevInfo in new DeviceManager().DeviceInfos)
        {
          // Look for CameraDeviceType devices
          if (DevInfo.Type == WiaDeviceType.CameraDeviceType)
          {
            DeviceId = DevInfo.DeviceID;
            Device = DevInfo.Connect();
            //Log(DateTime.Now.ToString() + " Digital Still Camera Connected\r\n");
            DeviceName = Device.Properties["Description"].get_Value();
            Manufacturer = Device.Properties["Manufacturer"].get_Value();

            if (WbTable.ContainsKey(Device.Properties["White Balance"].get_Value()))
              WhiteBalance = WbTable[Device.Properties["White Balance"].get_Value()];


            if (ExposureModeTable.ContainsKey(Device.Properties["Exposure Mode"].get_Value()))
              ExposureMode = ExposureModeTable[Device.Properties["Exposure Mode"].get_Value()];



            IsConected = true;
            return true;
          }
        }
        // Not a still camera
        return false;

      }
      catch (Exception exp)
      {
        //Log("Something went wrong when connecting the camera\r\n", exp.ToString());
        return false;
      }
    }

    #region events

    public delegate void PhotoTakedEventHandler(Item imageFile);

    public virtual event PhotoTakedEventHandler PhotoTaked;

    #endregion


    #region Implementation of INotifyPropertyChanged

    public virtual event PropertyChangedEventHandler PropertyChanged;

    public virtual void NotifyPropertyChanged(String info)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
    }

    #endregion
  }
}
