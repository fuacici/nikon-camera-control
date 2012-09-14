using CameraControl.Core.Devices.Classes;

namespace CameraControl.Core.Devices.Nikon
{
  public class NikonD40:NikonBase
  {

    public override bool Init(DeviceDescriptor deviceDescriptor)
    {
      bool res = base.Init(deviceDescriptor);
      Capabilities.Clear();
      HaveLiveView = false;
      CaptureInSdRam = false;
      return res;
    }

    public override void ReadDeviceProperties(int prop)
    {
      base.ReadDeviceProperties(prop);
      HaveLiveView = false;
    }
  }
}
