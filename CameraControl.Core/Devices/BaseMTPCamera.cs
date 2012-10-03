using System.Threading;
using CameraControl.Core.Devices.Classes;
using PortableDeviceLib;

namespace CameraControl.Core.Devices
{
  public class BaseMTPCamera : BaseCameraDevice
  {
    private const int CONST_READY_TIME = 1;
    private const int CONST_LOOP_TIME = 1000;

    protected StillImageDevice StillImageDevice = null;
    protected bool DeviceIsBusy = false;


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
        if ((res.ErrorCode == ErrorCodes.MTP_Device_Busy || res.ErrorCode == PortableDeviceErrorCodes.ERROR_BUSY) &&
            counter < loop)
        {
          Thread.Sleep(CONST_READY_TIME);
          counter++;
          allok = false;
        }
        res = StillImageDevice.ExecuteReadDataEx(code, param1, param2);
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

  }
}
