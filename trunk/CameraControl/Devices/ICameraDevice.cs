using CameraControl.Classes;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices
{
  public interface ICameraDevice
  {
    bool HaveLiveView { get; set; }
    PropertyValue FNumber { get; set; }
    PropertyValue IsoNumber { get; set; }
    PropertyValue ShutterSpeed { get; set; }
    PropertyValue WhiteBalance { get; set; }
    PropertyValue Mode { get; set; }
    PropertyValue ExposureCompensation { get; set; }
    PropertyValue CompressionSetting { get; set; }
    PropertyValue ExposureMeteringMode { get; set; }
    PropertyValue FocusMode { get; set; }

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
    void ReadDeviceProperties();
  }
}