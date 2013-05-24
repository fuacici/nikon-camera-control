using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Xml;
using CameraControl.Devices.Classes;

namespace CameraControl.Core.Scripting.ScriptCommands
{
    public class BulbCapture :BaseFieldClass, IScriptCommand
    {
        #region Implementation of IScriptCommand

        public bool Execute(ScriptObject scriptObject)
        {
            Executing = true;
            scriptObject.CameraDevice.IsoNumber.SetValue(Iso);
            Thread.Sleep(200);
            scriptObject.CameraDevice.FNumber.SetValue(Aperture);
            Thread.Sleep(200);
            scriptObject.StartCapture();
            Thread.Sleep(CaptureTime*1000);
            scriptObject.StopCapture();
            Thread.Sleep(200);
            Executing = false;
            IsExecuted = true;
            return true;
        }

        public IScriptCommand Create()
        {
            return new BulbCapture();
        }

        public XmlNode Save(XmlDocument doc)
        {
            XmlNode nameNode = doc.CreateElement("BulbCapture");
            nameNode.Attributes.Append(ScriptManager.CreateAttribute(doc, "CaptureTime", CaptureTime.ToString()));
            nameNode.Attributes.Append(ScriptManager.CreateAttribute(doc, "Iso", Iso));
            nameNode.Attributes.Append(ScriptManager.CreateAttribute(doc, "Aperture", Aperture));
            return nameNode;
        }

        public IScriptCommand Load(XmlNode node)
        {
            BulbCapture res = new BulbCapture
                                  {
                                      CaptureTime = Convert.ToInt32(ScriptManager.GetValue(node, "CaptureTime")),
                                      Iso = ScriptManager.GetValue(node, "Iso"),
                                      Aperture = ScriptManager.GetValue(node, "Aperture")
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

    }
}
