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

    public PropertyValue<int> FNumber { get; set; }

    public PropertyValue<int> IsoNumber { get; set; }

    public PropertyValue<long> ShutterSpeed { get; set; }

    public PropertyValue<long> WhiteBalance { get; set; }

    public PropertyValue<uint> Mode { get; set; }

    public PropertyValue<int> ExposureCompensation { get; set; }

    public PropertyValue<int> CompressionSetting { get; set; }

    public PropertyValue<int> ExposureMeteringMode { get; set; }

    public PropertyValue<uint> FocusMode { get; set; }
    public bool IsConected { get; set; }
    public string DeviceName { get; set; }
    public string Manufacturer { get; set; }


    public int Battery { get; set; }

    public byte LiveViewImageZoomRatio { get; set; }

    public bool Init(string id, WIAManager manager)
    {
      HaveLiveView = false;
      return true;
    }

    public void StartLiveView()
    {
     
    }

    public void StopLiveView()
    {
     
    }

    public LiveViewData GetLiveViewImage()
    {
      return new LiveViewData();
    }

    public void AutoFocus()
    {
     
    }

    public void Focus(int step)
    {
     
    }

    public void Focus(int x, int y)
    {
     
    }

    public void TakePictureNoAf()
    {
      
    }

    public void TakePicture()
    {
      
    }

    public void Close()
    {
      
    }

    public void ReadDeviceProperties(int prop)
    {
      
    }

    #endregion

    public NotConnectedCameraDevice()
    {
      IsConected = false;
      HaveLiveView = false;
    }
  }
}
