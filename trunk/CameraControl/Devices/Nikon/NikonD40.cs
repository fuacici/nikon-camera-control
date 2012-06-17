using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices.Nikon
{
  public class NikonD40:NikonD5100
  {
    public override bool Init(DeviceDescriptor deviceDescriptor)
    {
      bool res = base.Init(deviceDescriptor);
      HaveLiveView = false;
      return res;
    }

    public override void ReadDeviceProperties(int prop)
    {
      base.ReadDeviceProperties(prop);
      HaveLiveView = false;
    }
  }
}
