using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using CameraControl.Core.Classes;
using CameraControl.Devices;

namespace CameraControl.Core.Plugin
{
    public class PluginInfo
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string AssemblyFileName { get; set; }
        [XmlIgnore]
        public bool Enabled { get; set; }

        public static PluginInfo Load(string filename)
        {
            PluginInfo pluginInfo = new PluginInfo();
            try
            {
                if (File.Exists(filename))
                {
                    XmlSerializer mySerializer =
                      new XmlSerializer(typeof(PluginInfo));
                    FileStream myFileStream = new FileStream(filename, FileMode.Open);
                    pluginInfo = (PluginInfo)mySerializer.Deserialize(myFileStream);
                    myFileStream.Close();
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
            return pluginInfo;    
        }

    }
}
