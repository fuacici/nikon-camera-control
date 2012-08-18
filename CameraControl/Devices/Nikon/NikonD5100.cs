using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraControl.Devices.Nikon
{
  public class NikonD5100 : NikonBase
  {
    public override void StartLiveView()
    {
      SetProperty(CONST_CMD_SetDevicePropValue, new[] { (byte)1 }, CONST_PROP_RecordingMedia, -1);
      DeviceReady();
      base.StartLiveView();
    }

    public override void StopLiveView()
    {
      base.StopLiveView();
      DeviceReady();
      SetProperty(CONST_CMD_SetDevicePropValue, new[] { (byte)0 }, CONST_PROP_RecordingMedia, -1);
      DeviceReady();
    }

  }
}
