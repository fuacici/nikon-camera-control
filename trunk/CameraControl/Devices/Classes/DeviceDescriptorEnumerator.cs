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
  }
}
