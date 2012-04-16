using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PortableDeviceLib;

namespace CameraControl.Devices.Nikon
{
  public class NikonD5100
  {
    private const int CONST_CMD_AfDrive = 0x90C1;
    private const int CONST_CMD_StartLiveView = 0x9201;
    private const int CONST_CMD_EndLiveView = 0x9202;
    private const int CONST_CMD_GetLiveViewImage = 0x9203;

    private const string AppName = "CameraControl";
    private const int AppMajorVersionNumber = 1;
    private const int AppMinorVersionNumber = 0;

    private StillImageDevice _stillImageDevice = null;

    public NikonD5100(string id)
    {
      Init(id);
    }

    public bool Init(string id)
    {
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

  }
}
