using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using CameraControl.Core.Scripting.ScriptCommands;

namespace CameraControl.Core.Scripting
{
    public class ScriptManager
    {
        public List<IScriptCommand> AvaiableCommands { get; set; }

        public ScriptManager()
        {
            AvaiableCommands=new List<IScriptCommand> {new BulbCapture()};
        }

        public void Save(ScriptObject scriptObject,string fileName)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);
            XmlNode rootNode = doc.CreateElement("ScriptObject");
            doc.AppendChild(rootNode);
            XmlNode commandsNode = doc.CreateElement("Commands");
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
            XmlNode commandNode = doc.SelectSingleNode("/ScriptObject/Commands");
            foreach (XmlNode node in commandNode.ChildNodes)
            {
                foreach (var command in AvaiableCommands)
                {
                    if(command.Name==node.Name)
                        res.Commands.Add(command.Load(node));
                }
            }
            return res;
        }
    }
}
