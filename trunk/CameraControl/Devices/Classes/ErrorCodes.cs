using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CameraControl.Devices.Classes
{
  public class ErrorCodes
  {
    public const uint WIA_ERROR_GENERAL_ERROR = 0x80210001;
    public const uint WIA_ERROR_OFFLINE = 0x80210005;
    public const uint WIA_ERROR_BUSY = 0x80210006;
    public const uint WIA_ERROR_DEVICE_COMMUNICATION = 0x8021000A;
    public const uint WIA_ERROR_INVALID_COMMAND = 0x8021000B;
    public const uint WIA_ERROR_INCORRECT_HARDWARE_SETTING = 0x8021000C;
    public const uint WIA_ERROR_DEVICE_LOCKED = 0x8021000D;
    public const uint WIA_ERROR_EXCEPTION_IN_DRIVER = 0x8021000E;
    public const uint WIA_ERROR_INVALID_DRIVER_RESPONSE = 0x8021000F;
    public const uint WIA_S_NO_DEVICE_AVAILABLE = 0x80210015;
    public const uint WIA_ERROR_UNABLE_TO_FOCUS = 0x80004005;

    public static void GetException(int code)
    {
      if(code!=0)
      {
        throw new DeviceException("Device MTP error code:" + code + " " + code.ToString("X"));
      }
    }

    public static void GetException(COMException exception)
    {
      switch ((uint)exception.ErrorCode)
      {
        case WIA_ERROR_BUSY:
          throw new DeviceException("Device is bussy. Error code :WIA_ERROR_BUSY", exception);
          break;
        case WIA_ERROR_DEVICE_COMMUNICATION:
          throw new DeviceException("Device communication error. Error code :WIA_ERROR_DEVICE_COMMUNICATION", exception);
          break;
        case WIA_ERROR_DEVICE_LOCKED:
          throw new DeviceException("Device is locked. Error code :WIA_ERROR_DEVICE_LOCKED", exception);
          break;
        case WIA_ERROR_EXCEPTION_IN_DRIVER:
          throw new DeviceException("Exception in driver. Error code :WIA_ERROR_EXCEPTION_IN_DRIVER", exception);
          break;
        case WIA_ERROR_GENERAL_ERROR:
          throw new DeviceException("General error. Error code :WIA_ERROR_GENERAL_ERROR", exception);
          break;
        case WIA_ERROR_INCORRECT_HARDWARE_SETTING:
          throw new DeviceException("Incorrect hardware error. Error code :WIA_ERROR_INCORRECT_HARDWARE_SETTING", exception);
          break;
        case WIA_ERROR_INVALID_COMMAND:
          throw new DeviceException("Invalid command. Error code :WIA_ERROR_INVALID_COMMAND", exception);
          break;
        case WIA_ERROR_INVALID_DRIVER_RESPONSE:
          throw new DeviceException("Invalid driver response. Error code :WIA_ERROR_INVALID_DRIVER_RESPONSE", exception);
          break;
        case WIA_ERROR_OFFLINE:
          throw new DeviceException("Device is offline. Error code :WIA_ERROR_OFFLINE", exception, WIA_ERROR_OFFLINE);
          break;
        case WIA_ERROR_UNABLE_TO_FOCUS:
          throw new DeviceException("Unable to focus. Error code :WIA_ERROR_UNABLE_TO_FOCUS", exception, WIA_ERROR_UNABLE_TO_FOCUS);
          break;
        default:
          throw new DeviceException("Unknow error. Error code:" + (uint)exception.ErrorCode, exception);
          break;
      }

    }
  }
}
