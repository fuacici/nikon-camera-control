﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml.Serialization;
using CameraControl.Core.Interfaces;
using CameraControl.Devices.Classes;

namespace CameraControl.Core.Classes
{
    public class CustomConfig:BaseFieldClass
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
                IExternalDevice source = ServiceProvider.ExternalDeviceManager.Get(_driverName);
                if (source != null)
                {
                    Config = source.GetConfig(this);
                    SourceEnum = source.DeviceType;
                }
                NotifyPropertyChanged("DriverName");
            }
        }

        public SourceEnum SourceEnum { get; set; }
        private UserControl _config;

        [XmlIgnore]
        public UserControl Config
        {
            get
            {
                IExternalDevice source = ServiceProvider.ExternalDeviceManager.Get(_driverName);
                if (source != null)
                {
                    _config = source.GetConfig(this);
                }
                return _config;
            }
            set
            {
                _config = value;
                NotifyPropertyChanged("Config");
            }
        }
    }
}
