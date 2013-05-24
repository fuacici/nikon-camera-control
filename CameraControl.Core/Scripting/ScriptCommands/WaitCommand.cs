using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Xml;
using CameraControl.Devices;

namespace CameraControl.Core.Scripting.ScriptCommands
{
    public class WaitCommand:BaseScript
    {

        public override string DisplayName
        {
            get { return string.Format("[{0}][WaitTime={1}]", Name, WaitTime ); }
            set { }
        }

        private int _waitTIme;
        public int WaitTime
        {
            get { return _waitTIme; }
            set
            {
                _waitTIme = value;
                NotifyPropertyChanged("WaitTime");
                NotifyPropertyChanged("DisplayName");
            }
        }

        public override bool Execute(ScriptObject scriptObject)
        {
            Executing = true;
            for (int i = 0; i < WaitTime; i++)
            {
                Thread.Sleep(1000);
                StaticHelper.Instance.SystemMessage = string.Format("Waiting .... {0}/{1}", i + 1, WaitTime);
            }
            Executing = false;
            IsExecuted = true;
            return true;
        }

        public override IScriptCommand Create()
        {
            return new WaitCommand();
        }

        public override UserControl GetConfig()
        {
            return new WaitCommandControl(this);
        }

        public override XmlNode Save(XmlDocument doc)
        {
            XmlNode nameNode = doc.CreateElement("WaitCommand");
            nameNode.Attributes.Append(ScriptManager.CreateAttribute(doc, "WaitTime", WaitTime.ToString()));
            return nameNode;
        }

        public override IScriptCommand Load(XmlNode node)
        {
            WaitCommand res = new WaitCommand()
            {
                WaitTime = Convert.ToInt32(ScriptManager.GetValue(node, "WaitTime")),
            };
            return res;
        }

        public WaitCommand()
        {
            Name = "WaitCommand";
            WaitTime = 1;
        }
    }
}
