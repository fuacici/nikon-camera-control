using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraControl.Devices.Classes
{
  public class DeviceDescriptorEnumerator
  {
    public List<DeviceDescriptor> Devices { get; set; }

    public DeviceDescriptorEnumerator()
    {
      Devices = new List<DeviceDescriptor>();
    }

    public DeviceDescriptor GetBySerialNumber(string serial)
    {
      return Devices.FirstOrDefault(deviceDescriptor => deviceDescriptor.SerialNumber == serial);
    }
    
    public DeviceDescriptor GetByWiaId(string id)
    {
      return Devices.FirstOrDefault(deviceDescriptor => deviceDescriptor.WiaId == id);
    }

    public void Add(DeviceDescriptor descriptor)
    {
      Devices.Add(descriptor);
    }

    public void Remove(DeviceDescriptor descriptor)
    {
      Devices.Remove(descriptor);
    }

  }
}
