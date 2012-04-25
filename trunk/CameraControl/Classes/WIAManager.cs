using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Media.Imaging;
using CameraControl.Devices;
using WIA;
using DeviceManager = WIA.DeviceManager;

namespace CameraControl.Classes
{
  public class WIAManager : INotifyPropertyChanged
  {
    public DeviceManager DeviceManager { get; set; }
    public Device Device { get; set; }
    public string DeviceId { get; set; }
    public ICameraDevice CameraDevice { get; set; }

    public const string CONST_PROP_F_Number = "F Number";
    public const string CONST_PROP_ISO_Number = "Exposure Index";
    public const string CONST_PROP_Exposure_Time = "Exposure Time";
    public const string CONST_PROP_WhiteBalance = "White Balance";
    public const string CONST_PROP_ExposureMode = "Exposure Mode";
    public const string CONST_PROP_ExposureCompensation = "Exposure Compensation";
    
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
      if (CameraDevice != null)
        CameraDevice.Close();
      ServiceProvider.Settings.SystemMessage = "Camera disconnected !";
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
      Device.ExecuteCommand(Conts.wiaCommandTakePicture);
      return false;
    }

    public bool ConnectToCamera()
    {
      return ConnectToCamera(true);
    }

    public bool ConnectToCamera(bool retry)
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
            Thread.Sleep(500);
            //Log(DateTime.Now.ToString() + " Digital Still Camera Connected\r\n");
            DeviceName = Device.Properties["Description"].get_Value();
            Manufacturer = Device.Properties["Manufacturer"].get_Value();

            IsConected = true;
            CameraDevice = ServiceProvider.DeviceManager.GetIDevice(this);
            ServiceProvider.DeviceManager.SelectedCameraDevice.ReadDeviceProperties();
            ServiceProvider.Settings.SystemMessage = "Camera is connected !";
            return true;
          }
        }
        // Not a still camera
        return false;

      }
      catch (Exception exp)
      {
        if (retry)
        {
          Thread.Sleep(500);
          return ConnectToCamera(false);
        }
        ServiceProvider.Log.Error("Unable to connect to the camera", exp);
        ServiceProvider.Settings.SystemMessage = "Unable to connect to the camera. Please reconnect your camera !";
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
