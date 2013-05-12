using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;

namespace CameraControl.Plugins.ExternalDevices
{
    public class SerialPortShutterRelease : IExternalDevice
    {
        private SerialPort _serialPort =null;
        #region Implementation of IExternalShutterReleaseSource

        public string Name { get; set; }
        
        public bool Execute(CustomConfig config)
        {
            return true;
        }

        public bool CanExecute(CustomConfig config)
        {
            return true;
        }

        public UserControl GetConfig(CustomConfig config)
        {
            return new SerialPortShutterReleaseConfig();
        }

        public SourceEnum DeviceType { get; set; }
        public bool Start(CustomConfig config)
        {
           _serialPort=new SerialPort();
            return true;
        }

        public bool Stop(CustomConfig config)
        {
            return true;
        }

        #endregion

        public SerialPortShutterRelease()
        {
            Name = "Serial Port Shutter Release";
            DeviceType=SourceEnum.ExternaExternalShutterRelease;
        }
    }
}
