using CameraControl.Classes;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices
{
  public interface ICameraDevice
  {
    bool HaveLiveView { get; set; }
    PropertyValue<int> FNumber { get; set; }
    PropertyValue<int> IsoNumber { get; set; }
    PropertyValue<int> ShutterSpeed { get; set; }
    PropertyValue<int> WhiteBalance { get; set; }
    PropertyValue<int> Mode { get; set; }
    PropertyValue<int> ExposureCompensation { get; set; }
    PropertyValue<int> CompressionSetting { get; set; }
    PropertyValue<int> ExposureMeteringMode { get; set; }
    PropertyValue<uint> FocusMode { get; set; }

    int Battery { get; set; }
    byte LiveViewImageZoomRatio { get; set; }

    bool Init(string id, WIAManager manager);
    void StartLiveView();
    void StopLiveView();
    LiveViewData GetLiveViewImage();
    void AutoFocus();
    void Focus(int step);
    void Focus(int x, int y);
    void TakePictureNoAf();
    void TakePicture();
    void Close();
    void ReadDeviceProperties(int prop);
  }
}