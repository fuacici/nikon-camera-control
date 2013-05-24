using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml;
using CameraControl.Devices.Classes;

namespace CameraControl.Core.Scripting.ScriptCommands
{
    public class BulbCapture :BaseFieldClass, IScriptCommand
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
            nameNode.Attributes.Append(CreateAttribute(doc, "CaptureTime", CaptureTime.ToString()));
            nameNode.Attributes.Append(CreateAttribute(doc, "Iso", Iso));
            nameNode.Attributes.Append(CreateAttribute(doc, "Aperture", Aperture));
            return nameNode;
        }

        public IScriptCommand Load(XmlNode node)
        {
            BulbCapture res = new BulbCapture
                                  {
                                      CaptureTime = Convert.ToInt32(GetValue(node,"CaptureTime")),
                                      Iso = GetValue(node,"Iso"),
                                      Aperture = GetValue(node,"Aperture")
                                  };
            return res;
        }

        public bool IsExecuted { get; set; }

        public bool Executing { get; set; }

        public string Name { get; set; }

        public string DisplayName
        {
            get { return string.Format("[{0}][CaptureTime={1}, Iso={2}, Aperture={3}]", Name, CaptureTime, Iso,Aperture); }
            set { }
        }

        public UserControl GetConfig()
        {
            return new BulbCaptureControl(this);
        }

        #endregion

        private int _captureTime;
        public int CaptureTime
        {
            get { return _captureTime; }
            set
            {
                _captureTime = value;
                NotifyPropertyChanged("CaptureTime");
                NotifyPropertyChanged("DisplayName");
            }
        }

        private string _iso;
        public string Iso
        {
            get { return _iso; }
            set
            {
                _iso = value;
                NotifyPropertyChanged("Iso");
                NotifyPropertyChanged("DisplayName");
            }
        }

        private string _aperture;
        public string Aperture
        {
            get { return _aperture; }
            set
            {
                _aperture = value;
                NotifyPropertyChanged("Aperture");
                NotifyPropertyChanged("DisplayName");
            }
        }

        public BulbCapture()
        {
            Name = "BulbCapture";
            IsExecuted = false;
            Executing = false;
            CaptureTime = 30;
        }

        public string GetValue(XmlNode node, string atribute)
        {
            if (node.Attributes != null && node.Attributes[atribute] == null)
                return "";
            return node.Attributes[atribute].Value;
        }

        public XmlAttribute CreateAttribute(XmlDocument doc, string name,string val)
        {
            XmlAttribute attribute = doc.CreateAttribute(name);
            attribute.Value = val;
            return attribute;
        }

    }
}
