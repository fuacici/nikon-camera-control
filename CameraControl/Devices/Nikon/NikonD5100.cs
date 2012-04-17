using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CameraControl.Classes;
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

    public byte[] GetLiveViewImage()
    {
      const int headerSize = 384;

      byte[] result = _stillImageDevice.ExecuteWithData(CONST_CMD_GetLiveViewImage);
      if (result == null || result.Length <= headerSize)
        return null;
      int cbBytesRead = result.Length;


      MemoryStream copy = new MemoryStream((int)cbBytesRead - headerSize);
      copy.Write(result, headerSize, (int)cbBytesRead - headerSize);
      copy.Close();
      result = copy.GetBuffer();
      return result;
    }

    public void AutoFocus()
    {
      _stillImageDevice.ExecuteWithNoData(CONST_CMD_AfDrive);
    }

    public void TakePictureNoAf()
    {
      _stillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCaptureRecInMedia, 0xFFFFFFFF, 0x0000);
    }

    public void Close()
    {
      _stillImageDevice.Disconnect();
    }

  }
}
