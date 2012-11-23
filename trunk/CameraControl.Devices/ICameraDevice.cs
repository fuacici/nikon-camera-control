using System;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices
{
  public interface ICameraDevice
  {
    /// <summary>
    /// If false the camera is ready to take next capture
    /// Should be handled by the user code
    /// </summary>
    bool IsBusy { get; set; }
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
    object AttachedPhotoSession { get; set; }
    string DeviceName { get; set; }
    string Manufacturer { get; set; }
    string SerialNumber { get; set; }
    string DisplayName { get; set; }
    int	ExposureStatus { get; set; }
    bool GetCapability(CapabilityEnum capabilityEnum);
    uint TransferProgress { get; set; }

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
    string GetProhibitionCondition(OperationEnum operationEnum);
    /// <summary>
    /// Support only if capability Bulb is specified
    /// </summary>
    void EndBulbMode();

    void StartBulbMode();

    void LockCamera();
    void UnLockCamera();
    void Close();

    void TransferFile(object o, string filename);

    event PhotoCapturedEventHandler PhotoCaptured;
    event EventHandler CaptureCompleted;
    event CameraDisconnectedEventHandler CameraDisconnected;

    AsyncObservableCollection<PropertyValue<long>> AdvancedProperties { get; set; }
    
    AsyncObservableCollection<DeviceObject> GetObjects(object storageId);
    bool DeleteObject(DeviceObject deviceObject);
  }
}