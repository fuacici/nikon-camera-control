using CameraControl.Core.Devices.Classes;

namespace CameraControl.Core.Devices.Others
{
  public class NotConnectedCameraDevice : BaseCameraDevice
  {
    #region Implementation of ICameraDevice


    public override bool Init(DeviceDescriptor deviceDescriptor)
    {
      HaveLiveView = false;
      IsBusy = false;
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
