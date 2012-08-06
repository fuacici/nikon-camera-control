using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices.Nikon
{
  
  class NikonD300 : NikonD3X
  {
    public override void CapturePhotoNoAf()
    {
      base.CapturePhoto();
    }

  }
}
