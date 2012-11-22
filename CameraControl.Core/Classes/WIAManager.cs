using System;
using System.ComponentModel;
using System.Threading;
using CameraControl.Core.Devices.Classes;
using CameraControl.Devices.Classes;
using WIA;

namespace CameraControl.Core.Classes
{
  public class WIAManager : INotifyPropertyChanged
  {
    public DeviceManager DeviceManager { get; set; }

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

    public WIAManager()
    {
      DeviceManager = new DeviceManager();
      DeviceManager.RegisterEvent(Conts.wiaEventDeviceConnected, "*");
      DeviceManager.RegisterEvent(Conts.wiaEventDeviceDisconnected, "*");
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
    }

    public void OnPhotoTakenDone()
    {
      if (PhotoTakenDone != null)
        PhotoTakenDone(null);
    }

    public bool ConnectToCamera()
    {
      return ConnectToCamera(true);
    }

    public bool ConnectToCamera(bool retry)
    {
      if (!ServiceProvider.Settings.DisableNativeDrivers)
      {
        ServiceProvider.DeviceManager.ConnectDevices();
      }
      else
      {
        Log.Debug("Native drivers are disabled !!!!");
      }
      bool ret = false;
      int retries = 0;
      foreach (IDeviceInfo devInfo in new DeviceManager().DeviceInfos)
      {
        // Look for CameraDeviceType devices
        string model = devInfo.Properties["Name"].get_Value();
        if (devInfo.Type == WiaDeviceType.CameraDeviceType && (!ServiceProvider.DeviceManager.DeviceClass.ContainsKey(model) || ServiceProvider.Settings.DisableNativeDrivers))
        {
          do
          {
            try
            {
              ServiceProvider.DeviceManager.GetWiaIDevice(this, devInfo);
              retries = 4;
            }
            catch (Exception exception)
            {
              Log.Error("Unable to connect to the camera", exception);
              retries++;
              if (retries < 3)
              {
                Log.Debug("Retrying");
               StaticHelper.Instance.SystemMessage = "Unable to connect to the camera. Retrying";
              }
              else
              {
               StaticHelper.Instance.SystemMessage =
                  "Unable to connect to the camera. Please reconnect your camera !";
              }
              Thread.Sleep(1000);
            }
          } while (retries < 3);
          ret = true;
        }
      }
      return ret;
    }


    #region events

    public delegate void PhotoTakedEventHandler(Item imageFile);


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
