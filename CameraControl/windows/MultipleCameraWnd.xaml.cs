using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Timers;
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
    public int DelaySec { get; set; }
    public int WaitSec { get; set; }
    public int NumOfPhotos { get; set; }

    private System.Timers.Timer _timer = new System.Timers.Timer(1000);
    private int _secounter = 0;
    private int _photocounter = 0;

    public MultipleCameraWnd()
    {
      InitializeComponent();
      _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
    }

    void _timer_Elapsed(object sender, ElapsedEventArgs e)
    {
      _secounter++;
      if(_secounter>WaitSec)
      {
        WaitSec = 0;
        _timer.Stop();
        CapturePhotos();
      }
      ServiceProvider.Settings.SystemMessage = string.Format("Waiting {0})", _secounter);
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
      _secounter = 0;
      _photocounter = 0;
      _timer.Start();
    }

    private void CapturePhotos()
    {
      _photocounter++;
      ServiceProvider.Settings.SystemMessage = string.Format("Capture started {0}", _photocounter);
      Thread thread = new Thread(new ThreadStart(delegate
      {
        try
        {
          foreach (ICameraDevice connectedDevice in ServiceProvider.DeviceManager.ConnectedDevices)
          {
            try
            {
              if (DisbleAutofocus)
                connectedDevice.CapturePhotoNoAf();
              else
                connectedDevice.CapturePhoto();
              Thread.Sleep(DelaySec);
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
        if (_photocounter < NumOfPhotos)
          _timer.Start();
        else
        {
          StopCapture();
        }
      }));
      thread.Start();
    }

    private void btn_stop_Click(object sender, RoutedEventArgs e)
    {
      _timer.Stop();
      _photocounter = NumOfPhotos;
      StopCapture();
    }

    private void StopCapture()
    {
      ServiceProvider.Settings.SystemMessage = "All captures done !";
    }

  }
}
