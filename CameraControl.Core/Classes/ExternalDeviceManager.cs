using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Core.Interfaces;
using CameraControl.Devices.Classes;

namespace CameraControl.Core.Classes
{
    /// <summary>
    /// Manager for external device handing like external shutter release
    /// </summary>
    public class ExternalDeviceManager
    {
        public AsyncObservableCollection<IExternalShutterReleaseSource> ExternalShutterReleaseSources { get; set; }

        public ExternalDeviceManager()
        {
            ExternalShutterReleaseSources = new AsyncObservableCollection<IExternalShutterReleaseSource>();
        }
    }
}
