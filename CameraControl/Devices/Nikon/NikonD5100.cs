using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using CameraControl.Classes;
using CameraControl.Devices.Classes;
using CameraControl.Devices.Others;
using PortableDeviceLib;
using Timer = System.Timers.Timer;

namespace CameraControl.Devices.Nikon
{
  public class NikonD5100 : WiaCameraDevice, ICameraDevice
  {
    public const int CONST_CMD_AfDrive = 0x90C1;
    public const int CONST_CMD_StartLiveView = 0x9201;
    public const int CONST_CMD_EndLiveView = 0x9202;
    public const int CONST_CMD_GetLiveViewImage = 0x9203;
    public const int CONST_CMD_InitiateCaptureRecInMedia = 0x9207;
    public const int CONST_CMD_ChangeAfArea = 0x9205;
    public const int CONST_CMD_MfDrive = 0x9204;
    public const int CONST_CMD_GetDevicePropValue = 0x1015;
    public const int CONST_CMD_SetDevicePropValue = 0x1016;
    public const int CONST_CMD_GetEvent = 0x90C7;

    public const int CONST_PROP_Fnumber = 0x5007;
    public const int CONST_PROP_ExposureIndex = 0x500F;
    public const int CONST_PROP_ExposureTime = 0x500D;
    public const int CONST_PROP_WhiteBalance = 0x5005;
    public const int CONST_PROP_ExposureProgramMode = 0x500E;
    public const int CONST_PROP_ExposureBiasCompensation = 0x5010;
    public const int CONST_PROP_BatteryLevel = 0x5001;
    public const int CONST_PROP_LiveViewImageZoomRatio = 0xD1A3;
    public const int CONST_PROP_AFModeSelect = 0xD161;
    public const int CONST_PROP_CompressionSetting = 0x5004;
    public const int CONST_PROP_ExposureMeteringMode = 0x500B;
    public const int CONST_PROP_FocusMode = 0x500A;


    private const string AppName = "CameraControl";
    private const int AppMajorVersionNumber = 1;
    private const int AppMinorVersionNumber = 0;
    private Timer _timer = new Timer(1000/15);

    protected StillImageDevice _stillImageDevice = null;
    private WIAManager _manager = null;


    public NikonD5100()
    {
      _timer.AutoReset = true;
      _timer.Elapsed += _timer_Elapsed;
    }

    void _timer_Elapsed(object sender, ElapsedEventArgs e)
    {
      _timer.Stop();
      try
      {
        lock (Locker)
        {
          getEvent();
        }
      }
      catch (Exception)
      {
        
        
      }
      _timer.Start();
    }


    public override bool Init(string id, WIAManager manager)
    {
      base.Init(id, manager);
      ExposureCompensation.ValueChanged += ExposureCompensation_ValueChanged;
      FocusMode.ValueChanged += FocusMode_ValueChanged;
      _manager = manager;
      HaveLiveView = true;
      _stillImageDevice = new StillImageDevice(id);
      _stillImageDevice.ConnectToDevice(AppName, AppMajorVersionNumber, AppMinorVersionNumber);
      _timer.Start();
      return true;
    }

    void FocusMode_ValueChanged(object sender, string key, int val)
    {
      lock (Locker)
      {
        _stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, BitConverter.GetBytes((UInt16)val),
                                           CONST_PROP_FocusMode, -1);
      }
    }

    void ExposureCompensation_ValueChanged(object sender, string key, int val)
    {
      lock (Locker)
      {
        _stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, BitConverter.GetBytes(val),
                                           CONST_PROP_ExposureBiasCompensation, -1);
      }
    }


    public override void StartLiveView()
    {
      lock (Locker)
      {
        LiveViewImageZoomRatio = 0;
        _stillImageDevice.ExecuteWithNoData(CONST_CMD_StartLiveView);
      }
    }

    public override void StopLiveView()
    {
      lock (Locker)
      {
        _stillImageDevice.ExecuteWithNoData(CONST_CMD_EndLiveView);
      }
    }

    public override LiveViewData GetLiveViewImage()
    {
      LiveViewData viewData = new LiveViewData();
      if (Monitor.TryEnter(Locker,100))
      {
        try
        {
          viewData.HaveFocusData = true;

          const int headerSize = 384;

          byte[] result = _stillImageDevice.ExecuteReadData(CONST_CMD_GetLiveViewImage);
          if (result == null || result.Length <= headerSize)
            return null;
          int cbBytesRead = result.Length;
          GetAditionalLIveViewData(viewData, result);
          MemoryStream copy = new MemoryStream((int)cbBytesRead - headerSize);
          copy.Write(result, headerSize, (int)cbBytesRead - headerSize);
          copy.Close();
          viewData.ImageData = copy.GetBuffer();
        }
        finally
        {
          Monitor.Exit(Locker);
        }
      }
      return viewData;
    }

    public override void TakePicture()
    {
      Monitor.Enter(Locker);
      try
      {
        _manager.Device.ExecuteCommand(Conts.wiaCommandTakePicture);
      }
      finally
      {
        Monitor.Exit(Locker); 
      }
    }

    protected virtual void GetAditionalLIveViewData(LiveViewData viewData, byte[] result)
    {
      viewData.LiveViewImageWidth = ToInt16(result, 0);
      viewData.LiveViewImageHeight = ToInt16(result, 2);

      viewData.ImageWidth = ToInt16(result, 4);
      viewData.ImageHeight = ToInt16(result, 6);

      viewData.FocusFrameXSize = ToInt16(result, 16);
      viewData.FocusFrameYSize = ToInt16(result, 18);

      viewData.FocusX = ToInt16(result, 20);
      viewData.FocusY = ToInt16(result, 22);

      viewData.Focused = result[40] != 1;
    }

    public override void Focus(int step)
    {
      lock (Locker)
      {
        if (step > 0)
          _stillImageDevice.ExecuteWithNoData(CONST_CMD_MfDrive, 0x00000001, (uint) step);
        else
          _stillImageDevice.ExecuteWithNoData(CONST_CMD_MfDrive, 0x00000002, (uint) -step);
      }
    }

    public override void AutoFocus()
    {
      lock (Locker)
      {
        _stillImageDevice.ExecuteWithNoData(CONST_CMD_AfDrive);
      }
    }

    public override void TakePictureNoAf()
    {
      lock (Locker)
      {
        byte oldval = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_AFModeSelect, -1)[0];
        _stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, new[] {(byte) 4},
                                           CONST_PROP_AFModeSelect, -1);
        _stillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCaptureRecInMedia, 0xFFFFFFFF, 0x0000);
        _stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, new[] { oldval },
                                   CONST_PROP_AFModeSelect, -1);
      }
    }

    public override void Focus(int x, int y)
    {
      lock (Locker)
      {
        _stillImageDevice.ExecuteWithNoData(CONST_CMD_ChangeAfArea, (uint) x, (uint) y);
      }
    }

    public override void Close()
    {
      lock (Locker)
      {
        _timer.Stop();
        _stillImageDevice.Disconnect();
        HaveLiveView = false;
        ServiceProvider.Settings.SystemMessage = "Camera disconnected !";
      }
    }

    public static short ToInt16(byte[] value, int startIndex)
    {
      int i = (short) (value[startIndex] << 8 | value[startIndex+1]);
      return (short) (i);
      //return System.BitConverter.ToInt16(value.Reverse().ToArray(), value.Length - sizeof(Int16) - startIndex);
    }

    private byte _liveViewImageZoomRatio;

    public override byte LiveViewImageZoomRatio
    {
      get
      {
        if (_stillImageDevice == null)
          return 0;
        lock (Locker)
        {
          byte[] data = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue,
                                                          CONST_PROP_LiveViewImageZoomRatio,
                                                          -1);
          if (data != null && data.Length == 1)
          {
            _liveViewImageZoomRatio = data[0];
            ////NotifyPropertyChanged("LiveViewImageZoomRatio");
          }
        }
        return _liveViewImageZoomRatio;
      }
      set
      {
        lock (Locker)
        {
          if (_stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, new[] {value},
                                                 CONST_PROP_LiveViewImageZoomRatio, -1) != null)
            _liveViewImageZoomRatio = value;
          NotifyPropertyChanged("LiveViewImageZoomRatio");
        }
      }
    }

    public override void ReadDeviceProperties()
    {
      lock (Locker)
      {
        try
        {
          HaveLiveView = true;
          FNumber.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_Fnumber));
          IsoNumber.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_ExposureIndex));
          ShutterSpeed.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_ExposureTime));
          WhiteBalance.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_WhiteBalance));
          Mode.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_ExposureProgramMode));
          ExposureCompensation.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue,
                                                                          CONST_PROP_ExposureBiasCompensation));
          CompressionSetting.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue,
                                                                CONST_PROP_CompressionSetting));
          ExposureMeteringMode.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue,
                                                                CONST_PROP_ExposureMeteringMode));
          FocusMode.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_FocusMode));
          Battery = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_BatteryLevel, -1)[0];
        }
        catch (Exception)
        {
        }
      }
    }

    private void getEvent()
    {
      byte[] result = _stillImageDevice.ExecuteReadData(CONST_CMD_GetEvent);
      if (result == null)
        return;
      bool shouldRefresProperties = false;
      int eventCount = BitConverter.ToInt16(result, 0);
      if (eventCount > 0)
      {
        for (int i = 0; i < eventCount; i++)
        {
          int eventCode = BitConverter.ToInt16(result, 6*i + 2);
          int eventParam = BitConverter.ToInt16(result, 6*i + 4);
          if (eventCode == 0x4006)
          {
            shouldRefresProperties = true;
          }
        }
        if (shouldRefresProperties)
          ReadDeviceProperties();
      }
    }

  }
}
