using System;
using System.Threading;
using CameraControl.Devices.Classes;
using PortableDeviceLib;
using Timer = System.Timers.Timer;

namespace CameraControl.Devices
{
  public class BaseMTPCamera : BaseCameraDevice
  {
    protected const string AppName = "CameraControl";
    protected const int AppMajorVersionNumber = 1;
    protected const int AppMinorVersionNumber = 0;

    // common MTP commands
    public const int CONST_CMD_GetDevicePropValue = 0x1015;
    public const int CONST_CMD_SetDevicePropValue = 0x1016;
    public const int CONST_CMD_GetDevicePropDesc = 0x1014;
    public const int CONST_CMD_GetObject = 0x1009;
    public const int CONST_CMD_GetObjectHandles = 0x1007;
    public const int CONST_CMD_GetObjectInfo = 0x1008;
    public const int CONST_CMD_GetThumb = 0x100A;
    public const int CONST_CMD_DeleteObject = 0x100B;


    private const int CONST_READY_TIME = 1;
    private const int CONST_LOOP_TIME = 100;

    protected StillImageDevice StillImageDevice = null;
    protected bool DeviceIsBusy = false;
    /// <summary>
    /// The timer for get periodically the event list
    /// </summary>
    protected Timer _timer = new Timer(1000 / 15);



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
      WaitForReady();
      uint res = 0;
      bool allok;
      do
      {
        allok = true;
        res = StillImageDevice.ExecuteWithNoData(code, param1);
        if ((res == ErrorCodes.MTP_Device_Busy || res == PortableDeviceErrorCodes.ERROR_BUSY) && counter < loop)
        {
          Thread.Sleep(CONST_READY_TIME);
          counter++;
          allok = false;
        }
      } while (!allok);
      return res;
    }

    public uint ExecuteWithNoData(int code, int loop, int counter)
    {
      WaitForReady();
      uint res = 0;
      bool allok;
      do
      {
        allok = true;
        res = StillImageDevice.ExecuteWithNoData(code);
        if ((res == ErrorCodes.MTP_Device_Busy || res == PortableDeviceErrorCodes.ERROR_BUSY) && counter < loop)
        {
          Thread.Sleep(CONST_READY_TIME);
          counter++;
          allok = false;
        }
      } while (!allok);
      return res;
    }

    public uint ExecuteWithNoData(int code, uint param1, uint param2)
    {
      uint res = StillImageDevice.ExecuteWithNoData(code, param1, param2);
      return res;
    }

    public MTPDataResponse ExecuteReadDataEx(int code, int param1, int param2, int loop, int counter)
    {
      WaitForReady();
      DeviceIsBusy = true;
      MTPDataResponse res = new MTPDataResponse();
      bool allok;
      do
      {
        allok = true;
        res = StillImageDevice.ExecuteReadDataEx(code, param1, param2);
        if ((res.ErrorCode == ErrorCodes.MTP_Device_Busy || res.ErrorCode == PortableDeviceErrorCodes.ERROR_BUSY) &&
            counter < loop)
        {
          Thread.Sleep(CONST_READY_TIME);
          counter++;
          allok = false;
        }
      } while (!allok);
      DeviceIsBusy = false;
      return res;
    }

    public MTPDataResponse ExecuteReadDataEx(int code, uint param1)
    {
      int counter = 0;
      WaitForReady();
      DeviceIsBusy = true;
      MTPDataResponse res = new MTPDataResponse();
      bool allok;
      do
      {
        res = StillImageDevice.ExecuteReadDataEx(code, param1);
        allok = true;
        if ((res.ErrorCode == ErrorCodes.MTP_Device_Busy || res.ErrorCode == PortableDeviceErrorCodes.ERROR_BUSY) &&
            counter < CONST_LOOP_TIME)
        {
          Thread.Sleep(CONST_READY_TIME);
          counter++;
          allok = false;
        }
      } while (!allok);
      DeviceIsBusy = false;
      return res;
    }


    protected void WaitForReady()
    {
      //while (DeviceIsBusy)
      //{
      //  Thread.Sleep(1);
      //}
    }

    protected void SetProperty(int code, byte[] data, int param1, int param2)
    {
      bool timerstate = _timer.Enabled;
      _timer.Stop();
      bool retry = false;
      int retrynum = 0;
      //DeviceReady();
      do
      {
        if (retrynum > 5)
        {
          return;
        }
        try
        {
          retry = false;
          uint resp = StillImageDevice.ExecuteWriteData(code, data, param1, param2);
          if (resp != 0 || resp != ErrorCodes.MTP_OK)
          {
            //Console.WriteLine("Retry ...." + resp.ToString("X"));
            if (resp == ErrorCodes.MTP_Device_Busy || resp == 0x800700AA)
            {
              Thread.Sleep(100);
              retry = true;
              retrynum++;
            }
            else
            {
              ErrorCodes.GetException(resp);
            }
          }
        }
        catch (Exception exception)
        {
          Log.Debug("Error set property :" + param1.ToString("X"), exception);
        }
      } while (retry);
      if (timerstate)
        _timer.Start();
    }
  }
}
