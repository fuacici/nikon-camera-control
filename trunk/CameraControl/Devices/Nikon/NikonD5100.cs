using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
using CameraControl.Classes;
using CameraControl.Devices.Classes;
using CameraControl.Devices.Others;
using PortableDeviceLib;
using WIA;
using Timer = System.Timers.Timer;

namespace CameraControl.Devices.Nikon
{
  public class NikonD5100 :BaseFieldClass,  ICameraDevice
  {
    public const int CONST_CMD_AfDrive = 0x90C1;
    public const int CONST_CMD_StartLiveView = 0x9201;
    public const int CONST_CMD_EndLiveView = 0x9202;
    public const int CONST_CMD_GetLiveViewImage = 0x9203;
    public const int CONST_CMD_InitiateCapture = 0x100E;
    public const int CONST_CMD_InitiateCaptureRecInMedia = 0x9207;
    public const int CONST_CMD_AfAndCaptureRecInSdram = 0x90CB;
    public const int CONST_CMD_ChangeAfArea = 0x9205;
    public const int CONST_CMD_MfDrive = 0x9204;
    public const int CONST_CMD_GetDevicePropValue = 0x1015;
    public const int CONST_CMD_SetDevicePropValue = 0x1016;
    public const int CONST_CMD_GetEvent = 0x90C7;
    public const int CONST_CMD_GetDevicePropDesc = 0x1014;
    public const int CONST_CMD_DeviceReady = 0x90C8;

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
    public const int CONST_PROP_LiveViewStatus = 0xD1A2;



    private const string AppName = "CameraControl";
    private const int AppMajorVersionNumber = 1;
    private const int AppMinorVersionNumber = 0;
    private Timer _timer = new Timer(1000/15);

    protected StillImageDevice _stillImageDevice = null;
    
    //TODO: remove reference from wia device
    private Device Device { get; set; }
    public DeviceManager DeviceManager { get; set; }

    private Dictionary<uint, string> _isoTable = new Dictionary<uint, string>()
                                                  {
                                                    {0x0064, "100"},
                                                    {0x007D, "125"},
                                                    {0x00A0, "160"},
                                                    {0x00C8, "200"},
                                                    {0x00FA, "250"},
                                                    {0x0140, "320"},
                                                    {0x0190, "400"},
                                                    {0x01F4, "500"},
                                                    {0x0280, "640"},
                                                    {0x0320, "800"},
                                                    {0x03E8, "1000"},
                                                    {0x04E2, "1250"},
                                                    {0x0640, "1600"},
                                                    {0x07D0, "2000"},
                                                    {0x09C4, "2500"},
                                                    {0x0C80, "3200"},
                                                    {0x0FA0, "4000"},
                                                    {0x1388, "5000"},
                                                    {0x1900, "6400"},
                                                    {0x1F40, "Hi 0.3"},
                                                    {0x2710, "Hi 0.7"},
                                                    {0x3200, "Hi 1"},
                                                    {0x6400, "Hi 2"},
                                                  };
    Dictionary<uint, string> _shutterTable = new Dictionary<uint, string>
                       {
                         {1, "1/6400"},
                         {2, "1/4000"},
                         {3, "1/3200"},
                         {4, "1/2500"},
                         {5, "1/2000"},
                         {6, "1/1600"},
                         {8, "1/1250"},
                         {10, "1/1000"},
                         {12, "1/800"},
                         {13, "1/750"},
                         {15, "1/640"},
                         {20, "1/500"},
                         {25, "1/400"},
                         {28, "1/350"},
                         {31, "1/320"},
                         {40, "1/250"},
                         {50, "1/200"},
                         {55, "1/180"},
                         {62, "1/160"},
                         {80, "1/125"},
                         {100, "1/100"},
                         {111, "1/90"},
                         {125, "1/80"},
                         {166, "1/60"},
                         {200, "1/50"},
                         {222, "1/45"},
                         {250, "1/40"},
                         {333, "1/30"},
                         {400, "1/25"},
                         {500, "1/20"},
                         {666, "1/15"},
                         {769, "1/13"},
                         {1000, "1/10"},
                         {1250, "1/8"},
                         {1666, "1/6"},
                         {2000, "1/5"},
                         {2500, "1/4"},
                         {3333, "1/3"},
                         {4000, "1/2.5"},
                         {5000, "1/2"},
                         {6250, "1/1.6"},
                         {6666, "1/1.5"},
                         {7692, "1/1.3"},
                         {10000, "1s"},
                         {13000, "1.3s"},
                         {15000, "1.5s"},
                         {16000, "1.6s"},
                         {20000, "2s"},
                         {25000, "2.5s"},
                         {30000, "3s"},
                         {40000, "4s"},
                         {50000, "5s"},
                         {60000, "6s"},
                         {80000, "8s"},
                         {100000, "10s"},
                         {130000, "13s"},
                         {150000, "15s"},
                         {200000, "20s"},
                         {250000, "25s"},
                         {300000, "30s"},
                         { 0xFFFFFFFF , "Bulb"},
                       };
    
    Dictionary<int, string> _exposureModeTable = new Dictionary<int, string>()
                            {
                              {1, "M"},
                              {2, "P"},
                              {3, "A"},
                              {4, "S"},
                              {0x8010, "[Scene mode] AUTO"},
                              {0x8011, "[Scene mode] Portrait"},
                              {0x8012, "[Scene mode] Landscape"},
                              {0x8013, "[Scene mode] Close up"},
                              {0x8014, "[Scene mode] Sports"},
                              {0x8016, "[Scene mode] Flash prohibition AUTO"},
                              {0x8017, "[Scene mode] Child"},
                              {0x8018, "[Scene mode] SCENE"},
                              {0x8019, "[EffectMode] EFFECTS"},
                            };

    Dictionary<uint, string> _wbTable = new Dictionary<uint, string>()
                  {
                    {2, "Auto"},
                    {4, "Daylight"},
                    {5, "Fluorescent "},
                    {6, "Incandescent"},
                    {7, "Flash"},
                    {32784, "Cloudy"},
                    {32785, "Shade"},
                    {32786, "Kelvin"},
                    {32787, "Custom"}
                  };
    
    protected Dictionary<int, string> _csTable = new Dictionary<int, string>()
                                                {
                                                  {0, "JPEG (BASIC)"},
                                                  {1, "JPEG (NORMAL)"},
                                                  {2, "JPEG (FINE)"},
                                                  {3, "TIFF (RGB)"},
                                                  {4, "RAW"},
                                                  {5, "RAW + JPEG (BASIC)"},
                                                  {6, "RAW + JPEG (NORMAL)"},
                                                  {7, "RAW + JPEG (FINE)"}
                                                };
    private Dictionary<int, string> _emmTable = new Dictionary<int, string>
                                              {
                                                {2, "Center-weighted metering"},
                                                {3, "Multi-pattern metering"},
                                                {4, "Spot metering"}
                                              };

    private Dictionary<uint, string> _fmTable = new Dictionary<uint, string>()
                                              {
                                                {1, "[M] Manual focus"},
                                                {0x8010, "[S] Single AF servo"},
                                                {0x8011, "[C] Continuous AF servo"},
                                                {0x8012, "[A] AF servo mode automatic switching"},
                                                {0x8013, "[F] Constant AF servo"},
                                              };
    internal object Locker = new object(); // object used to lock multi hreaded mothods 

    private bool _haveLiveView;
    public bool HaveLiveView
    {
      get { return _haveLiveView; }
      set
      {
        _haveLiveView = value;
        NotifyPropertyChanged("HaveLiveView");
      }
    }

    private PropertyValue<int> _isoNumber;
    public PropertyValue<int> IsoNumber
    {
      get { return _isoNumber; }
      set
      {
        _isoNumber = value;
        NotifyPropertyChanged("IsoNumber");
      }
    }

    private PropertyValue<long> _shutterSpeed;
    public PropertyValue<long> ShutterSpeed
    {
      get { return _shutterSpeed; }
      set
      {
        _shutterSpeed = value;
        NotifyPropertyChanged("ShutterSpeed");
      }
    }

    private PropertyValue<uint> _mode;
    public PropertyValue<uint> Mode
    {
      get { return _mode; }
      set
      {
        _mode = value;
        NotifyPropertyChanged("Mode");
      }
    }

    private PropertyValue<int> _fNumber;
    public PropertyValue<int> FNumber
    {
      get { return _fNumber; }
      set
      {
        _fNumber = value;
        NotifyPropertyChanged("FNumber");
      }
    }

    private PropertyValue<long> _whiteBalance;
    public PropertyValue<long> WhiteBalance
    {
      get { return _whiteBalance; }
      set
      {
        _whiteBalance = value;
        NotifyPropertyChanged("WhiteBalance");
      }
    }

    private PropertyValue<int> _exposureCompensation;
    public PropertyValue<int> ExposureCompensation
    {
      get { return _exposureCompensation; }
      set
      {
        _exposureCompensation = value;
        NotifyPropertyChanged("ExposureCompensation");
      }
    }

    private PropertyValue<int> _compressionSetting;
    public PropertyValue<int> CompressionSetting
    {
      get { return _compressionSetting; }
      set
      {
        _compressionSetting = value;
        NotifyPropertyChanged("CompressionSetting");
      }
    }

    private PropertyValue<int> _exposureMeteringMode;
    public PropertyValue<int> ExposureMeteringMode
    {
      get { return _exposureMeteringMode; }
      set
      {
        _exposureMeteringMode = value;
        NotifyPropertyChanged("ExposureMeteringMode");
      }
    }

    private PropertyValue<uint> _focusMode;
    public PropertyValue<uint> FocusMode
    {
      get { return _focusMode; }
      set
      {
        _focusMode = value;
        NotifyPropertyChanged("FocusMode");
      }
    }

    private string _deviceName;

    public string DeviceName
    {
      get { return _deviceName; }
      set
      {
        _deviceName = value;
        NotifyPropertyChanged("DeviceName");
      }
    }

    private string _manufacturer;

    public string Manufacturer
    {
      get { return _manufacturer; }
      set
      {
        _manufacturer = value;
        NotifyPropertyChanged("Manufacturer");
      }
    }

    private string _serialNumber;
    public string SerialNumber
    {
      get { return _serialNumber; }
      set
      {
        _serialNumber = value;
        NotifyPropertyChanged("SerialNumber");
      }
    }


    private bool _isConnected;

    public bool IsConnected
    {
      get { return _isConnected; }
      set
      {
        _isConnected = value;
        NotifyPropertyChanged("IsConnected");
      }
    }

    private int _battery;
    public int Battery
    {
      get { return _battery; }
      set
      {
        _battery = value;
        NotifyPropertyChanged("Battery");
      }
    }

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


    public virtual bool Init(DeviceDescriptor deviceDescriptor)
    {
      HaveLiveView = true;
      _stillImageDevice = new StillImageDevice(deviceDescriptor.WpdId);
      _stillImageDevice.ConnectToDevice(AppName, AppMajorVersionNumber, AppMinorVersionNumber);
      _stillImageDevice.DeviceEvent += _stillImageDevice_DeviceEvent;
      DeviceName = _stillImageDevice.Model;
      Manufacturer = _stillImageDevice.Manufacturer;
      InitIso();
      InitShutterSpeed();
      InitFNumber();
      InitMode();
      InitWhiteBalance();
      InitExposureCompensation();
      InitCompressionSetting();
      InitExposureMeteringMode();
      InitFocusMode();
      ReadDeviceProperties(CONST_PROP_BatteryLevel);
      _timer.Start();
      IsConnected = true;
      return true;
    }

    void _stillImageDevice_DeviceEvent(object sender, PortableDeviceEventArgs e)
    {
      if (e.EventType.EventGuid == PortableDeviceGuids.WPD_EVENT_DEVICE_REMOVED)
      {
        _timer.Stop();
        _stillImageDevice.Disconnect();
        _stillImageDevice.IsConnected = false;
        IsConnected = false;
      }
      if (PhotoCaptured != null && e.EventType.EventGuid == PortableDeviceGuids.WPD_EVENT_OBJECT_ADDED)
      {
        PhotoCapturedEventArgs args = new PhotoCapturedEventArgs
                                        {
                                          WiaImageItem = null,
                                          EventArgs = e,
                                          CameraDevice = this,
                                          FileName = e.EventType.DeviceObject.Name
                                        };
        PhotoCaptured(this, args);
      }
    }


    void IsoNumber_ValueChanged(object sender, string key, int val)
    {
      _stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, BitConverter.GetBytes(val),
                                         CONST_PROP_ExposureIndex, -1);
    }

    void ExposureCompensation_ValueChanged(object sender, string key, int val)
    {
      lock (Locker)
      {
        _stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, BitConverter.GetBytes(val),
                                           CONST_PROP_ExposureBiasCompensation, -1);
      }
    }

    private void InitIso()
    {
      lock (Locker)
      {
        IsoNumber = new PropertyValue<int>();
        IsoNumber.ValueChanged += IsoNumber_ValueChanged;
        IsoNumber.Clear();
        byte[] result = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropDesc,CONST_PROP_ExposureIndex);
        int type = BitConverter.ToInt16(result, 2);
        byte formFlag = result[9];
        UInt16 defval = BitConverter.ToUInt16(result, 7);
        for (int i = 0; i < result.Length - 12; i += 2)
        {
          UInt16 val = BitConverter.ToUInt16(result, 12 + i);
          IsoNumber.AddValues(_isoTable.ContainsKey(val) ? _isoTable[val] : val.ToString(), val);
        }
        IsoNumber.SetValue(defval);
      }
    }

    private void InitShutterSpeed()
    {
      lock (Locker)
      {
        try
        {
          byte datasize = 4;
          ShutterSpeed = new PropertyValue<long>();
          ShutterSpeed.ValueChanged += ShutterSpeed_ValueChanged;
          ShutterSpeed.Clear();
          byte[] result = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropDesc, CONST_PROP_ExposureTime);
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
          
         
        }
      }
    }

    void ShutterSpeed_ValueChanged(object sender, string key, long val)
    {
      _stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, BitConverter.GetBytes(val),
                                         CONST_PROP_ExposureTime, -1);
    }

    private void InitMode()
    {
      try
      {
        byte datasize = 2;
        Mode = new PropertyValue<uint>();
        Mode.IsEnabled = false;
        Mode.ValueChanged += Mode_ValueChanged;
        byte[] result = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropDesc, CONST_PROP_ExposureProgramMode);
        int type = BitConverter.ToInt16(result, 2);
        byte formFlag = result[(2 * datasize) + 5];
        UInt16 defval = BitConverter.ToUInt16(result, datasize + 5);
        for (int i = 0; i < result.Length - ((2 * datasize) + 6 + 2); i += datasize)
        {
          UInt16 val = BitConverter.ToUInt16(result, ((2 * datasize) + 6 + 2) + i);
          Mode.AddValues(_exposureModeTable.ContainsKey(val) ? _exposureModeTable[val] : val.ToString(), val);
        }
        Mode.SetValue(defval);
      }
      catch (Exception ex)
      {
      }
    }



    void Mode_ValueChanged(object sender, string key, uint val)
    {
      switch (key)
      {
        case "M":
          ShutterSpeed.IsEnabled = true;
          FNumber.IsEnabled = true;
          break;
        case "P":
          ShutterSpeed.IsEnabled = false;
          FNumber.IsEnabled = false;
          break;
        case "A":
          ShutterSpeed.IsEnabled = false;
          FNumber.IsEnabled = true;
          break;
        case "S":
          ShutterSpeed.IsEnabled = true;
          FNumber.IsEnabled = false;
          break;
        default:
          ShutterSpeed.IsEnabled = false;
          FNumber.IsEnabled = false;
          break;
      } 
    }

    private void InitFNumber()
    {
      FNumber = new PropertyValue<int> { IsEnabled = true };
      FNumber.ValueChanged += FNumber_ValueChanged;
      ReInitFNumber();
    }

    private void ReInitFNumber()
    {
      try
      {
        FNumber.Clear();
        const byte datasize = 2;
        byte[] result = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropDesc, CONST_PROP_Fnumber);
        int type = BitConverter.ToInt16(result, 2);
        byte formFlag = result[(2 * datasize) + 5];
        UInt16 defval = BitConverter.ToUInt16(result, datasize + 5);
        for (int i = 0; i < result.Length - ((2 * datasize) + 6 + 2); i += datasize)
        {
          UInt16 val = BitConverter.ToUInt16(result, ((2 * datasize) + 6 + 2) + i);
          double d = val;
          string s = "f/" + (d / 100).ToString("0.0");
          FNumber.AddValues(s, val);
        }
        FNumber.SetValue(defval);
      }
      catch (Exception ex)
      {
      }
    }

    void FNumber_ValueChanged(object sender, string key, int val)
    {
      _stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, BitConverter.GetBytes(val),
                                   CONST_PROP_Fnumber, -1);
    }

    private void InitWhiteBalance()
    {
      try
      {
        byte datasize = 2;
        WhiteBalance = new PropertyValue<long>();
        WhiteBalance.ValueChanged += WhiteBalance_ValueChanged;
        byte[] result = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropDesc, CONST_PROP_WhiteBalance);
        int type = BitConverter.ToInt16(result, 2);
        byte formFlag = result[(2 * datasize) + 5];
        UInt16 defval = BitConverter.ToUInt16(result, datasize + 5);
        for (int i = 0; i < result.Length - ((2 * datasize) + 6 + 2); i += datasize)
        {
          UInt16 val = BitConverter.ToUInt16(result, ((2 * datasize) + 6 + 2) + i);
          WhiteBalance.AddValues(_wbTable.ContainsKey(val) ? _wbTable[val] : val.ToString(), val);
        }
        WhiteBalance.SetValue(defval);
      }
      catch (Exception ex)
      {
      }
    }

    void WhiteBalance_ValueChanged(object sender, string key, long val)
    {
      _stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, BitConverter.GetBytes((UInt16)val),
                              CONST_PROP_WhiteBalance, -1); 
    }

    public void InitExposureCompensation()
    {
      try
      {
        byte datasize = 2;
        ExposureCompensation = new PropertyValue<int>();
        ExposureCompensation.ValueChanged += ExposureCompensation_ValueChanged;
        byte[] result = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropDesc, CONST_PROP_ExposureBiasCompensation);
        int type = BitConverter.ToInt16(result, 2);
        byte formFlag = result[(2*datasize) + 5];
        Int16 defval = BitConverter.ToInt16(result, datasize + 5);
        for (int i = 0; i < result.Length - ((2*datasize) + 6 + 2); i += datasize)
        {
          Int16 val = BitConverter.ToInt16(result, ((2*datasize) + 6 + 2) + i);
          decimal d = val;
          string s = decimal.Round(d/1000, 1).ToString();
          if (d > 0)
            s = "+" + s;
          ExposureCompensation.AddValues(s, val);
        }
        ExposureCompensation.SetValue(defval);
      }
      catch (Exception ex)
      {
      }
    }

    protected virtual void InitCompressionSetting()
    {
      try
      {
        byte datasize = 1;
        CompressionSetting = new PropertyValue<int>();
        CompressionSetting.ValueChanged += CompressionSetting_ValueChanged;
        byte[] result = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropDesc, CONST_PROP_CompressionSetting);
        int type = BitConverter.ToInt16(result, 2);
        byte formFlag = result[(2 * datasize) + 5];
        byte defval = result[ datasize + 5];
        for (int i = 0; i < result.Length - ((2 * datasize) + 6 + 2); i += datasize)
        {
          byte val = result[((2*datasize) + 6 + 2) + i];
          CompressionSetting.AddValues(_csTable.ContainsKey(val) ? _csTable[val] : val.ToString(), val);
        }
        CompressionSetting.SetValue(defval);
      }
      catch (Exception ex)
      {
      }
    }

    protected void CompressionSetting_ValueChanged(object sender, string key, int val)
    {
      _stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, BitConverter.GetBytes((byte)val),
                        CONST_PROP_CompressionSetting, -1); 
    }

    public void InitExposureMeteringMode()
    {
      try
      {
        byte datasize = 2;
        ExposureMeteringMode = new PropertyValue<int>();
        ExposureMeteringMode.ValueChanged += ExposureMeteringMode_ValueChanged;
        byte[] result = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropDesc, CONST_PROP_ExposureMeteringMode);
        int type = BitConverter.ToInt16(result, 2);
        byte formFlag = result[(2 * datasize) + 5];
        UInt16 defval = BitConverter.ToUInt16(result, datasize + 5);
        for (int i = 0; i < result.Length - ((2 * datasize) + 6 + 2); i += datasize)
        {
          UInt16 val = BitConverter.ToUInt16(result, ((2 * datasize) + 6 + 2) + i);
          ExposureMeteringMode.AddValues(_emmTable.ContainsKey(val) ? _emmTable[val] : val.ToString(), val);
        }
        ExposureMeteringMode.SetValue(defval);
      }
      catch (Exception ex)
      {
      }
    }

    void ExposureMeteringMode_ValueChanged(object sender, string key, int val)
    {
      _stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, BitConverter.GetBytes((UInt16)val),
                        CONST_PROP_ExposureMeteringMode, -1);      
    }

    private void InitFocusMode()
    {
      try
      {
        byte datasize = 2;
        FocusMode = new PropertyValue<uint>();
        FocusMode.IsEnabled = false;
        byte[] result = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropDesc, CONST_PROP_FocusMode);
        int type = BitConverter.ToInt16(result, 2);
        byte formFlag = result[(2 * datasize) + 5];
        UInt16 defval = BitConverter.ToUInt16(result, datasize + 5);
        for (int i = 0; i < result.Length - ((2 * datasize) + 6 + 2); i += datasize)
        {
          UInt16 val = BitConverter.ToUInt16(result, ((2 * datasize) + 6 + 2) + i);
          FocusMode.AddValues(_fmTable.ContainsKey(val) ? _fmTable[val] : val.ToString(), val);
        }
        FocusMode.SetValue(defval);
      }
      catch (Exception ex)
      {
      }
    }
    
    public void StartLiveView()
    {
      lock (Locker)
      {
        LiveViewImageZoomRatio = 0;
        ErrorCodes.GetException(_stillImageDevice.ExecuteWithNoData(CONST_CMD_StartLiveView));
      }
    }

    public void StopLiveView()
    {
      lock (Locker)
      {
        ErrorCodes.GetException(_stillImageDevice.ExecuteWithNoData(CONST_CMD_EndLiveView));
      }
    }

    public virtual LiveViewData GetLiveViewImage()
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

    public  void TakePicture()
    {
      Monitor.Enter(Locker);
      try
      {
        //Device.ExecuteCommand(Conts.wiaCommandTakePicture);
        ErrorCodes.GetException(_stillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCapture));
        DeviceReady();
      }
      catch (COMException comException)
      {
        ErrorCodes.GetException(comException);
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

    public void Focus(int step)
    {
      lock (Locker)
      {
        if (step > 0)
          ErrorCodes.GetException(_stillImageDevice.ExecuteWithNoData(CONST_CMD_MfDrive, 0x00000001, (uint) step));
        else
          ErrorCodes.GetException(_stillImageDevice.ExecuteWithNoData(CONST_CMD_MfDrive, 0x00000002, (uint) -step));
      }
    }

    public  void AutoFocus()
    {
      lock (Locker)
      {
        ErrorCodes.GetException(_stillImageDevice.ExecuteWithNoData(CONST_CMD_AfDrive));
      }
    }

    public virtual void TakePictureNoAf()
    {
      lock (Locker)
      {
        byte oldval = 0;
        byte[] val = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_AFModeSelect, -1);
        if (val != null && val.Length > 0)
          oldval = val[0];
        ErrorCodes.GetException(_stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, new[] { (byte)4 },
                                           CONST_PROP_AFModeSelect, -1));
        ErrorCodes.GetException(_stillImageDevice.ExecuteWithNoData(CONST_CMD_InitiateCaptureRecInMedia, 0xFFFFFFFF, 0x0000));
        if (val != null && val.Length > 0)
          ErrorCodes.GetException(_stillImageDevice.ExecuteWriteData(CONST_CMD_SetDevicePropValue, new[] { oldval },
                                             CONST_PROP_AFModeSelect, -1));
      }
    }

    public void Focus(int x, int y)
    {
      lock (Locker)
      {
        ErrorCodes.GetException(_stillImageDevice.ExecuteWithNoData(CONST_CMD_ChangeAfArea, (uint) x, (uint) y));
      }
    }

    public void Close()
    {
      lock (Locker)
      {
        try
        {
          _timer.Stop();
          _stillImageDevice.Disconnect();
          HaveLiveView = false;
        }
        catch (Exception exception)
        {
          ServiceProvider.Log.Error("Close camera",exception);
        }
      }
    }

    public static short ToInt16(byte[] value, int startIndex)
    {
      int i = (short) (value[startIndex] << 8 | value[startIndex+1]);
      return (short) (i);
      //return System.BitConverter.ToInt16(value.Reverse().ToArray(), value.Length - sizeof(Int16) - startIndex);
    }

    private byte _liveViewImageZoomRatio;

    public byte LiveViewImageZoomRatio
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
                                                 CONST_PROP_LiveViewImageZoomRatio, -1) == 0)
            _liveViewImageZoomRatio = value;
          NotifyPropertyChanged("LiveViewImageZoomRatio");
        }
      }
    }

    public virtual void ReadDeviceProperties(int prop)
    {
      lock (Locker)
      {
        try
        {
          HaveLiveView = true;
          switch (prop)
          {
            case CONST_PROP_Fnumber:
              //FNumber.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_Fnumber));
              ReInitFNumber();
              break;
            case CONST_PROP_ExposureIndex:
              IsoNumber.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue,
                                                                   CONST_PROP_ExposureIndex));
              break;
            case CONST_PROP_ExposureTime:
              ShutterSpeed.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue,
                                                                      CONST_PROP_ExposureTime));
              break;
            case CONST_PROP_WhiteBalance:
              WhiteBalance.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue,
                                                                      CONST_PROP_WhiteBalance));
              break;
            case CONST_PROP_ExposureProgramMode:
              Mode.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue,
                                                              CONST_PROP_ExposureProgramMode));
              break;
            case CONST_PROP_ExposureBiasCompensation:
              ExposureCompensation.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue,
                                                                              CONST_PROP_ExposureBiasCompensation));
              break;
            case CONST_PROP_CompressionSetting:
              CompressionSetting.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue,
                                                                            CONST_PROP_CompressionSetting));
              break;
            case CONST_PROP_ExposureMeteringMode:
              ExposureMeteringMode.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue,
                                                                              CONST_PROP_ExposureMeteringMode));
              break;
            case CONST_PROP_FocusMode:
              FocusMode.SetValue(_stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_FocusMode));
              break;
            case CONST_PROP_BatteryLevel:
              Battery = _stillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropValue, CONST_PROP_BatteryLevel, -1)[0];
              break;
          }
        }
        catch (Exception)
        {
        }
      }
    }

    public void TransferFile(object o, string filename)
    {
      PortableDeviceEventArgs deviceEventArgs = o as PortableDeviceEventArgs;
      if (deviceEventArgs != null)
      {
        _stillImageDevice.SaveFile(deviceEventArgs.EventType.DeviceObject, filename);
      }
    }

    public event PhotoCapturedEventHandler PhotoCaptured;

    private void getEvent()
    {
      byte[] result = _stillImageDevice.ExecuteReadData(CONST_CMD_GetEvent);
      if (result == null)
        return;
      int eventCount = BitConverter.ToInt16(result, 0);
      if (eventCount > 0)
      {
        for (int i = 0; i < eventCount; i++)
        {
          int eventCode = BitConverter.ToInt16(result, 6*i + 2);
          int eventParam = BitConverter.ToInt16(result, 6*i + 4);
          if (eventCode == 0x4006)
          {
            ReadDeviceProperties(eventParam);
          }
        }
      }
    }

    private void DeviceReady()
    {
      int cod =_stillImageDevice.ExecuteWithNoData(CONST_CMD_DeviceReady);
      if(cod!=0)
      {
        
      }
    }

  }
}
