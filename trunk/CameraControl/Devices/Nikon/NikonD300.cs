using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices.Nikon
{
  
  public class NikonD300 : NikonD3X
  {

    public const int CONST_PROP_RecordingMedia = 0xD10B;

    public override void CapturePhotoNoAf()
    {
      base.CapturePhoto();
    }

    public override void StartLiveView()
    {
      SetProperty(CONST_CMD_SetDevicePropValue, new[] {(byte) 1}, CONST_PROP_RecordingMedia, -1);
      DeviceReady();
      base.StartLiveView();
    }

    public override void StopLiveView()
    {
      base.StopLiveView();
      DeviceReady();
      SetProperty(CONST_CMD_SetDevicePropValue, new[] {(byte) 0}, CONST_PROP_RecordingMedia, -1);
      DeviceReady();
    }
  }
}
