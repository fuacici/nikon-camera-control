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
    public const string CONST_PROP_BatteryStatus = "Battery Status";
    public const string CONST_PROP_CompressionSetting = "Compression Setting";
    public const string CONST_PROP_ExposureMeteringMode = "Exposure Metering Mode";
    public const string CONST_PROP_FocusMode = "Focus Mode";

    //private string _deviceName;

    //public string DeviceName
    //{
    //  get { return _deviceName; }
    //  set
    //  {
    //    _deviceName = value;
    //    NotifyPropertyChanged("DeviceName");
    //  }
    //}

    //private string _manufacturer;

    //public string Manufacturer
    //{
    //  get { return _manufacturer; }
    //  set
    //  {
    //    _manufacturer = value;
    //    NotifyPropertyChanged("Manufacturer");
    //  }
    //}

    //private bool _isConected;

    //public bool IsConected
    //{
    //  get { return _isConected; }
    //  set
    //  {
    //    _isConected = value;
    //    NotifyPropertyChanged("IsConected");
    //  }
    //}

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
    }

    private void DeviceManager_OnEvent(string eventId, string deviceId, string itemId)
    {
      if (eventId == Conts.wiaEventDeviceConnected)
      {
        ConnectToCamera();
      }
      else if (eventId == Conts.wiaEventDeviceDisconnected)
      {
        ServiceProvider.DeviceManager.DisconnectCamera(deviceId);
      }
      else if (eventId == Conts.wiaEventItemCreated)
      {
        Item item = Device.GetItem(itemId);
        if (PhotoTaked != null)
          PhotoTaked(item);
      }
    }

    public void OnPhotoTakenDone()
    {
      if (PhotoTakenDone != null)
        PhotoTakenDone(null);
    }

    //public void DisconnectCamera()
    //{
    //  //////IsConected = false;
    //  //if (Device != null)
    //  //  Marshal.ReleaseComObject(Device);
    //  //Device = null;
    //  //if (CameraDevice != null)
    //  //  CameraDevice.Close();
    //  ServiceProvider.Settings.SystemMessage = "Camera disconnected !";
    //}

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

    public bool ConnectToCamera()
    {
      return ConnectToCamera(true);
    }

    public bool ConnectToCamera(bool retry)
    {
      bool ret = false;
      foreach (IDeviceInfo devInfo in new DeviceManager().DeviceInfos)
      {
        // Look for CameraDeviceType devices
        if (devInfo.Type == WiaDeviceType.CameraDeviceType)
        {
          try
          {
            ServiceProvider.DeviceManager.GetIDevice(this, devInfo);
          }
          catch (Exception exception)
          {
            ServiceProvider.Log.Error("Unable to connect to the camera", exception);
            ServiceProvider.Settings.SystemMessage = "Unable to connect to the camera. Please reconnect your camera !";
          }
          ret = true;
        }
      }
      return ret;
    }


    #region events

    public delegate void PhotoTakedEventHandler(Item imageFile);

    public virtual event PhotoTakedEventHandler PhotoTaked;
    public virtual event PhotoTakedEventHandler PhotoTakenDone;

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
