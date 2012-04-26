using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraControl.Devices.Nikon
{
  public class NikonD40:NikonD5100
  {
    public override bool Init(string id, CameraControl.Classes.WIAManager manager)
    {
      bool res = base.Init(id, manager);
      HaveLiveView = false;
      return res;
    }

    public override void ReadDeviceProperties()
    {
      base.ReadDeviceProperties();
      HaveLiveView = false;
    }
  }
}
