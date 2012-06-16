using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraControl.Devices.Classes
{
  public class DeviceDescriptor
  {
    public string SerialNumber { get; set; }
    public string WiaId { get; set; }
    public string WpdId { get; set; }
    public ICameraDevice CameraDevice { get; set; }
  }
}
