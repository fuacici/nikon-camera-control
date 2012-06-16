using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CameraControl.Classes;
using CameraControl.Devices.Nikon;
using CameraControl.Devices.Others;
using PortableDeviceLib;

namespace CameraControl.Devices
{
  public class CameraDeviceManager : BaseFieldClass
  {
    private const string AppName = "CameraControl";
    private const int AppMajorVersionNumber = 1;
    private const int AppMinorVersionNumber = 0;

    
    public Dictionary<string,Type> DeviceClass { get; set; }

    private  ICameraDevice _selectedCameraDevice;
    public  ICameraDevice SelectedCameraDevice
    {
      get { return _selectedCameraDevice; }
      set
      {
        _selectedCameraDevice = value;
        NotifyPropertyChanged("SelectedCameraDevice");
      }
    }

    public CameraDeviceManager()
    {
      DeviceClass = new Dictionary<string, Type>
                      {
                        {"D200", typeof (NikonD40)},
                        {"D3S", typeof (NikonD90)},
                        {"D3X", typeof (NikonD3X)},
                        {"D300", typeof (NikonD3X)},
                        {"D300S", typeof (NikonD3X)},
                        {"D3200", typeof (NikonD3200)},
                        {"D4", typeof (NikonD5100)},
                        {"D40", typeof (NikonD40)},
                        {"D5100", typeof (NikonD5100)},
                        {"D5000", typeof (NikonD90)},
                        {"D60", typeof (NikonD40)},
                        {"D70", typeof (NikonD40)},
                        {"D70S", typeof (NikonD40)},
                        {"D700", typeof (NikonD3X)},
                        {"D7000", typeof (NikonD5100)},
                        {"D80", typeof (NikonD40)},
                        {"D800", typeof (NikonD5100)},
                        {"D800E", typeof (NikonD5100)},
                        {"D90", typeof (NikonD90)},
                      };
      SelectedCameraDevice = new NotConnectedCameraDevice();
    }

    //TODO: need to be fixed same type cameras isn't handled right 
    public ICameraDevice GetIDevice(WIAManager manager)
    {
      WiaCameraDevice wiaCameraDevice=new WiaCameraDevice();
      wiaCameraDevice.Init(manager.DeviceId, manager);

      if (!ServiceProvider.Settings.DisableNativeDrivers)
      {
        ObservableCollection<PortableDevice> PortableDevices = new ObservableCollection<PortableDevice>();
        if (PortableDeviceCollection.Instance == null)
        {
          PortableDeviceCollection.CreateInstance(AppName, AppMajorVersionNumber, AppMinorVersionNumber);
          PortableDeviceCollection.Instance.AutoConnectToPortableDevice = false;
        }

        foreach (var device in PortableDeviceCollection.Instance.Devices)
        {
          if (DeviceClass.ContainsKey(wiaCameraDevice.DeviceName.ToUpper()))
          {
            SelectedCameraDevice = (ICameraDevice)Activator.CreateInstance(DeviceClass[wiaCameraDevice.DeviceName]);
            SelectedCameraDevice.Init(device.DeviceId, manager);
            return SelectedCameraDevice;
          }
          break;
        }
      }

      SelectedCameraDevice = wiaCameraDevice;
      //SelectedCameraDevice.Init(manager.DeviceId, manager);
      return SelectedCameraDevice;
    }
  }
}
