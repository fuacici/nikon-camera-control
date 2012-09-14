using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CameraControl.Core
{

  public static class Log
  {
    private static readonly log4net.ILog _log = log4net.LogManager.GetLogger("NCC");
    
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
        fileAppender.RollingStyle = log4net.Appender.RollingFileAppender.RollingMode.Size;
        fileAppender.AppendToFile = true;
        fileAppender.File = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                         appfolder, "Log",
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
    }

    public static void Debug(object message, Exception exception)
    {
      _log.Debug(message, exception);
    }

    public static void Debug(object message)
    {
      _log.Debug(message);
    }

    public static void Error(object message, Exception exception)
    {
      _log.Error(message, exception);
    }

    public static void Error(object message)
    {
      _log.Error(message);
    }


  }
}
