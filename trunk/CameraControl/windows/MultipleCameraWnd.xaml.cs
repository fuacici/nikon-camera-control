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
using CameraControl.Core;
using CameraControl.Core.Devices;
using CameraControl.Core.Devices.Classes;
using CameraControl.Core.Interfaces;

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
     StaticHelper.Instance.SystemMessage = string.Format("Waiting {0})", _secounter);
    }

    #region Implementation of IWindow

    public void ExecuteCommand(string cmd, object param)
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
     StaticHelper.Instance.SystemMessage = string.Format("Capture started {0}", _photocounter);
      Thread thread = new Thread(new ThreadStart(delegate
      {
        try
        {
          foreach (ICameraDevice connectedDevice in ServiceProvider.DeviceManager.ConnectedDevices.Where(connectedDevice => connectedDevice.IsConnected && connectedDevice.IsChecked))
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
              Log.Error(exception);
              throw;
            }
          }
        }
        catch (Exception exception)
        {
          Log.Error(exception);
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
     StaticHelper.Instance.SystemMessage = "All captures done !";
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
      if(listBox1.SelectedItem!=null)
        ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.CameraPropertyWnd_Show,
                                                    listBox1.SelectedItem);
    }

    private void MenuItem_Click_1(object sender, RoutedEventArgs e)
    {
      if(listBox1.SelectedItem!=null)
      {
        CameraPreset preset=new CameraPreset();
        preset.Get((ICameraDevice)listBox1.SelectedItem);
        foreach (ICameraDevice connectedDevice in ServiceProvider.DeviceManager.ConnectedDevices)
        {
          if (connectedDevice.IsConnected && connectedDevice.IsChecked)
            preset.Set(connectedDevice); 
        }
      }
    }

  }
}
