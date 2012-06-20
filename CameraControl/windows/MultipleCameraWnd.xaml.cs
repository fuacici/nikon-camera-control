using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CameraControl.Classes;
using CameraControl.Devices;
using CameraControl.Interfaces;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for MultipleCameraWnd.xaml
  /// </summary>
  public partial class MultipleCameraWnd : Window,IWindow
  {
    public bool DisbleAutofocus { get; set; }
    public int WaitSec { get; set; }
    public MultipleCameraWnd()
    {
      InitializeComponent();
    }

    #region Implementation of IWindow

    public void ExecuteCommand(string cmd)
    {
      switch (cmd)
      {
        case WindowsCmdConsts.MultipleCameraWnd_Show:
          Dispatcher.Invoke(new Action(delegate
          {
            Show();
            Activate();
            Topmost = true;
            Topmost = false;
            Focus();
          }));
          break;
        case WindowsCmdConsts.MultipleCameraWnd_Hide:
          Hide();
          break;
        case WindowsCmdConsts.All_Close:
          Dispatcher.Invoke(new Action(delegate
          {
            Hide();
            Close();
          }));
          break;
      }
    }

    #endregion

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (IsVisible)
      {
        e.Cancel = true;
        ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.MultipleCameraWnd_Hide);
      }
    }

    private void btn_shot_Click(object sender, RoutedEventArgs e)
    {
      Thread thread = new Thread(new ThreadStart(delegate
                                                   {
                                                     try
                                                     {
                                                       foreach (ICameraDevice connectedDevice in ServiceProvider.DeviceManager.ConnectedDevices)
                                                       {
                                                         try
                                                         {
                                                           if(DisbleAutofocus)
                                                             connectedDevice.TakePictureNoAf();
                                                           else
                                                             connectedDevice.TakePicture();
                                                           Thread.Sleep(WaitSec*1000);
                                                         }
                                                         catch (COMException exception)
                                                         {
                                                           ServiceProvider.Log.Error(exception);
                                                           throw;
                                                         }
                                                       }
                                                     }
                                                     catch (Exception exception)
                                                     {
                                                       ServiceProvider.Log.Error(exception);
                                                     }
                                                   }));
      thread.Start();
    }
  }
}
