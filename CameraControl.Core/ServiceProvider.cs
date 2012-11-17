using System;
using CameraControl.Core.Classes;
using CameraControl.Core.Devices;

namespace CameraControl.Core
{
  public class ServiceProvider
  {
    public static string AppName = "digiCamControl";

    public static Settings Settings { get; set; }
    public static readonly log4net.ILog log = log4net.LogManager.GetLogger("digiCamControl");
    public static CameraDeviceManager DeviceManager { get; set; }
    public static TriggerClass Trigger { get; set; }
    public static WindowsManager WindowsManager { get; set; }
    public static ActionManager ActionManager { get; set; }
    public static QueueManager QueueManager { get; set; }
    public static PluginManager PluginManager { get; set; }

    public static void Configure()
    {
      Log.Configure(AppName); 
      DeviceManager = new CameraDeviceManager();
      Trigger = new TriggerClass();
      ActionManager = new ActionManager();
      QueueManager = new QueueManager();
      Log.Debug("Application starting");
      Log.Debug("Application verison :" + System.Reflection.Assembly.GetEntryAssembly().GetName().Version);
      PluginManager = new PluginManager();
    }
  }
}
