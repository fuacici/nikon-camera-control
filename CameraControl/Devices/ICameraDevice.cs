using System;
using CameraControl.Classes;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices
{
  public interface ICameraDevice
  {
    bool HaveLiveView { get; set; }
    bool CaptureInSdRam { get; set; }
    PropertyValue<int> FNumber { get; set; }
    PropertyValue<int> IsoNumber { get; set; }
    PropertyValue<long> ShutterSpeed { get; set; }
    PropertyValue<long> WhiteBalance { get; set; }
    PropertyValue<uint> Mode { get; set; }
    PropertyValue<int> ExposureCompensation { get; set; }
    PropertyValue<int> CompressionSetting { get; set; }
    PropertyValue<int> ExposureMeteringMode { get; set; }
    PropertyValue<uint> FocusMode { get; set; }
    bool IsConnected { get; set; }
    bool IsChecked { get; set; }
    PhotoSession AttachedPhotoSession { get; set; }
    string DeviceName { get; set; }
    string Manufacturer { get; set; }
    string SerialNumber { get; set; }
    string DisplayName { get; set; }
    int	ExposureStatus { get; set; }
    bool GetCapability(CapabilityEnum capabilityEnum);

    int Battery { get; set; }
    PropertyValue<int> LiveViewImageZoomRatio { get; set; }

    bool Init(DeviceDescriptor deviceDescriptor);
    void StartLiveView();
    void StopLiveView();
    LiveViewData GetLiveViewImage();
    void AutoFocus();
    void Focus(int step);
    void Focus(int x, int y);
    void CapturePhotoNoAf();
    void CapturePhoto();
    void StartRecordMovie();
    void StopRecordMovie();

    /// <summary>
    /// Support only if capability Bulb is specified
    /// </summary>
    void EndBulbMode();

    void StartBulbMode();

    void LockCamera();
    void UnLockCamera();
    void Close();
    //void ReadDeviceProperties(int prop);
    void TransferFile(object o, string filename);

    event PhotoCapturedEventHandler PhotoCaptured;
    event EventHandler CaptureCompleted;
  }
}