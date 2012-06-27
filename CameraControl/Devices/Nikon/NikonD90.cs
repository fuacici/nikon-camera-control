using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices.Nikon
{
  public class NikonD90:NikonD5100
  {

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
    public virtual void TakePictureNoAf()
    {
      lock (Locker)
      {
        byte oldval = 0;
        byte[] val = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_AFModeSelect, -1);
        if (val != null && val.Length > 0)
          oldval = val[0];
        byte[] live = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_LiveViewStatus, -1);
        if (live != null && live.Length > 0 && live[0] == 1)
          StopLiveView();
        ErrorCodes.GetException(_stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, new[] { (byte)4 },
                                                                   CONST_PROP_AFModeSelect, -1));
        ErrorCodes.GetException(_stillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCapture));
        if (val != null && val.Length > 0)
          ErrorCodes.GetException(_stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, new[] { oldval },
                                                                     CONST_PROP_AFModeSelect, -1));
        if (live != null && live.Length > 0 && live[0] == 1)
          StartLiveView();
      }
    }


  }
}
