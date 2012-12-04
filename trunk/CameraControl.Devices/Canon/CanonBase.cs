using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using CameraControl.Devices.Classes;
using PortableDeviceLib;

namespace CameraControl.Devices.Canon
{
  public class CanonBase:BaseMTPCamera
  {
    public const int CONST_CMD_InitiateCapture = 0x100E;
    public const int CONST_CMD_EOS_ShutterSpeed = 0xD102;

    protected Dictionary<uint, string> _shutterTable = new Dictionary<uint, string>
                                                         {
                                                           {0, "30"},
                                                           {1, "25"},
                                                           {2, "20"},
                                                           {3, "15"},
                                                           {4, "13"},
                                                           {5, "10"},
                                                           {6, "8"},
                                                           {7, "6"},
                                                           {8, "5"},
                                                           {9, "4"},
                                                           {10, "3.2"},
                                                           {11, "2.5"},
                                                           {12, "2"},
                                                           {13, "1.6"},
                                                           {14, "1.3"},
                                                           {15, "1"},
                                                           {16, "0.8"},
                                                           {17, "0.6"},
                                                           {18, "0.5"},
                                                           {19, "0.4"},
                                                           {20, "0.3"},
                                                           {21, "1/4"},
                                                           {22, "1/5"},
                                                           {23, "1/6"},
                                                           {24, "1/8"},
                                                           {25, "1/10"},
                                                           {26, "1/13"},
                                                           {27, "1/15"},
                                                           {28, "1/20"},
                                                           {29, "1/25"},
                                                           {30, "1/30"},
                                                           {31, "1/40"},
                                                           {32, "1/50"},
                                                           {33, "1/60"},
                                                           {34, "1/80"},
                                                           {35, "1/100"},
                                                           {36, "1/125"},
                                                           {37, "1/160"},
                                                           {38, "1/200"},
                                                           {39, "1/250"},
                                                           {40, "1/320"},
                                                           {41, "1/400"},
                                                           {42, "1/500"},
                                                           {43, "1/640"},
                                                           {44, "1/800"},
                                                           {45, "1/1000"},
                                                           {46, "1/1250"},
                                                           {47, "1/1600"},
                                                           {48, "1/2000"},
                                                           {49, "1/2500"},
                                                           {50, "1/3200"},
                                                           {51, "1/4000"},
                                                           {52, "1/5000"},
                                                           {53, "1/6400"},
                                                           {54, "1/8000"}
                                                         };

    public override bool Init(DeviceDescriptor deviceDescriptor)
    {
      StillImageDevice = new StillImageDevice(deviceDescriptor.WpdId);
      StillImageDevice.ConnectToDevice(AppName, AppMajorVersionNumber, AppMinorVersionNumber);
      StillImageDevice.DeviceEvent += _stillImageDevice_DeviceEvent;
      DeviceName = StillImageDevice.Model;
      Manufacturer = StillImageDevice.Manufacturer;
      InitShutterSpeed();
      return true;
    }

    private void InitShutterSpeed()
    {
      ShutterSpeed = new PropertyValue<long>();
      ShutterSpeed.Name = "ShutterSpeed";
      ShutterSpeed.ValueChanged += ShutterSpeed_ValueChanged;
      ReInitShutterSpeed();
    }

    private void ReInitShutterSpeed()
    {
      lock (Locker)
      {
        try
        {
          byte datasize = 4;
          ShutterSpeed.Clear();
          byte[] result = StillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropDesc, CONST_CMD_EOS_ShutterSpeed);
          int type = BitConverter.ToInt16(result, 2);
          byte formFlag = result[(2 * datasize) + 5];
          UInt32 defval = BitConverter.ToUInt32(result, datasize + 5);
          for (int i = 0; i < result.Length - ((2 * datasize) + 6 + 2); i += datasize)
          {
            UInt32 val = BitConverter.ToUInt32(result, ((2 * datasize) + 6 + 2) + i);
            ShutterSpeed.AddValues(_shutterTable.ContainsKey(val) ? _shutterTable[val] : val.ToString(), val);
          }
          ShutterSpeed.SetValue(defval);
        }
        catch (Exception ex)
        {
          Log.Debug("EOS Shutter speed init", ex);
        }
      }
    }

    void ShutterSpeed_ValueChanged(object sender, string key, long val)
    {
      SetProperty(CONST_CMD_SetDevicePropValue, BitConverter.GetBytes(val),
                                         CONST_CMD_EOS_ShutterSpeed, -1);
    }

    public override void CapturePhoto()
    {
      Monitor.Enter(Locker);
      try
      {
        IsBusy = true;
        //ErrorCodes.GetException(CaptureInSdRam
        //                          ? ExecuteWithNoData(CONST_CMD_AfAndCaptureRecInSdram)
        //                          : ExecuteWithNoData(CONST_CMD_InitiateCapture));
        ErrorCodes.GetException(ExecuteWithNoData(CONST_CMD_InitiateCapture));
      }
      catch (COMException comException)
      {
        IsBusy = false;
        ErrorCodes.GetException(comException);
      }
      catch
      {
        IsBusy = false;
        throw;
      }
      finally
      {
        Monitor.Exit(Locker);
      }
    }

    void _stillImageDevice_DeviceEvent(object sender, PortableDeviceEventArgs e)
    {
      if (e.EventType.EventGuid == PortableDeviceGuids.WPD_EVENT_DEVICE_REMOVED)
      {
        StillImageDevice.Disconnect();
        StillImageDevice.IsConnected = false;
        IsConnected = false;

        OnCameraDisconnected(this, new DisconnectCameraEventArgs { StillImageDevice = StillImageDevice });
      }
    }
  }
}
