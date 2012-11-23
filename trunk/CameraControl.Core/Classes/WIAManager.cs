using System;
using System.ComponentModel;
using System.Threading;
using CameraControl.Core.Devices.Classes;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using WIA;

namespace CameraControl.Core.Classes
{
  //public class WIAManager 
  //{
  //  public DeviceManager WiaDeviceManager { get; set; }

  //  /// <summary>
  //  /// Gets or sets a value for disabling native drivers.
  //  /// </summary>
  //  /// <value>
  //  /// <c>true</c> all devices are loaded like WIA devices <c>false</c> If native driver are available for connected model the will be loaded that driver else will be loaded WIA driver.
  //  /// </value>
  //  public bool DisableNativeDrivers { get; set; }

  //  public WIAManager()
  //  {
  //    WiaDeviceManager = new DeviceManager();
  //    WiaDeviceManager.RegisterEvent(Conts.wiaEventDeviceConnected, "*");
  //    WiaDeviceManager.RegisterEvent(Conts.wiaEventDeviceDisconnected, "*");
  //    WiaDeviceManager.OnEvent += DeviceManager_OnEvent;
  //  }

  //  private void DeviceManager_OnEvent(string eventId, string deviceId, string itemId)
  //  {
  //    if (eventId == Conts.wiaEventDeviceConnected)
  //    {
  //      ConnectToCamera();
  //    }
  //    else if (eventId == Conts.wiaEventDeviceDisconnected)
  //    {
  //      ServiceProvider.DeviceManager.DisconnectCamera(deviceId);
  //    }
  //  }


  //  public bool ConnectToCamera()
  //  {
  //    return ConnectToCamera(true);
  //  }

  //  public bool ConnectToCamera(bool retry)
  //  {
  //    if (!DisableNativeDrivers)
  //    {
  //      ServiceProvider.DeviceManager.ConnectDevices();
  //    }
  //    else
  //    {
  //      Log.Debug("Native drivers are disabled !!!!");
  //    }
  //    bool ret = false;
  //    int retries = 0;
  //    foreach (IDeviceInfo devInfo in new DeviceManager().DeviceInfos)
  //    {
  //      // Look for CameraDeviceType devices
  //      string model = devInfo.Properties["Name"].get_Value();
  //      if (devInfo.Type == WiaDeviceType.CameraDeviceType && (!ServiceProvider.DeviceManager.DeviceClass.ContainsKey(model) || DisableNativeDrivers))
  //      {
  //        do
  //        {
  //          try
  //          {
  //            ServiceProvider.DeviceManager.GetWiaIDevice(this, devInfo);
  //            retries = 4;
  //          }
  //          catch (Exception exception)
  //          {
  //            Log.Error("Unable to connect to the camera", exception);
  //            retries++;
  //            if (retries < 3)
  //            {
  //              Log.Debug("Retrying");
  //             StaticHelper.Instance.SystemMessage = "Unable to connect to the camera. Retrying";
  //            }
  //            else
  //            {
  //             StaticHelper.Instance.SystemMessage =
  //                "Unable to connect to the camera. Please reconnect your camera !";
  //            }
  //            Thread.Sleep(1000);
  //          }
  //        } while (retries < 3);
  //        ret = true;
  //      }
  //    }
  //    return ret;
  //  }

  //}
}
