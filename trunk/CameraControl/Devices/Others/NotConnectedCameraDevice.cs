using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Classes;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices.Others
{
  public class NotConnectedCameraDevice:ICameraDevice
  {
    #region Implementation of ICameraDevice

    public bool HaveLiveView { get; set; }

    public PropertyValue FNumber { get; set; }

    public PropertyValue IsoNumber { get; set; }

    public PropertyValue ShutterSpeed { get; set; }

    public PropertyValue WhiteBalance { get; set; }

    public PropertyValue Mode { get; set; }

    public PropertyValue ExposureCompensation { get; set; }

    public int Battery { get; set; }

    public byte LiveViewImageZoomRatio { get; set; }

    public bool Init(string id, WIAManager manager)
    {
      HaveLiveView = false;
      return true;
    }

    public void StartLiveView()
    {
      throw new NotImplementedException();
    }

    public void StopLiveView()
    {
      throw new NotImplementedException();
    }

    public LiveViewData GetLiveViewImage()
    {
      throw new NotImplementedException();
    }

    public void AutoFocus()
    {
      throw new NotImplementedException();
    }

    public void Focus(int step)
    {
      throw new NotImplementedException();
    }

    public void Focus(int x, int y)
    {
      throw new NotImplementedException();
    }

    public void TakePictureNoAf()
    {
      throw new NotImplementedException();
    }

    public void Close()
    {
      throw new NotImplementedException();
    }

    public void ReadDeviceProperties()
    {
      
    }

    #endregion
  }
}
