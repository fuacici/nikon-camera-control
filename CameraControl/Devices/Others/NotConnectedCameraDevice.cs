using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Classes;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices.Others
{
  public class NotConnectedCameraDevice : BaseCameraDevice
  {
    #region Implementation of ICameraDevice


    public override bool Init(DeviceDescriptor deviceDescriptor)
    {
      HaveLiveView = false;
      return true;
    }


    #endregion

    public NotConnectedCameraDevice()
    {
      IsConnected = false;
      HaveLiveView = false;
      ExposureStatus = 1;
    }
  }
}
