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
            get { return string.Format("[{0}][Time={1}]", Name, Time ); }
            set { }
        }

        private int _time;
        public int Time
        {
            get { return _time; }
            set
            {
                _time = value;
                NotifyPropertyChanged("Time");
                NotifyPropertyChanged("DisplayName");
            }
        }

        public override bool Execute(ScriptObject scriptObject)
        {
            Executing = true;
            for (int i = 0; i < Time; i++)
            {
                Thread.Sleep(1000);
                ServiceProvider.ScriptManager.OutPut(string.Format("Waiting .... {0}/{1}", i + 1, Time));
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
            XmlNode nameNode = doc.CreateElement("wait");
            nameNode.Attributes.Append(ScriptManager.CreateAttribute(doc, "time", Time.ToString()));
            return nameNode;
        }

        public override IScriptCommand Load(XmlNode node)
        {
            WaitCommand res = new WaitCommand()
            {
                Time = Convert.ToInt32(ScriptManager.GetValue(node, "time")),
            };
            return res;
        }

        public WaitCommand()
        {
            Name = "wait";
            Description = "Wait the specified seconds in time parameter";
            DefaultValue = "wait time=\"2\"";
            Time = 1;
        }
    }
}
