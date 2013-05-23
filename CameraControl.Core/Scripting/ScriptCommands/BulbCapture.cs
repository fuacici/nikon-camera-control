using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml;

namespace CameraControl.Core.Scripting.ScriptCommands
{
    public class BulbCapture : IScriptCommand
    {
        #region Implementation of IScriptCommand

        public bool Execute()
        {
            return true;
        }

        public IScriptCommand Create()
        {
            return new BulbCapture();
        }

        public XmlNode Save(XmlDocument doc)
        {
            XmlNode nameNode = doc.CreateElement("BulbCapture");
            XmlAttribute attribute = doc.CreateAttribute("CaptureTime");
            attribute.Value = CaptureTime.ToString();
            nameNode.Attributes.Append(attribute);
            return nameNode;
        }

        public void Load(XmlNode node)
        {
            
        }

        public bool IsExecuted { get; set; }

        public bool Executing { get; set; }

        public string Name { get; set; }

        public string DisplayName
        {
            get { return string.Format("[{0}]", Name); }
            set { }
        }

        public UserControl GetConfig()
        {
            return new BulbCaptureControl(this);
        }

        #endregion

        public int CaptureTime { get; set; }


        public BulbCapture()
        {
            Name = "BulbCapture";
            IsExecuted = false;
            Executing = false;
        }
    }
}
