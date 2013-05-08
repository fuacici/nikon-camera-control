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
    public class ExternalDeviceManager:BaseFieldClass
    {
        private AsyncObservableCollection<IExternalShutterReleaseSource> _externalShutterReleaseSources;
        public AsyncObservableCollection<IExternalShutterReleaseSource> ExternalShutterReleaseSources
        {
            get { return _externalShutterReleaseSources; }
            set
            {
                _externalShutterReleaseSources = value;
                NotifyPropertyChanged("ExternalShutterReleaseSources");
            }
        }

        public AsyncObservableCollection<string> ExternalShutterReleaseSourcesNames
        {
            get
            {
                var res = new AsyncObservableCollection<string>();
                foreach (IExternalShutterReleaseSource externalShutterReleaseSource in ExternalShutterReleaseSources)
                {
                    res.Add(externalShutterReleaseSource.Name);
                }
                return res;
            }
        }

        public IExternalShutterReleaseSource Get(string name)
        {
            return ExternalShutterReleaseSources.FirstOrDefault(externalShutterReleaseSource => externalShutterReleaseSource.Name == name);
        }

        public ExternalDeviceManager()
        {
            ExternalShutterReleaseSources = new AsyncObservableCollection<IExternalShutterReleaseSource>();
        }
    }
}
