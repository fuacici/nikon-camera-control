using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
