using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraControl.Devices.Xml
{
    public class XmlDeviceData
    {
        public List<XmlCommandDescriptor> AvaiableCommands { get; set; }
        public List<XmlEventDescriptor> AvaiableEvents { get; set; }
        public List<XmlPropertyDescriptor> AvaiableProperties { get; set; }

        public string Model { get; set; }
        public string Manufacturer { get; set; }


        public XmlDeviceData()
        {
            AvaiableCommands = new List<XmlCommandDescriptor>();
            AvaiableEvents = new List<XmlEventDescriptor>();
            AvaiableProperties = new List<XmlPropertyDescriptor>();
        }


    }
}
