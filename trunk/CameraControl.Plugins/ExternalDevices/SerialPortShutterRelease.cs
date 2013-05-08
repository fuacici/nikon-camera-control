using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;

namespace CameraControl.Plugins.ExternalDevices
{
    public class SerialPortShutterRelease : IExternalShutterReleaseSource
    {
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

        #endregion

        public SerialPortShutterRelease()
        {
            Name = "Serial Port Shutter Release";
        }
    }
}
