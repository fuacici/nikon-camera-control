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
      return res;
    }

    public override void EndCapture()
    {
      lock (Locker)
      {
        DeviceReady();
        ErrorCodes.GetException(_stillImageDevice.ExecuteWithNoData(CONST_CMD_TerminateCapture));
        DeviceReady();
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
