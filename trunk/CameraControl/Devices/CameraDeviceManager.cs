using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CameraControl.Classes;
using CameraControl.Devices.Nikon;
using PortableDeviceLib;

namespace CameraControl.Devices
{
  public class CameraDeviceManager
  {
    private const string AppName = "CameraControl";
    private const int AppMajorVersionNumber = 1;
    private const int AppMinorVersionNumber = 0;

    
    public Dictionary<string,Type> DeviceClass { get; set; }

    public CameraDeviceManager()
    {
      DeviceClass = new Dictionary<string, Type>
                      {
                        {"D5100", typeof (NikonD5100)},
                        {"D7000", typeof (NikonD5100)},
                        {"D90", typeof (NikonD90)},
                        {"D4", typeof (NikonD5100)}
                      };
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
        //string pnp_id = manager.Device.Properties["PnP ID String"].get_Value();
        //if (device.DeviceId == manager.Device.Properties["PnP ID String"].get_Value())
        //{
         if(DeviceClass.ContainsKey(manager.DeviceName))
         {
           ICameraDevice dv= (ICameraDevice)Activator.CreateInstance(DeviceClass[manager.DeviceName]);
           dv.Init(device.DeviceId, manager);
           return dv;
         }
        //}
        //this.PortableDevices.Add(device);
        //NikonD5100 portableDevice = new NikonD5100(device.DeviceId, manager);
        //this.SelectedPortableDevice = portableDevice;
        break;
      }
      return null;
    }
  }
}
