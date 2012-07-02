using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices.Nikon
{
  public class NikonD80 : NikonD5100
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

    /// <summary>
    /// Take picture with no autofocus
    /// If live view runnig the live view is stoped after done restarted
    /// </summary>
    public override void TakePictureNoAf()
    {
      lock (Locker)
      {
        byte oldval = 0;
        byte[] val = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_AFModeSelect, -1);
        if (val != null && val.Length > 0)
          oldval = val[0];

        ErrorCodes.GetException(_stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, new[] { (byte)4 },
                                                                   CONST_PROP_AFModeSelect, -1));
        ErrorCodes.GetException(_stillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCapture));
        if (val != null && val.Length > 0)
          ErrorCodes.GetException(_stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, new[] { oldval },
                                                                     CONST_PROP_AFModeSelect, -1));
      }
    }

  }
}
