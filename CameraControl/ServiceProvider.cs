using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Classes;

namespace CameraControl
{
  public class ServiceProvider
  {
    public static Settings Settings { get; set; }
    public static ThumbWorker ThumbWorker { get; set; }
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public static void Configure()
    {
      bool isConfigured = log.Logger.Repository.Configured;
      if (!isConfigured)
      {
        // Setup RollingFileAppender
        log4net.Appender.RollingFileAppender fileAppender = new log4net.Appender.RollingFileAppender();
        fileAppender.Layout = new log4net.Layout.PatternLayout("%d [%t]%-5p %c [%x] - %m%n");
        fileAppender.MaximumFileSize = "100KB";
        fileAppender.MaxSizeRollBackups = 5;
        fileAppender.RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Size;
        fileAppender.AppendToFile = true;
        fileAppender.File = "D:\\test.log";
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
      log.Debug("Test");
    }
  }
}
