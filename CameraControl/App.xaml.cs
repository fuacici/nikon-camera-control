using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using CameraControl.Actions;
using CameraControl.Actions.Enfuse;
using CameraControl.Classes;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;
using CameraControl.windows;

namespace CameraControl
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App
  {
    private void Application_Exit(object sender, ExitEventArgs e)
    {
      ServiceProvider.Settings.Save(ServiceProvider.Settings.DefaultSession);
      ServiceProvider.Settings.Save();
      if (ServiceProvider.Trigger != null)
      {
        ServiceProvider.Trigger.Stop();
      }
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
      // Global exception handling  
      Current.DispatcherUnhandledException += AppDispatcherUnhandledException;
      //check if wia 2.0 is registered 
      try
      {
        Type t = Type.GetTypeFromCLSID(new Guid("{E1C5D730-7E97-4D8A-9E42-BBAE87C2059F}"), true);
      }
      catch (COMException)
      {
        System.Windows.Forms.MessageBox.Show("Wia 2.0 not installed");
      }
      InitApplication();
    }

    private void InitApplication()
    {
      ServiceProvider.Configure();
      ServiceProvider.ActionManager.Actions = new AsyncObservableCollection<IMenuAction>
                                                {
                                                  new CmdFocusStackingCombineZP(),
                                                  new CmdEnfuse(),
                                                  new CmdToJpg(),
                                                  new CmdExpJpg()
                                                };
      ServiceProvider.Settings = new Settings();
      ServiceProvider.Settings = ServiceProvider.Settings.Load();
      ServiceProvider.Settings.LoadSessionData();

      ServiceProvider.WindowsManager = new WindowsManager();
      ServiceProvider.WindowsManager.Add(new FullScreenWnd());
      ServiceProvider.WindowsManager.Add(new LiveViewWnd());
      ServiceProvider.WindowsManager.Add(new MultipleCameraWnd());
      ServiceProvider.WindowsManager.Add(new CameraPropertyWnd());
      ServiceProvider.WindowsManager.Add(new BrowseWnd());
      ServiceProvider.WindowsManager.Add(new TagSelectorWnd());
      ServiceProvider.WindowsManager.Event += WindowsManager_Event;

      ServiceProvider.Trigger.Start();
    }

    void WindowsManager_Event(string cmd, object o)
    {
      Log.Debug("Window command received :" + cmd);
    }


    private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
//#if DEBUG
//      // In debug mode do not custom-handle the exception, let Visual Studio handle it

//      e.Handled = false;

//#else

//          ShowUnhandeledException(e);    

//#endif
      ShowUnhandeledException(e);
    }

    private void ShowUnhandeledException(DispatcherUnhandledExceptionEventArgs e)
    {
      e.Handled = true;

      Log.Error("Unhandled error ", e.Exception);

      string errorMessage =
        string.Format(
          "An application error occurred.\nPlease check whether your data is correct and repeat the action. If this error occurs again there seems to be a more serious malfunction in the application, and you better close it.\n\nError:{0}\n\nDo you want to continue?\n(if you click Yes you will continue with your work, if you click No the application will close)",

          e.Exception.Message + (e.Exception.InnerException != null
                                   ? "\n" +
                                     e.Exception.InnerException.Message
                                   : null));
      // check if wia 2.0 is registered 
      // isn't a clean way
      if (errorMessage.Contains("{E1C5D730-7E97-4D8A-9E42-BBAE87C2059F}"))
      {
        System.Windows.Forms.MessageBox.Show("Wia 2.0 not installed");
        PhotoUtils.RunAndWait("regwia.bat", "");
        System.Windows.Forms.MessageBox.Show("Restart the application !");
        Application.Current.Shutdown();
      }
      else
      {
        if (MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.YesNo, MessageBoxImage.Error) ==
            MessageBoxResult.No)
        {
          Application.Current.Shutdown();
        }
      }
    }
  }
}
