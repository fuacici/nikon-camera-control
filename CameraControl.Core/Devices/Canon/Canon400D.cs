using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CameraControl.Core.Devices.Classes;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using PortableDeviceLib;

namespace CameraControl.Core.Devices.Canon
{
  public class Canon400D:BaseMTPCamera
  {
    public const int CONST_CMD_InitiateCapture = 0x100E;

    public override bool Init(DeviceDescriptor deviceDescriptor)
    {
      StillImageDevice = new StillImageDevice(deviceDescriptor.WpdId);
      StillImageDevice.ConnectToDevice(AppName, AppMajorVersionNumber, AppMinorVersionNumber);
      StillImageDevice.DeviceEvent += _stillImageDevice_DeviceEvent;
      DeviceName = StillImageDevice.Model;
      Manufacturer = StillImageDevice.Manufacturer;
      return true;
    }

    public override void CapturePhoto()
    {
      Monitor.Enter(Locker);
      try
      {
        IsBusy = true;
        //ErrorCodes.GetException(CaptureInSdRam
        //                          ? ExecuteWithNoData(CONST_CMD_AfAndCaptureRecInSdram)
        //                          : ExecuteWithNoData(CONST_CMD_InitiateCapture));
        ErrorCodes.GetException(ExecuteWithNoData(CONST_CMD_InitiateCapture));
      }
      catch (COMException comException)
      {
        IsBusy = false;
        ErrorCodes.GetException(comException);
      }
      catch
      {
        IsBusy = false;
        throw;
      }
      finally
      {
        Monitor.Exit(Locker);
      }
    }

    void _stillImageDevice_DeviceEvent(object sender, PortableDeviceEventArgs e)
    {
      if (e.EventType.EventGuid == PortableDeviceGuids.WPD_EVENT_DEVICE_REMOVED)
      {
        StillImageDevice.Disconnect();
        StillImageDevice.IsConnected = false;
        IsConnected = false;
        ServiceProvider.DeviceManager.DisconnectCamera(StillImageDevice);
      }
    }
  }
}
