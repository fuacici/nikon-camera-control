using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using CameraControl.Devices.Classes;

namespace CameraControl.Core.Classes
{
    public class LiveviewSettings : BaseFieldClass
    {
        [XmlIgnore]
        public double CanvasWidt { get; set; }

        [XmlIgnore]
        public double CanvasHeight { get; set; }
        
        [XmlIgnore]
        private double _gridVerticalMinSize;
        public double GridVerticalMinSize
        {
            get { return _gridVerticalMinSize; }
            set
            {
                _gridVerticalMinSize = value;
                NotifyPropertyChanged("GridVerticalMinSize");
            }
        }

        private double _gridVerticalMin;
        public double GridVerticalMin
        {
            get { return _gridVerticalMin; }
            set
            {
                _gridVerticalMin = value;
                GridVerticalMinSize = CanvasHeight * (100-_gridVerticalMin) / 100;
                NotifyPropertyChanged("GridVerticalMin");
            }
        }
    }
}
