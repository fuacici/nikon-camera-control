using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CameraControl.Classes;
using CameraControl.Core.Classes;
using CameraControl.Devices;
using CameraControl.Devices.Others;

namespace CameraControl
{
  public class ServiceProvider
  {
    public static string AppName = "NikonCameraControl";

    public static Settings Settings { get; set; }
    public static readonly log4net.ILog Log = log4net.LogManager.GetLogger("NCC");
    public static CameraDeviceManager DeviceManager { get; set; }
    public static TriggerClass Trigger { get; set; }
    public static WindowsManager WindowsManager { get; set; }
    public static ActionManager ActionManager { get; set; }
    public static QueueManager QueueManager { get; set; }

    public static void Configure()
    {
      bool isConfigured = Log.Logger.Repository.Configured;
      if (!isConfigured)
      {
        // Setup RollingFileAppender
        log4net.Appender.RollingFileAppender fileAppender = new log4net.Appender.RollingFileAppender();
        fileAppender.Layout = new log4net.Layout.PatternLayout("%d [%t]%-5p %c [%x] - %m%n");
        fileAppender.MaximumFileSize = "1000KB";
        fileAppender.MaxSizeRollBackups = 5;
        fileAppender.RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Size;
        fileAppender.AppendToFile = true;
        fileAppender.File = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                         AppName, "Log",
                                         "app.log");
        fileAppender.Name = "XXXRollingFileAppender";
        fileAppender.ActivateOptions(); // IMPORTANT, creates the file
        log4net.Config.BasicConfigurator.Configure(fileAppender);
#if DEBUG
        // Setup TraceAppender
        log4net.Appender.TraceAppender ta = new log4net.Appender.TraceAppender(
          new log4net.Layout.PatternLayout("%d [%t]%-5p %c [%x] - %m%n"));
        log4net.Config.BasicConfigurator.Configure(ta);
#endif
      }
      DeviceManager = new CameraDeviceManager();
      Trigger = new TriggerClass();
      ActionManager = new ActionManager();
      QueueManager = new QueueManager();
      Log.Debug("Application starting");
      Log.Debug("Application verison" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
    }
  }
}
