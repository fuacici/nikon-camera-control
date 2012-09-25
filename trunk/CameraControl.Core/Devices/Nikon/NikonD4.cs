using System;
using CameraControl.Core.Devices.Classes;

namespace CameraControl.Core.Devices.Nikon
{
  public class NikonD4 : NikonBase
  {
    public const int CONST_CMD_TerminateCapture = 0x920C;

    public override bool Init(DeviceDescriptor deviceDescriptor)
    {
      bool res = base.Init(deviceDescriptor);
      Capabilities.Clear();
      Capabilities.Add(CapabilityEnum.LiveView);
      Capabilities.Add(CapabilityEnum.Bulb);
      Capabilities.Add(CapabilityEnum.RecordMovie);
      Capabilities.Add(CapabilityEnum.CaptureInRam);
      Capabilities.Add(CapabilityEnum.CaptureNoAf);
      return res;
    }

    public override void StartBulbMode()
    {
      DeviceReady();
      StillImageDevice.ExecuteWithNoData(CONST_CMD_ChangeCameraMode, 1);
      SetProperty(CONST_CMD_SetDevicePropValue, BitConverter.GetBytes((UInt16) 0x0001),
                  CONST_PROP_ExposureProgramMode, -1);
      SetProperty(CONST_CMD_SetDevicePropValue, BitConverter.GetBytes((UInt32) 0xFFFFFFFF),
                  CONST_PROP_ExposureTime, -1);

      ErrorCodes.GetException(CaptureInSdRam
                                ? StillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCaptureRecInMedia, 0xFFFFFFFF,
                                                                      0x0001)
                                : StillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCaptureRecInMedia, 0xFFFFFFFF,
                                                                      0x0000));
    }

    public override void EndBulbMode()
    {
      lock (Locker)
      {
        DeviceReady();
        ErrorCodes.GetException(StillImageDevice.ExecuteWithNoData(CONST_CMD_TerminateCapture, 0, 0));
        DeviceReady();
        StillImageDevice.ExecuteWithNoData(CONST_CMD_ChangeCameraMode, 0);
      }
    }

    public override void LockCamera()
    {
      StillImageDevice.ExecuteWithNoData(CONST_CMD_ChangeCameraMode, 1);
    }

    public override void UnLockCamera()
    {
      StillImageDevice.ExecuteWithNoData(CONST_CMD_ChangeCameraMode, 0);
    }
  }
}
