using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Devices.Classes;

namespace CameraControl.Core.Scripting
{
    public class ScriptObject
    {
        public AsyncObservableCollection<IScriptCommand> Commands { get; set; }

        public ScriptObject()
        {
            Commands = new AsyncObservableCollection<IScriptCommand>();
        }
    }
}
