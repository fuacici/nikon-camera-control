using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using CameraControl.Core.Classes;
using CameraControl.Core.Scripting.ScriptCommands;
using CameraControl.Devices;
using CameraControl.Devices.Classes;

namespace CameraControl.Core.Scripting
{
    public class ScriptManager : BaseFieldClass
    {
        public ValuePairEnumerator Variabiles { get; set; }


        public delegate void MessageEventHandler(object sender, MessageEventArgs e);

        public event MessageEventHandler OutPutMessageReceived;

        private bool _shouldStop = false;

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                NotifyPropertyChanged("IsBusy");
            }
        }


        public List<IScriptCommand> AvaiableCommands { get; set; }

        public ScriptManager()
        {
            AvaiableCommands = new List<IScriptCommand>
                                   {new BulbCapture(), new WaitCommand(), new PHDGuiding(), new Echo()};
            Variabiles = new ValuePairEnumerator();
        }

        public void Save(ScriptObject scriptObject, string fileName)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);
            XmlNode rootNode = doc.CreateElement("dccscript");
            rootNode.Attributes.Append(CreateAttribute(doc, "UseExternal", scriptObject.UseExternal ? "true" : "false"));
            rootNode.Attributes.Append(CreateAttribute(doc, "SelectedConfig", scriptObject.SelectedConfig.Name));
            doc.AppendChild(rootNode);
            XmlNode commandsNode = doc.CreateElement("commands");
            rootNode.AppendChild(commandsNode);
            foreach (IScriptCommand avaiableCommand in scriptObject.Commands)
            {
                commandsNode.AppendChild(avaiableCommand.Save(doc));
            }
            doc.Save(fileName);
        }

        public ScriptObject Load(string fileName)
        {
            ScriptObject res = new ScriptObject();
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            XmlNode rootNode = doc.SelectSingleNode("/dccscript");
            if(rootNode==null)
                throw new ArgumentException("Wrong start of script. Should use ScriptObject");
            if (GetValue(rootNode, "UseExternal") == "true")
                res.UseExternal = true;
            res.SelectedConfig = ServiceProvider.Settings.DeviceConfigs.Get(GetValue(rootNode, "SelectedConfig"));
            XmlNode commandNode = doc.SelectSingleNode("/dccscript/commands");
            if (commandNode != null)
            {
                foreach (XmlNode node in commandNode.ChildNodes)
                {
                    foreach (var command in AvaiableCommands)
                    {
                        if (command.Name.ToLower() == node.Name.ToLower())
                            res.Commands.Add(command.Load(node));
                    }
                }
            }
            return res;
        }

        public bool Verify(ScriptObject scriptObject)
        {
            if (scriptObject == null)
                return false;
            var res = true;
            if (scriptObject.Commands.Count == 0)
            {
                ServiceProvider.ScriptManager.OutPut("No commands are defined");
                res = false;
            }

            return res;
        }

        public static string GetValue(XmlNode node, string atribute)
        {
            if (node.Attributes != null && node.Attributes[atribute] == null)
                return "";
            return node.Attributes[atribute].Value;
        }

        public static XmlAttribute CreateAttribute(XmlDocument doc, string name, string val)
        {
            XmlAttribute attribute = doc.CreateAttribute(name);
            attribute.Value = val;
            return attribute;
        }

        public void Execute(ScriptObject scriptObject)
        {
            _shouldStop = false;
            IsBusy = true;
            Variabiles.Items.Clear();
            var thread=new Thread(ExecuteThread);
            thread.Start(scriptObject);
        }

        private void ExecuteThread(object o)
        {
            try
            {
                StaticHelper.Instance.SystemMessage = "Script execution started";
                ScriptObject scriptObject = o as ScriptObject;
                foreach (IScriptCommand scriptCommand in scriptObject.Commands)
                {
                    if (_shouldStop)
                        break;
                    scriptCommand.Execute(scriptObject);
                }
                StaticHelper.Instance.SystemMessage = _shouldStop ? "Script execution stopped" : "Script execution done";
            }
            catch (Exception exception)
            {
                Log.Error("Error executing script", exception);
                StaticHelper.Instance.SystemMessage = exception.Message;
            }
            IsBusy = false;
        }

        public void Stop()
        {
            _shouldStop = true;
            StaticHelper.Instance.SystemMessage = "Script execution stopping ....";
        }

        public void OnOutPutMessageReceived(MessageEventArgs e)
        {
            MessageEventHandler handler = OutPutMessageReceived;
            if (handler != null) handler(this, e);
        }

        public void OutPut(string text)
        {
            OnOutPutMessageReceived(new MessageEventArgs(text));
        }

    }
}
