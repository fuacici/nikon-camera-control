using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraControl.Devices.Xml
{
    public class XmlEventDescriptor
    {
        public uint Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        private string _hexCode;
        public string HexCode
        {
            get
            {
                _hexCode = Code.ToString("X");
                return _hexCode;
            }
            set { _hexCode = value; }
        }

        public override string ToString()
        {
            return HexCode + Name;
        }
    }
}
