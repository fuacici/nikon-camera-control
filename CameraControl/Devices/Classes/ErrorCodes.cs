﻿using System;
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
    
    public const uint MTP_OK = 0x2001;
    public const uint MTP_Device_Busy = 0x2019;
    public const uint MTP_Set_Property_Not_Support = 0xA005;
    public const uint MTP_Store_Not_Available = 0x2013;
    public const uint MTP_Out_of_Focus = 0xA002;
    public const uint MTP_Shutter_Speed_Bulb = 0xA008;
    public const uint MTP_Store_Full = 0x200C;
    public const uint MTP_Store_Read_Only = 0x200E;

    public static void GetException(int code)
    {
      GetException((uint) code);
    }

    public static void GetException(uint code)
    {
      if (code != 0 && code != MTP_OK)
      {
        switch (code)
        {
          case MTP_Device_Busy:
            throw new DeviceException("Device MTP error: Device is busy");
          case MTP_Store_Not_Available:
            throw new DeviceException("The card cannot be accessed");
          case MTP_Shutter_Speed_Bulb:
            throw new DeviceException("Bulb mode isn't supported");
          case MTP_Out_of_Focus:
            throw new DeviceException("Unable to focus.");
          case MTP_Store_Full:
            throw new DeviceException("Storage is full.");
          case MTP_Store_Read_Only:
            throw new DeviceException("Storage is read only.");
          default:
            throw new DeviceException("Device MTP error code: " + code.ToString("X"));
        }
      }
    }

    public static void GetException(COMException exception)
    {
      switch ((uint)exception.ErrorCode)
      {
        case WIA_ERROR_BUSY:
          throw new DeviceException("Device is bussy. Error code :WIA_ERROR_BUSY", exception);
        case WIA_ERROR_DEVICE_COMMUNICATION:
          throw new DeviceException("Device communication error. Error code :WIA_ERROR_DEVICE_COMMUNICATION", exception);
        case WIA_ERROR_DEVICE_LOCKED:
          throw new DeviceException("Device is locked. Error code :WIA_ERROR_DEVICE_LOCKED", exception);
        case WIA_ERROR_EXCEPTION_IN_DRIVER:
          throw new DeviceException("Exception in driver. Error code :WIA_ERROR_EXCEPTION_IN_DRIVER", exception);
        case WIA_ERROR_GENERAL_ERROR:
          throw new DeviceException("General error. Error code :WIA_ERROR_GENERAL_ERROR", exception);
        case WIA_ERROR_INCORRECT_HARDWARE_SETTING:
          throw new DeviceException("Incorrect hardware error. Error code :WIA_ERROR_INCORRECT_HARDWARE_SETTING", exception);
        case WIA_ERROR_INVALID_COMMAND:
          throw new DeviceException("Invalid command. Error code :WIA_ERROR_INVALID_COMMAND", exception);
        case WIA_ERROR_INVALID_DRIVER_RESPONSE:
          throw new DeviceException("Invalid driver response. Error code :WIA_ERROR_INVALID_DRIVER_RESPONSE", exception);
        case WIA_ERROR_OFFLINE:
          throw new DeviceException("Device is offline. Error code :WIA_ERROR_OFFLINE", exception, WIA_ERROR_OFFLINE);
        case WIA_ERROR_UNABLE_TO_FOCUS:
          throw new DeviceException("Unable to focus. Error code :WIA_ERROR_UNABLE_TO_FOCUS", exception, WIA_ERROR_UNABLE_TO_FOCUS);
        default:
          throw new DeviceException("Unknow error. Error code:" + (uint)exception.ErrorCode, exception);
      }

    }
  }
}