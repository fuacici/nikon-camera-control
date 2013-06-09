using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraControl.Core.Scripting.ScriptCommands
{
    public sealed class Set:BaseScript
    {
        public override bool Execute(ScriptObject scriptObject)
        {
            var varName = LoadedParams["variable"];
            if (!string.IsNullOrEmpty(varName))
            {
                var val = scriptObject.ParseString(LoadedParams["value"]);
                int intval = 0;
                int intinc = 0;
                if (int.TryParse(val, out intval) &&
                    int.TryParse(scriptObject.ParseString(LoadedParams["inc"]), out intinc))
                {
                    intval += intinc;
                    val = intval.ToString();
                }
                scriptObject.Variabiles[varName] = val;
            }
            return base.Execute(scriptObject);
        }

        public Set()
        {
            Name = "set";
            Description = "Set value to a variable";
            DefaultValue = "set variable=\"var_name\" value=\"value\" inc=\"0\"";
        }
    }
}
