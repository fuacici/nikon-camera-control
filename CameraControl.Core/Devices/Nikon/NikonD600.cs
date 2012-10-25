using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CameraControl.Core.Devices.Classes;

namespace CameraControl.Core.Devices.Nikon
{
  public class NikonD600 : NikonD800
  {
    public override LiveViewData GetLiveViewImage()
    {
      LiveViewData viewData = new LiveViewData();
      if (Monitor.TryEnter(Locker, 10))
      {
        try
        {
          //DeviceReady();
          viewData.HaveFocusData = true;

          const int headerSize = 376;

          byte[] result = StillImageDevice.ExecuteReadData(CONST_CMD_GetLiveViewImage);
          if (result == null || result.Length <= headerSize)
            return null;
          GetAditionalLIveViewData(viewData, result);
          viewData.ImagePosition = headerSize;
          viewData.ImageData = result;
        }
        finally
        {
          Monitor.Exit(Locker);
        }
      }
      return viewData;
    }

    protected override void GetAditionalLIveViewData(LiveViewData viewData, byte[] result)
    {
      viewData.LiveViewImageWidth = ToInt16(result, 8);
      viewData.LiveViewImageHeight = ToInt16(result, 10);

      viewData.ImageWidth = ToInt16(result, 12);
      viewData.ImageHeight = ToInt16(result, 14);

      viewData.FocusFrameXSize = ToInt16(result, 24);
      viewData.FocusFrameYSize = ToInt16(result, 26);

      viewData.FocusX = ToInt16(result, 28);
      viewData.FocusY = ToInt16(result, 30);

      viewData.Focused = result[48] != 1;
    }


  }
}
