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
    public class MultiCameraBoxShutterRelease : IExternalDevice
    {
        private SerialPort sp = new SerialPort();
        #region Implementation of IExternalDevice

        public string Name { get; set; }
        public bool Capture(CustomConfig config)
        {
            return true;
        }

        public bool Focus(CustomConfig config)
        {
            SerialPort serialPort = new SerialPort(config.Get("Port"));
            serialPort.Open();
            serialPort.RtsEnable = true;
            config.AttachedObject = serialPort;
            return true;
        }

        public bool CanExecute(CustomConfig config)
        {
            return true;
        }

        public UserControl GetConfig(CustomConfig config)
        {
            return new SerialPortShutterReleaseConfig(config);
        }

        public SourceEnum DeviceType { get; set; }
        public bool Start(CustomConfig config)
        {
            throw new NotImplementedException();
        }

        public bool Stop(CustomConfig config)
        {
            throw new NotImplementedException();
        }

        #endregion

        public MultiCameraBoxShutterRelease()
        {
            Name = "Multi camera Shutter Release";
            DeviceType = SourceEnum.ExternaExternalShutterRelease;
        }
    }
}
