using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Core.Devices.Classes;

namespace CameraControl.Core.Devices.Nikon
{
  public class NikonD7000:NikonBase
  {
    public override bool Init(DeviceDescriptor deviceDescriptor)
    {
      bool res = base.Init(deviceDescriptor);
      Capabilities.Clear();
      Capabilities.Add(CapabilityEnum.LiveView);
      Capabilities.Add(CapabilityEnum.RecordMovie);
      Capabilities.Add(CapabilityEnum.CaptureInRam);
      Capabilities.Add(CapabilityEnum.CaptureNoAf);
      return res;
    }
  }
}
