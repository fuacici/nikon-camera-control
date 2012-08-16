using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using CameraControl.Classes;
using CameraControl.Devices.Classes;
using WIA;

namespace CameraControl.Devices.Others
{
  public class WiaCameraDevice : BaseFieldClass, ICameraDevice
  {
       Dictionary<int,string> ShutterTable = new Dictionary<int, string>
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
                         {15, "1/640"},
                         {20, "1/500"},
                         {25, "1/400"},
                         {31, "1/320"},
                         {40, "1/250"},
                         {50, "1/200"},
                         {62, "1/160"},
                         {80, "1/125"},
                         {100, "1/100"},
                         {125, "1/80"},
                         {166, "1/60"},
                         {200, "1/50"},
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
                         {7692, "1/1.3"},
                         {10000, "1s"},
                         {13000, "1.3s"},
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
                         {-1, "Bulb"},
                       };
     Dictionary<int,string> ExposureModeTable = new Dictionary<int, string>()
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

     Dictionary<int, string> WbTable = new Dictionary<int, string>()
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

    private Dictionary<int, string> CSTable = new Dictionary<int, string>()
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

    private Dictionary<int,string> EMMTable=new Dictionary<int, string>
                                              {
                                                {2, "Center-weighted metering"},
                                                {3, "Multi-pattern metering"},
                                                {4, "Spot metering"}
                                              }; 
    private Dictionary<uint, string> FMTable=new Dictionary<uint, string>()
                                              {
                                                {1, "[M] Manual focus"},
                                                {0x8010, "[S] Single AF servo"},
                                                {0x8011, "[C] Continuous AF servo"},
                                                {0x8012, "[A] AF servo mode automatic switching"},
                                                {0x8013, "[F] Constant AF servo"},
                                              };

    #region Implementation of ICameraDevice

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

    public bool CaptureInSdRam { get; set; }


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

    private PropertyValue<int> _isoNumber;
    public virtual PropertyValue<int> IsoNumber
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

    public PropertyValue<uint> FocusMode { get; set; }
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

    private string _displayName;
    public string DisplayName
    {
      get
      {
        if (string.IsNullOrEmpty(_displayName))
          return DeviceName + " (" + SerialNumber + ")" + "(WIA)";
        return _displayName;
      }
      set
      {
        _displayName = value;
        NotifyPropertyChanged("DisplayName");
      }
    }

    private int _exposureStatus;
    public int ExposureStatus
    {
      get { return _exposureStatus; }
      set
      {
        _exposureStatus = value;
        NotifyPropertyChanged("ExposureStatus");
      }
    }

    private bool _isConected;

    public bool IsConnected
    {
      get { return _isConected; }
      set
      {
        _isConected = value;
        NotifyPropertyChanged("IsConected");
      }
    }


    private int _battery;
    public bool GetCapability(CapabilityEnum capabilityEnum)
    {
      return false;
    }

    public int Battery
    {
      get { return _battery; }
      set
      {
        _battery = value;
        NotifyPropertyChanged("Battery");
      }
    }

    public virtual byte LiveViewImageZoomRatio { get; set; }

    private Device Device { get; set; }
    internal object Locker = new object(); // object used to lock multi hreaded mothods 
    public DeviceManager DeviceManager { get; set; }

    public virtual bool Init(DeviceDescriptor deviceDescriptor)
    {
      //the device not connected
      try
      {
        if (deviceDescriptor.WiaDevice == null)
        {
          Thread.Sleep(500);
          deviceDescriptor.WiaDevice = deviceDescriptor.WiaDeviceInfo.Connect();
          deviceDescriptor.CameraDevice = this;
          Thread.Sleep(250);
        }
      }
      catch (Exception exception)
      {
        ServiceProvider.Log.Error("Unable to connect camera using wia driver", exception);
        return false;
      }
      DeviceManager = new DeviceManager();
      DeviceManager.RegisterEvent(Conts.wiaEventItemCreated, deviceDescriptor.WiaId);
      DeviceManager.OnEvent += DeviceManager_OnEvent;

      try
      {
        Device = deviceDescriptor.WiaDevice;
        DeviceName = Device.Properties["Description"].get_Value();
        Manufacturer = Device.Properties["Manufacturer"].get_Value();
        SerialNumber = PhotoUtils.GetSerial(Device.Properties["PnP ID String"].get_Value());
      }
      catch (Exception ex)
      {
        ServiceProvider.Log.Error(ex);
      }
      IsConnected = true;
      try
      {
        try
        {
          Property apertureProperty = Device.Properties[WIAManager.CONST_PROP_F_Number];
          if (apertureProperty != null)
          {
            foreach (var subTypeValue in apertureProperty.SubTypeValues)
            {
              double d = (int) subTypeValue;
              string s = "f/" + (d/100).ToString("0.0");
              FNumber.AddValues(s, (int) d);
              if ((int) subTypeValue == (int) apertureProperty.get_Value())
                FNumber.SetValue((int) d);
            }
          }
        }
        catch (COMException)
        {
          FNumber.IsEnabled = false;
        }

        try
        {
          Property isoProperty = Device.Properties[WIAManager.CONST_PROP_ISO_Number];
          if (isoProperty != null)
          {
            foreach (var subTypeValue in isoProperty.SubTypeValues)
            {
              IsoNumber.AddValues(subTypeValue.ToString(), (int) subTypeValue);
              if ((int) subTypeValue == (int) isoProperty.get_Value())
                IsoNumber.SetValue((int) subTypeValue);
            }
          }
        }
        catch (COMException)
        {
          IsoNumber.IsEnabled = false;
        }

        try
        {
          Property shutterProperty = Device.Properties[WIAManager.CONST_PROP_Exposure_Time];
          if (shutterProperty != null)
          {
            foreach (int subTypeValue in shutterProperty.SubTypeValues)
            {
              if (ShutterTable.ContainsKey((int) subTypeValue))
                ShutterSpeed.AddValues(ShutterTable[(int) subTypeValue], (int) subTypeValue);
            }
            ShutterSpeed.SetValue(shutterProperty.get_Value());
          }
        }
        catch (COMException)
        {
          ShutterSpeed.IsEnabled = false;
        }

        try
        {
          Property wbProperty = Device.Properties[WIAManager.CONST_PROP_WhiteBalance];
          if (wbProperty != null)
          {
            foreach (var subTypeValue in wbProperty.SubTypeValues)
            {
              if (WbTable.ContainsKey((int) subTypeValue))
                WhiteBalance.AddValues(WbTable[(int) subTypeValue], (int) subTypeValue);
            }
            WhiteBalance.SetValue(wbProperty.get_Value());
          }
        }
        catch (COMException)
        {
          WhiteBalance.IsEnabled = false;
        }

        try
        {
          Property modeProperty = Device.Properties[WIAManager.CONST_PROP_ExposureMode];
          if (modeProperty != null)
          {
            foreach (var subTypeValue in modeProperty.SubTypeValues)
            {
              if (ExposureModeTable.ContainsKey((int) subTypeValue))
                Mode.AddValues(ExposureModeTable[(int) subTypeValue], Convert.ToUInt32(subTypeValue));
            }
            Mode.SetValue(Convert.ToUInt32(modeProperty.get_Value()));
          }
          Mode.IsEnabled = false;
        }
        catch (COMException)
        {
          Mode.IsEnabled = false;
        }

        try
        {
          Property ecProperty = Device.Properties[WIAManager.CONST_PROP_ExposureCompensation];
          if (ecProperty != null)
          {
            foreach (var subTypeValue in ecProperty.SubTypeValues)
            {
              decimal d = (int) subTypeValue;
              string s = decimal.Round(d/1000, 1).ToString();
              if (d > 0)
                s = "+" + s;
              ExposureCompensation.AddValues(s, (int) subTypeValue);
            }
            ExposureCompensation.SetValue(ecProperty.get_Value());
          }
        }
        catch (COMException)
        {
          ExposureCompensation.IsEnabled = false;
        }

        try
        {
          Property csProperty = Device.Properties[WIAManager.CONST_PROP_CompressionSetting];
          if (csProperty != null)
          {
            foreach (var subTypeValue in csProperty.SubTypeValues)
            {
              if (CSTable.ContainsKey((int) subTypeValue))
                CompressionSetting.AddValues(CSTable[(int) subTypeValue], (int) subTypeValue);
            }
            CompressionSetting.SetValue(csProperty.get_Value());
          }
        }
        catch (COMException)
        {
          CompressionSetting.IsEnabled = false;
        }

        try
        {
          Property emmProperty = Device.Properties[WIAManager.CONST_PROP_ExposureMeteringMode];
          if (emmProperty != null)
          {
            foreach (var subTypeValue in emmProperty.SubTypeValues)
            {
              if (EMMTable.ContainsKey((int) subTypeValue))
                ExposureMeteringMode.AddValues(EMMTable[(int) subTypeValue], (int) subTypeValue);
            }
            CompressionSetting.SetValue(emmProperty.get_Value());
          }
        }
        catch (COMException)
        {
          CompressionSetting.IsEnabled = false;
        }

        try
        {
          Property fmProperty = Device.Properties[WIAManager.CONST_PROP_FocusMode];
          if (fmProperty != null)
          {
            foreach (int subTypeValue in fmProperty.SubTypeValues)
            {
              uint subval = Convert.ToUInt16(subTypeValue);
              if (FMTable.ContainsKey(subval))
                FocusMode.AddValues(FMTable[subval], subval);
            }
            FocusMode.SetValue(Convert.ToUInt16((int) fmProperty.get_Value()));
          }
        }
        catch (COMException)
        {
          FocusMode.IsEnabled = false;
        }

        try
        {
          Battery = Device.Properties[WIAManager.CONST_PROP_BatteryStatus].get_Value();
        }
        catch (COMException)
        {
          Battery = 0;
        }
        IsConnected = true;
      }
      catch (Exception exception)
      {
        ServiceProvider.Log.Error(exception);
        IsConnected = false;
      }
      HaveLiveView = false;
      return true;
    }

    void DeviceManager_OnEvent(string EventID, string DeviceID, string ItemID)
    {
      if (PhotoCaptured != null)
      {
        PhotoCapturedEventArgs args = new PhotoCapturedEventArgs {WiaImageItem = Device.GetItem(ItemID)};
        PhotoCaptured(this, args);
      }
    }

    public WiaCameraDevice()
    {
      FNumber = new PropertyValue<int>();
      FNumber.ValueChanged += FNumber_ValueChanged;
      IsoNumber = new PropertyValue<int>();
      IsoNumber.ValueChanged += IsoNumber_ValueChanged;
      ShutterSpeed = new PropertyValue<long>();
      ShutterSpeed.ValueChanged += ShutterSpeed_ValueChanged;
      WhiteBalance = new PropertyValue<long>();
      WhiteBalance.ValueChanged += WhiteBalance_ValueChanged;
      Mode = new PropertyValue<uint>();
      Mode.ValueChanged += Mode_ValueChanged;
      CompressionSetting = new PropertyValue<int>();
      CompressionSetting.ValueChanged += CompressionSetting_ValueChanged;
      ExposureCompensation = new PropertyValue<int>();
      ExposureCompensation.ValueChanged += ExposureCompensation_ValueChanged;
      ExposureMeteringMode = new PropertyValue<int>();
      ExposureMeteringMode.ValueChanged += ExposureMeteringMode_ValueChanged;
      FocusMode = new PropertyValue<uint>();
      FocusMode.IsEnabled = false;

    }

    void ExposureMeteringMode_ValueChanged(object sender, string key, int val)
    {
      lock (Locker)
      {
        try
        {
          Device.Properties[WIAManager.CONST_PROP_ExposureMeteringMode].set_Value(val);
        }
        catch (Exception)
        {

        }
      }
    }

    void CompressionSetting_ValueChanged(object sender, string key, int val)
    {
      lock (Locker)
      {
        try
        {
          Device.Properties[WIAManager.CONST_PROP_CompressionSetting].set_Value(val);
        }
        catch (Exception)
        {

        }
      }
    }

    void ExposureCompensation_ValueChanged(object sender, string key, int val)
    {
      lock (Locker)
      {
        try
        {
          Device.Properties[WIAManager.CONST_PROP_ExposureCompensation].set_Value(val);
        }
        catch (Exception)
        {

        }
      }
    }

    void Mode_ValueChanged(object sender, string key, uint val)
    {
      lock (Locker)
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
        Device.Properties[WIAManager.CONST_PROP_ExposureMode].set_Value(val);
      }
    }

    void WhiteBalance_ValueChanged(object sender, string key, long val)
    {
      lock (Locker)
      {
        try
        {
          Device.Properties[WIAManager.CONST_PROP_WhiteBalance].set_Value(val);
        }
        catch (Exception)
        {
          
        }
      }
    }

    void ShutterSpeed_ValueChanged(object sender, string key, long val)
    {
      lock (Locker)
      {
        try
        {
          Device.Properties[WIAManager.CONST_PROP_Exposure_Time].set_Value(val);
        }
        catch (Exception)
        {

        }
      }
    }

    void IsoNumber_ValueChanged(object sender, string key, int val)
    {
      lock (Locker)
      {
        try
        {
          Device.Properties[WIAManager.CONST_PROP_ISO_Number].set_Value(val);
        }
        catch (Exception)
        {

        }
      }
    }

    void FNumber_ValueChanged(object sender, string key, int val)
    {
      lock (Locker)
      {
        try
        {
          Device.Properties[WIAManager.CONST_PROP_F_Number].set_Value(val);
        }
        catch (Exception)
        {

        }
      }
    }

 
    public virtual void StartLiveView()
    {
      //throw new NotImplementedException();
    }

    public virtual void StopLiveView()
    {
      //throw new NotImplementedException();
    }

    public virtual LiveViewData GetLiveViewImage()
    {
      //throw new NotImplementedException();
      return new LiveViewData();
    }

    public virtual void AutoFocus()
    {
      //throw new NotImplementedException();
    }

    public virtual void Focus(int step)
    {
      //throw new NotImplementedException();
    }

    public virtual void Focus(int x, int y)
    {
      //throw new NotImplementedException();
    }

    public virtual void CapturePhotoNoAf()
    {
      lock (Locker)
      {
          CapturePhoto();
      }
    }

    public virtual void CapturePhoto()
    {
      Monitor.Enter(Locker);
      try
      {
        Device.ExecuteCommand(Conts.wiaCommandTakePicture);
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

    public void EndBulbMode()
    {
      throw new NotImplementedException();
    }

    public void StartBulbMode()
    {
      throw new NotImplementedException();
    }

    public void LockCamera()
    {
      throw new NotImplementedException();
    }

    public void UnLockCamera()
    {
      throw new NotImplementedException();
    }

    public virtual void Close()
    {
      if (Device != null)
        Marshal.ReleaseComObject(Device);
      Device = null;
      HaveLiveView = false;
      if (DeviceName != null)
        DeviceManager.OnEvent -= DeviceManager_OnEvent;
    }

    public virtual void ReadDeviceProperties(int o)
    {
      HaveLiveView = false;
    }

    public void TransferFile(object o, string filename)
    {
      
    }

    public event PhotoCapturedEventHandler PhotoCaptured;

    #endregion
  }
}
