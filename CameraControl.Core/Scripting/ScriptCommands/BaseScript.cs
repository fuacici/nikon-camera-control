using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml;
using CameraControl.Devices.Classes;

namespace CameraControl.Core.Scripting.ScriptCommands
{
    public class BaseScript : BaseFieldClass, IScriptCommand
    {
        #region Implementation of IScriptCommand

        public virtual bool Execute(ScriptObject scriptObject)
        {
            return true;
        }

        public virtual IScriptCommand Create()
        {
            return new BaseScript();
        }

        public virtual XmlNode Save(XmlDocument doc)
        {
            return null;
        }

        public virtual IScriptCommand Load(XmlNode node)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsExecuted { get; set; }
        public virtual bool Executing { get; set; }
        public virtual string Name { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual UserControl GetConfig()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
