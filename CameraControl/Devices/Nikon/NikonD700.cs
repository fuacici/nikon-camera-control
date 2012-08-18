using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices.Nikon
{
  public class NikonD700:NikonBase
  {
    public const int CONST_PROP_RecordingMedia = 0xD10B;

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


    override public LiveViewData GetLiveViewImage()
    {
      LiveViewData viewData = new LiveViewData();
      viewData.HaveFocusData = true;

      const int headerSize = 64;

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
  }
}
