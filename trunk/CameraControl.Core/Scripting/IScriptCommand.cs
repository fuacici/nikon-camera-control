using System.Windows.Controls;
using System.Xml;
using CameraControl.Core.Classes;

namespace CameraControl.Core.Scripting
{
    public interface IScriptCommand
    {
        bool Execute();
        XmlNode Save();
        void Load(XmlNode node);
        bool IsExecuted { get; set; }
        bool Executing { get; set; }
        string Name { get; set; }
        string DisplayName { get; set; }
        UserControl GetConfig();
    }
}