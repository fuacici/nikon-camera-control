using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using CameraControl.Devices.Classes;

namespace CameraControl.Core.Scripting.ScriptCommands
{
    public class Loop:BaseScript
    {
        
        private AsyncObservableCollection<IScriptCommand> _commands;
        public AsyncObservableCollection<IScriptCommand> Commands
        {
            get { return _commands; }
            set
            {
                _commands = value;
                NotifyPropertyChanged("Commands");
            }
        }

        public override IScriptCommand Load(System.Xml.XmlNode loadnode)
        {
            base.Load(loadnode);
            foreach (XmlNode node in loadnode.ChildNodes)
            {
                foreach (var command in ServiceProvider.ScriptManager.AvaiableCommands)
                {
                    if (command.Name.ToLower() == node.Name.ToLower())
                        Commands.Add(((IScriptCommand)Activator.CreateInstance(command.GetType())).Load(node));
                }
            }
            return this;
        }

        public override bool Execute(ScriptObject scriptObject)
        {
            int loopcount = 0;
            int.TryParse(scriptObject.ParseString(this.LoadedParams["loopcount"]), out loopcount);
            for (int i = 0; i < loopcount; i++)
            {
                scriptObject.Variabiles["loopcount"] = i.ToString();
                if (ServiceProvider.ScriptManager.ShouldStop)
                    break;
                foreach (IScriptCommand command in Commands)
                {
                    if (ServiceProvider.ScriptManager.ShouldStop)
                        break;
                    command.Execute(scriptObject);
                }
            }
            return true;
        }

        public Loop()
        {
            Name = "loop";
            Description = "execute a subset of commands for loopcount";
            DefaultValue = "loop loopcount=\"10\"";
            Commands = new AsyncObservableCollection<IScriptCommand>();
        }
    }
}
