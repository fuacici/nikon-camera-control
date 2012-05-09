using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace CameraControl
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private void Application_Exit(object sender, ExitEventArgs e)
    {
      if (ServiceProvider.Trigger != null)
      {
        ServiceProvider.Trigger.Stop();
      }
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
      // Global exception handling  
      Application.Current.DispatcherUnhandledException +=
        new DispatcherUnhandledExceptionEventHandler(AppDispatcherUnhandledException);
      
    }

    private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
      //#if DEBUG
      //      // In debug mode do not custom-handle the exception, let Visual Studio handle it

      //      e.Handled = false;

      //#else

      //    ShowUnhandeledException(e);    

      //#endif
      ShowUnhandeledException(e);
    }

    private void ShowUnhandeledException(DispatcherUnhandledExceptionEventArgs e)
    {
      e.Handled = true;

      if (ServiceProvider.Log != null && ServiceProvider.Log.Logger.Repository.Configured)
      {
        ServiceProvider.Log.Error("Unhandled error ", e.Exception);
      }

      string errorMessage =
        string.Format(
          "An application error occurred.\nPlease check whether your data is correct and repeat the action. If this error occurs again there seems to be a more serious malfunction in the application, and you better close it.\n\nError:{0}\n\nDo you want to continue?\n(if you click Yes you will continue with your work, if you click No the application will close)",

          e.Exception.Message + (e.Exception.InnerException != null
                                   ? "\n" +
                                     e.Exception.InnerException.Message
                                   : null));

      if (MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.YesNo, MessageBoxImage.Error) ==
          MessageBoxResult.No)
      {
        Application.Current.Shutdown();
      }
    }
  }
}
