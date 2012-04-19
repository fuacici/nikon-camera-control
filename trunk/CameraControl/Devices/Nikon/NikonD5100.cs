using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CameraControl.Classes;
using CameraControl.Devices.Classes;
using PortableDeviceLib;

namespace CameraControl.Devices.Nikon
{
  public class NikonD5100 :ICameraDevice
  {
    private const int CONST_CMD_AfDrive = 0x90C1;
    private const int CONST_CMD_StartLiveView = 0x9201;
    private const int CONST_CMD_EndLiveView = 0x9202;
    private const int CONST_CMD_GetLiveViewImage = 0x9203;
    private const int CONST_CMD_InitiateCaptureRecInMedia = 0x9207;
    private const int CONST_CMD_ChangeAfArea = 0x9205;
    private const int CONST_CMD_MfDrive = 0x9204;

    private const string AppName = "CameraControl";
    private const int AppMajorVersionNumber = 1;
    private const int AppMinorVersionNumber = 0;

    private StillImageDevice _stillImageDevice = null;
    private WIAManager _manager = null;

    public NikonD5100()
    {
    
    }

    //public NikonD5100(string id)
    //{
    //  Init(id);
    //}

    public bool Init(string id, WIAManager manager)
    {
      _manager = manager;
      _manager.HaveLiveView = true;
      _stillImageDevice = new StillImageDevice(id);
      _stillImageDevice.ConnectToDevice(AppName, AppMajorVersionNumber, AppMinorVersionNumber);
      return true;
    }

    public void StartLiveView()
    {
      _stillImageDevice.ExecuteWithNoData(CONST_CMD_StartLiveView);
    }

    public void StopLiveView()
    {
      _stillImageDevice.ExecuteWithNoData(CONST_CMD_EndLiveView);
    }

    public LiveViewData GetLiveViewImage()
    {
      LiveViewData viewData=new LiveViewData();
      viewData.HaveFocusData = true;

      const int headerSize = 384;

      byte[] result = _stillImageDevice.ExecuteWithData(CONST_CMD_GetLiveViewImage);
      if (result == null || result.Length <= headerSize)
        return null;
      int cbBytesRead = result.Length;

      viewData.LiveViewImageWidth = ToInt16(result, 0);
      viewData.LiveViewImageHeight = ToInt16(result, 2);

      viewData.ImageWidth = ToInt16(result, 4);
      viewData.ImageHeight = ToInt16(result, 6);

      viewData.FocusFrameXSize = ToInt16(result, 16);
      viewData.FocusFrameYSize = ToInt16(result, 18);

      viewData.FocusX = ToInt16(result, 20);
      viewData.FocusY = ToInt16(result, 22);

      viewData.Focused = result[40] != 1;
      MemoryStream copy = new MemoryStream((int)cbBytesRead - headerSize);
      copy.Write(result, headerSize, (int)cbBytesRead - headerSize);
      copy.Close();
      viewData.ImageData = copy.GetBuffer();

      return viewData;
    }

    public void Focus(int step)
    {
      if (step > 0)
        _stillImageDevice.ExecuteWithNoData(CONST_CMD_MfDrive, 0x00000001, (uint) step);
      else
        _stillImageDevice.ExecuteWithNoData(CONST_CMD_MfDrive, 0x00000002, (uint) -step);
    }

    public void AutoFocus()
    {
      _stillImageDevice.ExecuteWithNoData(CONST_CMD_AfDrive);
    }

    public void TakePictureNoAf()
    {
      _stillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCaptureRecInMedia, 0xFFFFFFFF, 0x0000);
    }

    public void Focus(int x, int y)
    {
      _stillImageDevice.ExecuteWithNoData(CONST_CMD_ChangeAfArea, (uint) x, (uint) y);
    }

    public void Close()
    {
      _stillImageDevice.Disconnect();
    }

    public static short ToInt16(byte[] value, int startIndex)
    {
        return System.BitConverter.ToInt16(value.Reverse().ToArray(), value.Length - sizeof(Int16) - startIndex);
    }

  }
}
