using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Devices.Classes;

namespace CameraControl.Core.Classes
{
    public class CustomConfig
    {
        public string Name { get; set; }
        public AsyncObservableCollection<ValuePair> ConfigData { get; set; }
        public string DriverName { get; set; }
        public SourceEnum SourceEnum { get; set; }
    }
}
