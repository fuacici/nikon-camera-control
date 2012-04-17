using CameraControl.Classes;

namespace CameraControl.Devices
{
  public interface ICameraDevice
  {
    bool Init(string id, WIAManager manager);
    void StartLiveView();
    void StopLiveView();
    byte[] GetLiveViewImage();
    void AutoFocus();
    void TakePictureNoAf();
    void Close();
  }
}