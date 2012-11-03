using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CameraControl.Core;
using CameraControl.Core.Devices;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for BulbWnd.xaml
  /// </summary>
  public partial class BulbWnd : INotifyPropertyChanged
  {
    private Timer _captureTimer = new Timer(1000);
    private Timer _waitTimer = new Timer(1000);
    private int _captureSecs;
    private int _waitSecs;
    private int _photoCount = 0;

    public ICameraDevice CameraDevice { get; set; }
    private bool _noAutofocus;
    public bool NoAutofocus
    {
      get { return _noAutofocus; }
      set
      {
        _noAutofocus = value;
        NotifyPropertyChanged("NoAutofocus");
      }
    }

    private int _captureTime;
    public int CaptureTime
    {
      get { return _captureTime; }
      set
      {
        _captureTime = value;
        NotifyPropertyChanged("CaptureTime");
      }
    }


    private int _numOfPhotos;
    public int NumOfPhotos
    {
      get { return _numOfPhotos; }
      set
      {
        _numOfPhotos = value;
        NotifyPropertyChanged("NumOfPhotos");
      }
    }

    private int _waitTime;
    public int WaitTime
    {
      get { return _waitTime; }
      set
      {
        _waitTime = value;
        NotifyPropertyChanged("WaitTime");
      }
    }


    public BulbWnd()
    {
      InitializeComponent();
      CameraDevice = ServiceProvider.DeviceManager.SelectedCameraDevice;
      NoAutofocus = true;
      CaptureTime = 60;
      NumOfPhotos = 1;
      WaitTime = 0;
      _captureTimer.Elapsed += _captureTimer_Elapsed;
      _waitTimer.Elapsed += _waitTimer_Elapsed;
      ServiceProvider.Settings.ApplyTheme(this);
    }

    void _waitTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      _waitSecs++;
     StaticHelper.Instance.SystemMessage = string.Format("Waiting for next capture {0} sec. Photo done {1}",
                                                             _waitSecs, _photoCount);
      if (_waitSecs >= WaitTime)
      {
        _waitTimer.Stop();
        StartCapture();
      }
    }

    void _captureTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
      _captureSecs ++;
     StaticHelper.Instance.SystemMessage = string.Format("Capture time {0} sec", _captureSecs);
      if (_captureSecs > CaptureTime)
      {
        _captureTimer.Stop();
        StopCapture();
        _photoCount++;
        _waitSecs = 0;
        if (_photoCount < NumOfPhotos)
        {
          _waitTimer.Start();
        }
      }
    }

    private void btn_start_Click(object sender, RoutedEventArgs e)
    {
      _photoCount = 0;
      StartCapture();
    }

    void StartCapture()
    {
      try
      {
        Log.Debug("Bulb capture started");
        CameraDevice.LockCamera();
        //if (NoAutofocus)
        //{
        //  CameraDevice.CapturePhotoNoAf();
        //}
        //else
        //{
        //  CameraDevice.CapturePhoto();
        //}
        CameraDevice.StartBulbMode();
      }
      catch (Exception exception)
      {
       StaticHelper.Instance.SystemMessage = exception.Message;
        Log.Error("Bulb start", exception);
      }
      _waitSecs = 0;
      _captureSecs = 0;
      _captureTimer.Start();
    }

    #region Implementation of INotifyPropertyChanged

    public virtual event PropertyChangedEventHandler PropertyChanged;

    public virtual void NotifyPropertyChanged(String info)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
    }

    #endregion

    private void btn_stop_Click(object sender, RoutedEventArgs e)
    {
      StopCapture();
      _captureTimer.Stop();
      _waitTimer.Stop();
     StaticHelper.Instance.SystemMessage = "Capture stoped";
      Log.Debug("Bulb capture stoped");
    }

    private void StopCapture()
    {
      try
      {
        CameraDevice.EndBulbMode();
       StaticHelper.Instance.SystemMessage = "Capture done";
        Log.Debug("Bulb capture done");
      }
      catch (Exception exception)
      {
       StaticHelper.Instance.SystemMessage = exception.Message;
        Log.Error("Bulb done",exception);
      }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      if (_captureTimer.Enabled)
      {
        StopCapture();
        CameraDevice.UnLockCamera();
      }
      _captureTimer.Stop();
      _waitTimer.Stop();
    }

  }
}
