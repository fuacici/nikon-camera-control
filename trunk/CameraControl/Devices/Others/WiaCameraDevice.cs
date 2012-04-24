using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CameraControl.Classes;
using CameraControl.Devices.Classes;
using WIA;

namespace CameraControl.Devices.Others
{
  public class WiaCameraDevice : BaseFieldClass, ICameraDevice
  {
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

    private PropertyValue _fNumber;
    public PropertyValue FNumber
    {
      get { return _fNumber; }
      set
      {
        _fNumber = value;
        NotifyPropertyChanged("FNumber");
      }
    }

    private Device Device { get; set; }

    public bool Init(string id, WIAManager manager)
    {
      Device = manager.Device;

      FNumber = new PropertyValue();

      Property apertureProperty = manager.Device.Properties[WIAManager.CONST_PROP_F_Number];
      if (apertureProperty != null)
      {
        foreach (var subTypeValue in apertureProperty.SubTypeValues)
        {
          double d = (int)subTypeValue;
          string s = "f/" + (d / 100).ToString("0.0");
          FNumber.AddValues(s, d);
          if ((int)subTypeValue == (int)apertureProperty.get_Value())
            FNumber.SetValue(d);
        }
      }

      HaveLiveView = false;
      return true;
    }

    public void StartLiveView()
    {
      throw new NotImplementedException();
    }

    public void StopLiveView()
    {
      throw new NotImplementedException();
    }

    public LiveViewData GetLiveViewImage()
    {
      throw new NotImplementedException();
    }

    public void AutoFocus()
    {
      throw new NotImplementedException();
    }

    public void Focus(int step)
    {
      throw new NotImplementedException();
    }

    public void Focus(int x, int y)
    {
      throw new NotImplementedException();
    }

    public void TakePictureNoAf()
    {
      throw new NotImplementedException();
    }

    public void Close()
    {
      if (Device != null)
        Marshal.ReleaseComObject(Device);
      Device = null;
      HaveLiveView = false;
    }

    public void ReadDeviceProperties()
    {
      HaveLiveView = false;
    }

    #endregion
  }
}
