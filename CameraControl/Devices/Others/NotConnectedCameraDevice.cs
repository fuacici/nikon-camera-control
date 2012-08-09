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

    public bool CaptureInSdRam { get; set; }

    public PropertyValue<int> FNumber { get; set; }

    public PropertyValue<int> IsoNumber { get; set; }

    public PropertyValue<long> ShutterSpeed { get; set; }

    public PropertyValue<long> WhiteBalance { get; set; }

    public PropertyValue<uint> Mode { get; set; }

    public PropertyValue<int> ExposureCompensation { get; set; }

    public PropertyValue<int> CompressionSetting { get; set; }

    public PropertyValue<int> ExposureMeteringMode { get; set; }

    public PropertyValue<uint> FocusMode { get; set; }
    public bool IsConnected { get; set; }
    public string DeviceName { get; set; }
    public string Manufacturer { get; set; }

    public string SerialNumber{ get; set; }

    public int ExposureStatus { get; set; }

    public bool GetCapability(CapabilityEnum capabilityEnum)
    {
      return false;
    }

    public int Battery { get; set; }

    public byte LiveViewImageZoomRatio { get; set; }

    public bool Init(DeviceDescriptor deviceDescriptor)
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

    public void CapturePhotoNoAf()
    {
      
    }

    public void CapturePhoto()
    {
      
    }

    public void EndBulbMode()
    {
      throw new NotImplementedException();
    }

    public void StartBulbMode()
    {
      throw new NotImplementedException();
    }

    public void LockCamera()
    {
      throw new NotImplementedException();
    }

    public void UnLockCamera()
    {
      throw new NotImplementedException();
    }

    public void Close()
    {
      
    }

    public void ReadDeviceProperties(int prop)
    {
      
    }

    public void TransferFile(object o, string filename)
    {
      
    }

    public event PhotoCapturedEventHandler PhotoCaptured;

    #endregion

    public NotConnectedCameraDevice()
    {
      IsConnected = false;
      HaveLiveView = false;
      ExposureStatus = 10;
    }
  }
}
