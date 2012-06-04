using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraControl.Devices.Nikon
{
  public class NikonD3200: NikonD5100
  {
    protected override void GetAditionalLIveViewData(Classes.LiveViewData viewData, byte[] result)
    {
      viewData.LiveViewImageWidth = ToInt16(result, 8);
      viewData.LiveViewImageHeight = ToInt16(result, 10);

      viewData.ImageWidth = ToInt16(result, 12);
      viewData.ImageHeight = ToInt16(result, 14);

      viewData.FocusFrameXSize = ToInt16(result, 16);
      viewData.FocusFrameYSize = ToInt16(result, 18);

      viewData.FocusX = ToInt16(result, 20);
      viewData.FocusY = ToInt16(result, 22);

      viewData.Focused = result[40] != 1;
    }
  }
}
