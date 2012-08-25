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

    public override bool Init(DeviceDescriptor deviceDescriptor)
    {
      bool res = base.Init(deviceDescriptor);
      Capabilities.Clear();
      Capabilities.Add(CapabilityEnum.LiveView);
      Capabilities.Add(CapabilityEnum.RecordMovie);
      return res;
    }


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

    public override void CapturePhotoNoAf()
    {
      ServiceProvider.Log.Debug("CapturePhotoNoAf: Started");
      //lock (Locker)
      //{
        DeviceReady();
        ServiceProvider.Log.Debug("CapturePhotoNoAf: Step 1");
        byte oldval = 0;
        byte[] val = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_AFModeSelect, -1);
        if (val != null && val.Length > 0)
          oldval = val[0];
        ServiceProvider.Log.Debug("CapturePhotoNoAf: Step 2");
        SetProperty(CONST_CMD_SetDevicePropValue, new[] { (byte)4 }, CONST_PROP_AFModeSelect, -1);
        ServiceProvider.Log.Debug("CapturePhotoNoAf: Step 3");
        DeviceReady();
        ServiceProvider.Log.Debug("CapturePhotoNoAf: Step 4");
        ErrorCodes.GetException(CaptureInSdRam
                                  ? _stillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCaptureRecInSdram, 0xFFFFFFFF)
                                  : _stillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCapture));
        ServiceProvider.Log.Debug("CapturePhotoNoAf: Step 5");
        if (val != null && val.Length > 0)
          SetProperty(CONST_CMD_SetDevicePropValue, new[] { oldval }, CONST_PROP_AFModeSelect, -1);
        ServiceProvider.Log.Debug("CapturePhotoNoAf: Step 6");
      //}
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
