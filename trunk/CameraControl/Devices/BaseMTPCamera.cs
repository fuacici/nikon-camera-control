using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CameraControl.Devices.Classes;
using PortableDeviceLib;

namespace CameraControl.Devices
{
  public class BaseMTPCamera : BaseCameraDevice
  {
    protected StillImageDevice _stillImageDevice = null;


    public MTPDataResponse ExecuteReadDataEx(int code, int param1, int param2)
    {
      return ExecuteReadDataEx(code, param1, param2, 100, 0);
    }


    public MTPDataResponse ExecuteReadDataEx(int code, int param1, int param2, int loop, int counter)
    {
      MTPDataResponse res=new MTPDataResponse();
      res = _stillImageDevice.ExecuteReadDataEx(code, param1, param2);
      if ((res.ErrorCode == ErrorCodes.MTP_Device_Busy || res.ErrorCode == PortableDeviceErrorCodes.ERROR_BUSY) && loop < counter)
      {
        Thread.Sleep(10);
        loop++;
        return ExecuteReadDataEx(code, param1, param2, loop, counter);
      }
      return res;
    }

  }
}
