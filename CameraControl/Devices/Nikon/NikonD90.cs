using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices.Nikon
{
  public class NikonD90:NikonBase
  {

    public override bool Init(DeviceDescriptor deviceDescriptor)
    {
      bool res = base.Init(deviceDescriptor);
      Capabilities.Clear();
      Capabilities.Add(CapabilityEnum.LiveView);
      Capabilities.Add(CapabilityEnum.RecordMovie);
      return res;
    }

    override public LiveViewData GetLiveViewImage()
    {
      LiveViewData viewData = new LiveViewData();
      viewData.HaveFocusData = true;

      const int headerSize = 128;

      byte[] result = _stillImageDevice.ExecuteReadData(CONST_CMD_GetLiveViewImage);
      if (result == null || result.Length <= headerSize)
        return null;
      int cbBytesRead = result.Length;
      GetAditionalLIveViewData(viewData, result);

      MemoryStream copy = new MemoryStream((int)cbBytesRead - headerSize);
      copy.Write(result, headerSize, (int)cbBytesRead - headerSize);
      copy.Close();
      viewData.ImageData = copy.GetBuffer();

      return viewData;
    }

    /// <summary>
    /// Take picture with no autofocus
    /// If live view runnig the live view is stoped after done restarted
    /// </summary>
    public override void CapturePhotoNoAf()
    {
      lock (Locker)
      {
        byte[] live = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_LiveViewStatus, -1);
        if (live != null && live.Length > 0 && live[0] == 1)
          StopLiveView();
        // the focus mode can be sett only in host mode
        LockCamera();
        byte oldval = 0;
        byte[] val = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_AFModeSelect, -1);
        if (val != null && val.Length > 0)
          oldval = val[0];
        ErrorCodes.GetException(_stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, new[] { (byte)4 },
                                                                   CONST_PROP_AFModeSelect, -1));
                ErrorCodes.GetException(CaptureInSdRam
                                  ? _stillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCaptureRecInSdram, 0xFFFFFFFF)
                                  : _stillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCapture));
        if (val != null && val.Length > 0)
          ErrorCodes.GetException(_stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, new[] { oldval },
                                                                     CONST_PROP_AFModeSelect, -1));
        UnLockCamera();
        if (live != null && live.Length > 0 && live[0] == 1)
          StartLiveView();
      }
    }


  }
}
