using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml.Serialization;
using CameraControl.Core.Interfaces;
using CameraControl.Devices.Classes;

namespace CameraControl.Core.Classes
{
    public class CustomConfig
    {
        public string Name { get; set; }
        public AsyncObservableCollection<ValuePair> ConfigData { get; set; }
        private string _driverName;
        public string DriverName
        {
            get { return _driverName; }
            set
            {
                _driverName = value;
                IExternalShutterReleaseSource source = ServiceProvider.ExternalDeviceManager.Get(_driverName);
                Config = source.GetConfig(this);
            }
        }

        public SourceEnum SourceEnum { get; set; }
        [XmlIgnore]
        public UserControl Config { get; set; }
    }
}
