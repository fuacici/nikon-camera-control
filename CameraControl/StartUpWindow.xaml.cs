using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CameraControl.Actions;
using CameraControl.Actions.Enfuse;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using CameraControl.Translation;
using CameraControl.windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace CameraControl
{
  /// <summary>
  /// Interaction logic for StartUpWindow.xaml
  /// </summary>
  public partial class StartUpWindow : Window
  {
    public StartUpWindow()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      InitApplication();
      //Thread thread = new Thread(InitApplication);
      //thread.SetApartmentState(ApartmentState.STA);
      //thread.Start();
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
      if (ServiceProvider.Settings.DisableNativeDrivers && MessageBox.Show(TranslationStrings.MsgDisabledDrivers, "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        ServiceProvider.Settings.DisableNativeDrivers = false;
      ServiceProvider.Settings.LoadSessionData();
      TranslationManager.LoadLanguage(ServiceProvider.Settings.SelectedLanguage);
      ServiceProvider.WindowsManager = new WindowsManager();
      ServiceProvider.WindowsManager.Add(new FullScreenWnd());
      ServiceProvider.WindowsManager.Add(new LiveViewWnd());
      ServiceProvider.WindowsManager.Add(new MultipleCameraWnd());
      ServiceProvider.WindowsManager.Add(new CameraPropertyWnd());
      ServiceProvider.WindowsManager.Add(new BrowseWnd());
      ServiceProvider.WindowsManager.Add(new TagSelectorWnd());
      ServiceProvider.WindowsManager.Add(new DownloadPhotosWnd());
      ServiceProvider.WindowsManager.Event += WindowsManager_Event;
      ServiceProvider.Trigger.Start();
      ServiceProvider.PluginManager.LoadPlugins(System.IO.Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Plugins"));
      StartApplication();
      Dispatcher.Invoke(new Action(Hide));
    }

    private void StartApplication()
    {
      string procName = Process.GetCurrentProcess().ProcessName;
      // get the list of all processes by that name

      Process[] processes = Process.GetProcessesByName(procName);

      if (processes.Length > 1)
      {
        MessageBox.Show("Application already running");
        ServiceProvider.WindowsManager.ExecuteCommand(CmdConsts.All_Close);
        return;
      }
      else
      {
        MainWindow mainView = new MainWindow();
        mainView.Show();

      }
    }

    void WindowsManager_Event(string cmd, object o)
    {
      Log.Debug("Window command received :" + cmd);
      if (cmd == CmdConsts.All_Close)
      {
        ServiceProvider.DeviceManager.CloseAll();
        Thread.Sleep(1000);
        Application.Current.Shutdown();
      }
    }
  }
}
