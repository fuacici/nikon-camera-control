﻿using System.Threading;
using CameraControl.Core.Devices.Classes;
using CameraControl.Devices.Classes;
using PortableDeviceLib;

namespace CameraControl.Devices
{
  public class BaseMTPCamera : BaseCameraDevice
  {
    private const int CONST_READY_TIME = 1;
    private const int CONST_LOOP_TIME = 1000;

    protected StillImageDevice _stillImageDevice = null;


    public MTPDataResponse ExecuteReadDataEx(int code)
    {
      return ExecuteReadDataEx(code, -1, -1, CONST_LOOP_TIME, 0);
    }

    public MTPDataResponse ExecuteReadDataEx(int code, int param1, int param2)
    {
      return ExecuteReadDataEx(code, param1, param2, CONST_LOOP_TIME, 0);
    }

    public uint ExecuteWithNoData(int code, uint param1)
    {
      return ExecuteWithNoData(code, param1, CONST_LOOP_TIME, 0);
    }

    public uint ExecuteWithNoData(int code)
    {
      return ExecuteWithNoData(code, CONST_LOOP_TIME, 0);
    }


    public uint ExecuteWithNoData(int code, uint param1, int loop, int counter)
    {
      uint res = _stillImageDevice.ExecuteWithNoData(code, param1);
      if ((res == ErrorCodes.MTP_Device_Busy || res == PortableDeviceErrorCodes.ERROR_BUSY) &&  counter<loop)
      {
        Thread.Sleep(CONST_READY_TIME);
        counter++;
        return ExecuteWithNoData(code, param1, loop, counter);
      }
      return res;
    }

    public uint ExecuteWithNoData(int code,  int loop, int counter)
    {
      uint res = _stillImageDevice.ExecuteWithNoData(code);
      if ((res == ErrorCodes.MTP_Device_Busy || res == PortableDeviceErrorCodes.ERROR_BUSY) && counter < loop)
      {
        Thread.Sleep(CONST_READY_TIME);
        counter++;
        return ExecuteWithNoData(code, loop, counter);
      }
      return res;
    }

    public MTPDataResponse ExecuteReadDataEx(int code, int param1, int param2, int loop, int counter)
    {
      MTPDataResponse res=new MTPDataResponse();
      res = _stillImageDevice.ExecuteReadDataEx(code, param1, param2);
      if ((res.ErrorCode == ErrorCodes.MTP_Device_Busy || res.ErrorCode == PortableDeviceErrorCodes.ERROR_BUSY) && counter < loop)
      {
        Thread.Sleep(CONST_READY_TIME);
        counter++;
        return ExecuteReadDataEx(code, param1, param2, loop, counter);
      }
      return res;
    }

  }
}
