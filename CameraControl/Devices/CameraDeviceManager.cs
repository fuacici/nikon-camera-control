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
                        //{"D5100", typeof (NikonD5100)},
                        {"D7000", typeof (NikonD5100)},
                        {"D90", typeof (NikonD90)},
                        {"D4", typeof (NikonD5100)}
                      };
      SelectedCameraDevice = new NotConnectedCameraDevice();
    }

    //TODO: need to be fixed same type cameras isn't handled right 
    public ICameraDevice GetIDevice(WIAManager manager)
    {
      ObservableCollection<PortableDevice>  PortableDevices = new ObservableCollection<PortableDevice>();
      if (PortableDeviceCollection.Instance == null)
      {
        PortableDeviceCollection.CreateInstance(AppName, AppMajorVersionNumber, AppMinorVersionNumber);
        PortableDeviceCollection.Instance.AutoConnectToPortableDevice = false;
      }

      foreach (var device in PortableDeviceCollection.Instance.Devices)
      {
         if(DeviceClass.ContainsKey(manager.DeviceName))
         {
           SelectedCameraDevice = (ICameraDevice)Activator.CreateInstance(DeviceClass[manager.DeviceName]);
           SelectedCameraDevice.Init(device.DeviceId, manager);
           return SelectedCameraDevice;
         }
        break;
      }
      SelectedCameraDevice = new WiaCameraDevice();
      SelectedCameraDevice.Init(manager.DeviceId, manager);
      return SelectedCameraDevice;
    }
  }
}
