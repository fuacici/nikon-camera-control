using System;
using System.IO;
using System.Reflection;
using CameraControl.Core.Classes;
using CameraControl.Core.Devices;
using CameraControl.Devices;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;

namespace CameraControl.Core
{
  public class ServiceProvider
  {
    private static readonly ILog _log = LogManager.GetLogger("DCC");

    public static string AppName = "digiCamControl";


    public static Settings Settings { get; set; }
    public static readonly ILog log = LogManager.GetLogger("digiCamControl");
    public static CameraDeviceManager DeviceManager { get; set; }
    public static TriggerClass Trigger { get; set; }
    public static WindowsManager WindowsManager { get; set; }
    public static ActionManager ActionManager { get; set; }
    public static QueueManager QueueManager { get; set; }
    public static PluginManager PluginManager { get; set; }

    public static void Configure()
    {
      Configure(AppName);
      Log.LogDebug += Log_LogDebug;
      Log.LogError += Log_LogError;
      DeviceManager = new CameraDeviceManager();
      Trigger = new TriggerClass();
      ActionManager = new ActionManager();
      QueueManager = new QueueManager();
      Log.Debug("Application starting");
      Log.Debug("Application verison :" + Assembly.GetEntryAssembly().GetName().Version);
      PluginManager = new PluginManager();
    }

    static void Log_LogError(CameraControl.Devices.Classes.LogEventArgs e)
    {
      _log.Error(e.Message, e.Exception);
    }

    static void Log_LogDebug(CameraControl.Devices.Classes.LogEventArgs e)
    {
      _log.Debug(e.Message, e.Exception);
    }

    public static void Configure(string appfolder)
    {
      bool isConfigured = _log.Logger.Repository.Configured;
      if (!isConfigured)
      {
        // Setup RollingFileAppender
        log4net.Appender.RollingFileAppender fileAppender = new log4net.Appender.RollingFileAppender();
        fileAppender.Layout = new log4net.Layout.PatternLayout("%d [%t]%-5p %c [%x] - %m%n");
        fileAppender.MaximumFileSize = "1000KB";
        fileAppender.MaxSizeRollBackups = 5;
        fileAppender.RollingStyle = RollingFileAppender.RollingMode.Size;
        fileAppender.AppendToFile = true;
        fileAppender.File = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                         appfolder, "Log",
                                         "app.log");
        fileAppender.Name = "XXXRollingFileAppender";
        fileAppender.ActivateOptions(); // IMPORTANT, creates the file
        BasicConfigurator.Configure(fileAppender);
#if DEBUG
        // Setup TraceAppender
        log4net.Appender.TraceAppender ta = new log4net.Appender.TraceAppender(
          new log4net.Layout.PatternLayout("%d [%t]%-5p %c [%x] - %m%n"));
        BasicConfigurator.Configure(ta);
#endif

      }
    }
  }
}
