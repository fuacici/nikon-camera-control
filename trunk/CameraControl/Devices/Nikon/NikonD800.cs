using System;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices.Nikon
{
  public class NikonD800 : NikonBase
  {
    public const int CONST_CMD_TerminateCapture = 0x920C;

    public override bool Init(DeviceDescriptor deviceDescriptor)
    {
      bool res = base.Init(deviceDescriptor);
      Capabilities.Clear();
      Capabilities.Add(CapabilityEnum.LiveView);
      Capabilities.Add(CapabilityEnum.Bulb);
      Capabilities.Add(CapabilityEnum.RecordMovie);
      return res;
    }

    public override void StartBulbMode()
    {
      DeviceReady();
      _stillImageDevice.ExecuteWithNoData(CONST_CMD_ChangeCameraMode, 1);
      SetProperty(CONST_CMD_SetDevicePropValue, BitConverter.GetBytes((UInt16) 0x0001),
                  CONST_PROP_ExposureProgramMode, -1);
      SetProperty(CONST_CMD_SetDevicePropValue, BitConverter.GetBytes((UInt32)0xFFFFFFFF),
                  CONST_PROP_ExposureTime, -1);

      ErrorCodes.GetException(CaptureInSdRam
                                ? _stillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCaptureRecInMedia, 0xFFFFFFFF,
                                                                      0x0001)
                                : _stillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCaptureRecInMedia, 0xFFFFFFFF,
                                                                      0x0000));
    }

    public override void EndBulbMode()
    {
      lock (Locker)
      {
        DeviceReady();
        ErrorCodes.GetException(_stillImageDevice.ExecuteWithNoData(CONST_CMD_TerminateCapture, 0, 0));
        DeviceReady();
        _stillImageDevice.ExecuteWithNoData(CONST_CMD_ChangeCameraMode, 0);
      }
    }

    public override void LockCamera()
    {
      _stillImageDevice.ExecuteWithNoData(CONST_CMD_ChangeCameraMode, 1);  
    }

    public override void UnLockCamera()
    {
      _stillImageDevice.ExecuteWithNoData(CONST_CMD_ChangeCameraMode, 0);
    }

  }
}
