using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraControl.Devices.Classes
{
  public class LogEventArgs : EventArgs
  {
    public object Message { get; set; }
    public Exception Exception { get; set; }
  }
}
