using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CameraControl.Devices.Classes;
using PortableDeviceLib;

namespace CameraControl.Devices.Nikon
{
  
  public class NikonD300 : NikonD3X
  {

    public override void StartLiveView()
    {
      if (!CaptureInSdRam)
        SetProperty(CONST_CMD_SetDevicePropValue, new[] {(byte) 1}, CONST_PROP_RecordingMedia, -1);
      DeviceReady();
      base.StartLiveView();
    }

    public override void StopLiveView()
    {
      base.StopLiveView();
      DeviceReady();
      if (!CaptureInSdRam)
        SetProperty(CONST_CMD_SetDevicePropValue, new[] {(byte) 0}, CONST_PROP_RecordingMedia, -1);
      DeviceReady();
    }

    public override void CapturePhotoNoAf()
    {
      lock (Locker)
      {
        MTPDataResponse response = ExecuteReadDataEx(CONST_CMD_GetDevicePropValue, CONST_PROP_LiveViewStatus, -1);
        ErrorCodes.GetException(response.ErrorCode);
        // test if live view is on 
        if (response.Data != null && response.Data.Length > 0 && response.Data[0] > 0)
        {
          if (CaptureInSdRam)
          {
            DeviceReady();
            ErrorCodes.GetException(_stillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCaptureRecInSdram, 0xFFFFFFFF));
            return;
          }
          StopLiveView();
        }

        DeviceReady();
        byte oldval = 0;
        byte[] val = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_AFModeSelect, -1);
        if (val != null && val.Length > 0)
          oldval = val[0];
        SetProperty(CONST_CMD_SetDevicePropValue, new[] { (byte)4 }, CONST_PROP_AFModeSelect, -1);
        DeviceReady();
        ErrorCodes.GetException(_stillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCapture));
        if (val != null && val.Length > 0)
          SetProperty(CONST_CMD_SetDevicePropValue, new[] { oldval }, CONST_PROP_AFModeSelect, -1);
      }
    }
  }
}
