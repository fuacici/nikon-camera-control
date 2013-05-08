using System.Windows.Controls;
using CameraControl.Core.Classes;

namespace CameraControl.Core.Interfaces
{
    public interface IExternalShutterReleaseSource
    {
        string Name { get; set; }
        bool Execute(CustomConfig config);
        bool CanExecute(CustomConfig config);
        UserControl GetConfig(CustomConfig config);
    }
}