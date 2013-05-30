using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace CameraControl.Core.Scripting.ScriptCommands
{
    public class PHDGuiding:BaseScript
    {
        public override IScriptCommand Create()
        {
            return new PHDGuiding();
        }

        public override XmlNode Save(XmlDocument doc)
        {
            XmlNode nameNode = doc.CreateElement("PHDGuiding");
            //nameNode.Attributes.Append(ScriptManager.CreateAttribute(doc, "CaptureTime", CaptureTime.ToString()));
            //nameNode.Attributes.Append(ScriptManager.CreateAttribute(doc, "Iso", Iso));
            //nameNode.Attributes.Append(ScriptManager.CreateAttribute(doc, "Aperture", Aperture));
            return nameNode;
        }

        public override IScriptCommand Load(XmlNode node)
        {
            BulbCapture res = new BulbCapture
            {
                //CaptureTime = Convert.ToInt32(ScriptManager.GetValue(node, "CaptureTime")),
                //Iso = ScriptManager.GetValue(node, "Iso"),
                //Aperture = ScriptManager.GetValue(node, "Aperture")
            };
            return res;
        }
    }
}
