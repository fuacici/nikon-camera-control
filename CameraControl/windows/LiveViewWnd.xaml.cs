using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
//using System.Threading;
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
using CameraControl.Devices.Classes;
using CameraControl.Devices.Nikon;
using CameraControl.Interfaces;
using PortableDeviceLib;
using MessageBox = System.Windows.Forms.MessageBox;
using Timer = System.Timers.Timer;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for LiveViewWnd.xaml
  /// </summary>
  public partial class LiveViewWnd : Window, IWindow, INotifyPropertyChanged
  {
    private const int Step1 = 10;
    private const int Step2 = 100;
    private const int Step3 = 500;

    private int _retries = 0;
    private ICameraDevice selectedPortableDevice;
    private Rectangle _focusrect=new Rectangle(); 
    Line _line11=new Line();
    Line _line12 = new Line();
    Line _line21 = new Line();
    Line _line22 = new Line();
    private BackgroundWorker _worker = new BackgroundWorker();
    

    public LiveViewData LiveViewData { get; set; }

    private string _counterMessage;
    public string CounterMessage
    {
      get
      {
        if (!LockA && !LockB)
          return "?";
        if (LockA && !LockB)
          return FocusCounter.ToString();
        if (LockA && LockB)
          return FocusCounter + "/" + FocusValue;
        return _counterMessage;
      }
      set
      {
        _counterMessage = value;
        NotifyPropertyChanged("CounterMessage");
      }
    }

    private int _focusCounter;
    public int FocusCounter
    {
      get { return _focusCounter; }
      set
      {
        _focusCounter = value;
        NotifyPropertyChanged("FocusCounter");
        NotifyPropertyChanged("CounterMessage");
      }
    }

    private int _focusValue;
    public int FocusValue
    {
      get { return _focusValue; }
      set
      {
        _focusValue = value;
        NotifyPropertyChanged("FocusValue");
        NotifyPropertyChanged("CounterMessage");
      }
    }

    private bool _lockA;
    public bool LockA
    {
      get { return _lockA; }
      set
      {
        _lockA = value;
        if (_lockA)
        {
          FocusCounter = 0;
          FocusValue = 0;
          LockB = false;
        }
        NotifyPropertyChanged("LockA");
        NotifyPropertyChanged("CounterMessage");
      }
    }

    private bool _lockB;
    public bool LockB
    {
      get { return _lockB; }
      set
      {
        _lockB = value;
        if (_lockB)
          FocusValue = FocusCounter;
        NotifyPropertyChanged("LockB");
        NotifyPropertyChanged("CounterMessage");
      }
    }

    private Timer _timer = new Timer(1000/20);
    private bool oper_in_progress = false;
    /// <summary>
    /// Gets the <see cref="PortableDevice"/> connected
    /// </summary>
    public ObservableCollection<PortableDevice> PortableDevices
    {
      get;
      private set;
    }

    public ICameraDevice SelectedPortableDevice
    {
      get { return this.selectedPortableDevice; }
      set
      {
        if (this.selectedPortableDevice != value)
        {
          this.selectedPortableDevice = value;
        }
      }
    }

    public LiveViewWnd()
    {
      SelectedPortableDevice = ServiceProvider.DeviceManager.SelectedCameraDevice;
      Init();
      LockA = false;
    }

    public LiveViewWnd(ICameraDevice device)
    {
      SelectedPortableDevice = device;
      Init();
    }

    public void Init()
    {
      InitializeComponent();
      _timer.Stop();
      _timer.AutoReset = true;
      _timer.Elapsed += _timer_Elapsed;
      _focusrect.Stroke = new SolidColorBrush(Colors.Green);
      canvas.Children.Add(_focusrect);
      _line11.Stroke = new SolidColorBrush(Colors.White);
      _line12.Stroke = new SolidColorBrush(Colors.White);
      _line21.Stroke = new SolidColorBrush(Colors.White);
      _line22.Stroke = new SolidColorBrush(Colors.White);
      canvas.Children.Add(_line11);
      canvas.Children.Add(_line12);
      canvas.Children.Add(_line21);
      canvas.Children.Add(_line22);
      _worker.DoWork += delegate { GetLiveImage(); };
    }

    void Manager_PhotoTaked(WIA.Item imageFile)
    {
      if (!IsVisible)
        return;
      Thread thread = new Thread(new ThreadStart(delegate
                                                   {
                                                     //Thread.Sleep(200);
                                                     StartLiveView();
                                                     _timer.Start();
                                                   }));
      thread.Start();
    }

    void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      if (_retries > 100)
      {
        _timer.Stop();

        Dispatcher.Invoke(new ThreadStart(delegate
                                                 {
                                                   image1.Visibility = Visibility.Hidden;
                                                   chk_grid.IsChecked = false;
                                                 }));
        return;
      }
      if(!_worker.IsBusy)
        _worker.RunWorkerAsync();
    }

    private void GetLiveImage()
    {
      if(oper_in_progress)
        return;
      oper_in_progress = true;
      try
      {
        LiveViewData = SelectedPortableDevice.GetLiveViewImage();
      }
      catch (Exception)
      {
        _retries++;
        oper_in_progress = false;
        return;
      }

      if (LiveViewData == null || LiveViewData.ImageData == null)
      {
        _retries++;
        oper_in_progress = false;
        return;
      }

      try
      {
        Dispatcher.Invoke(new Action(delegate
                                            {
                                              if (LiveViewData != null && LiveViewData.ImageData != null)
                                              {

                                                MemoryStream stream = new MemoryStream(LiveViewData.ImageData, 0,
                                                                                       LiveViewData.ImageData.Length);

                                                JpegBitmapDecoder decoder = new JpegBitmapDecoder(stream,
                                                                                                  BitmapCreateOptions.
                                                                                                    None,
                                                                                                  BitmapCacheOption.
                                                                                                    OnLoad);

                                                decoder.Frames[0].Freeze();

                                                if (decoder.Frames.Count > 0)
                                                {
                                                  image1.Source = decoder.Frames[0];
                                                }
                                                stream.Close();
                                              }
                                            }));
      }
      catch (Exception)
      {
        _retries++;
        oper_in_progress = false;
        return;
      }
      Dispatcher.Invoke(new Action(delegate { DrawLines(); ; }));
      _retries = 0;
      oper_in_progress = false;
    }

    void DrawLines()
    {
      if (LiveViewData == null)
        return;
      _focusrect.BeginInit();
      _focusrect.Visibility = LiveViewData.HaveFocusData ? Visibility.Visible : Visibility.Hidden;
      double xt = image1.ActualWidth / LiveViewData.ImageWidth;
      double yt = image1.ActualHeight / LiveViewData.ImageHeight;
      _focusrect.Height = LiveViewData.FocusFrameXSize * xt;
      _focusrect.Width = LiveViewData.FocusFrameYSize * yt;
      double xx = (canvas.ActualWidth - image1.ActualWidth) / 2;
      double yy = (canvas.ActualHeight - image1.ActualHeight) / 2;
      SetLinePos(_line11, (int)(xx + image1.ActualWidth / 3), (int)yy, (int)(xx + image1.ActualWidth / 3), (int)(yy + image1.ActualHeight));
      SetLinePos(_line12, (int)(xx + (image1.ActualWidth / 3) * 2), (int)yy, (int)(xx + (image1.ActualWidth / 3) * 2), (int)(yy + image1.ActualHeight));

      SetLinePos(_line21, (int)xx, (int) (yy+ (image1.ActualHeight/3)), (int) (xx + image1.ActualWidth),(int) (yy + image1.ActualHeight/3));
      SetLinePos(_line22, (int)xx, (int)(yy+(image1.ActualHeight / 3)*2), (int)(xx + image1.ActualWidth), (int)(yy + (image1.ActualHeight / 3)*2));

      _focusrect.SetValue(Canvas.LeftProperty, LiveViewData.FocusX * xt - (_focusrect.Height / 2) + xx);
      _focusrect.SetValue(Canvas.TopProperty, LiveViewData.FocusY * yt - (_focusrect.Width / 2) + yy);
      _focusrect.Stroke = new SolidColorBrush(LiveViewData.Focused ? Colors.Green : Colors.Red);
      _focusrect.EndInit();
    }

    private void SetLinePos(Line line,int x1, int y1, int x2, int y2)
    {
      line.X1 = x1;
      line.X2 = x2;
      line.Y1 = y1;
      line.Y2 = y2;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      //SelectedPortableDevice.StoptLiveView();
    }



    private void Window_Closed(object sender, EventArgs e)
    {

    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      _timer.Stop();
      Thread.Sleep(100);
      try
      {
        selectedPortableDevice.AutoFocus();
      }
      catch (Exception exception)
      {
        ServiceProvider.Log.Error("Unable to autofocus", exception);
      }
      FocusCounter = 0;
      _timer.Start();
    }

    private void button2_Click(object sender, RoutedEventArgs e)
    {
      _timer.Stop();
      Thread.Sleep(100);
      try
      {
        selectedPortableDevice.TakePictureNoAf();
      }
      catch (Exception exception)
      {
        ServiceProvider.Log.Error("Unable to take pictore with no af", exception);
      }
      //_timer.Start();
    }

    private void StartLiveView()
    {
      try
      {
        SelectedPortableDevice.StartLiveView();
        oper_in_progress = false;
        _retries = 0;
      }
      catch (Exception exception)
      {
        ServiceProvider.Log.Error("Unable to start liveview !", exception);
        MessageBox.Show("Unable to start liveview !");
        ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.LiveViewWnd_Hide);
      }
      Dispatcher.Invoke(new Action(delegate { image1.Visibility = Visibility.Visible; }));
    }

    private void image1_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left && LiveViewData != null &&
          LiveViewData.HaveFocusData)
      {
        Point _initialPoint = e.MouseDevice.GetPosition(image1);
        double xt = LiveViewData.ImageWidth/image1.ActualWidth;
        double yt = LiveViewData.ImageHeight/image1.ActualHeight;
        int posx = (int) (_initialPoint.X*xt);
        int posy = (int) (_initialPoint.Y*yt);
        selectedPortableDevice.Focus(posx, posy);
      }
    }

    private void btn_focusm_Click(object sender, RoutedEventArgs e)
    {
      SetFocus(-Step1);
    }

    private void btn_focusp_Click(object sender, RoutedEventArgs e)
    {
      SetFocus(Step1);
    }

    private void btn_focusmm_Click(object sender, RoutedEventArgs e)
    {
      SetFocus(-Step2);
    }

    private void btn_focuspp_Click(object sender, RoutedEventArgs e)
    {
      SetFocus(Step2);
    }

    private void btn_focusmmm_Click(object sender, RoutedEventArgs e)
    {
      SetFocus(-Step3);
    }

    private void btn_focusppp_Click(object sender, RoutedEventArgs e)
    {
      SetFocus(Step3);
    }

    private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      DrawLines();
    }

    private void chk_grid_Checked(object sender, RoutedEventArgs e)
    {
      _line11.Visibility = Visibility.Visible;
      _line12.Visibility = Visibility.Visible;
      _line21.Visibility = Visibility.Visible;
      _line22.Visibility = Visibility.Visible;
    }

    private void chk_grid_Unchecked(object sender, RoutedEventArgs e)
    {
      _line11.Visibility = Visibility.Hidden;
      _line12.Visibility = Visibility.Hidden;
      _line21.Visibility = Visibility.Hidden;
      _line22.Visibility = Visibility.Hidden;
    }

    #region Implementation of IWindow

    public void ExecuteCommand(string cmd)
    {
      switch (cmd)
      {
        case WindowsCmdConsts.LiveViewWnd_Show:
          Dispatcher.Invoke(new Action(delegate
                                              {
                                                SelectedPortableDevice = ServiceProvider.DeviceManager.SelectedCameraDevice;
                                                Show();
                                                Activate();
                                                Topmost = true;
                                                Topmost = false;
                                                Focus();
                                                ServiceProvider.Settings.Manager.PhotoTakenDone += Manager_PhotoTaked;
                                                StartLiveView();
                                                Thread.Sleep(500);
                                                _timer.Start();
                                              }));
          break;
        case WindowsCmdConsts.LiveViewWnd_Hide:
          Dispatcher.Invoke(new Action(delegate
                                         {
                                           Hide();
                                           try
                                           {
                                             _timer.Stop();
                                             ServiceProvider.Settings.Manager.PhotoTakenDone -= Manager_PhotoTaked;
                                             Thread.Sleep(100);
                                             SelectedPortableDevice.StopLiveView();
                                           }
                                           catch (Exception exception)
                                           {
                                             ServiceProvider.Log.Error("Unable to stop liveview", exception);
                                           }
                                           ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.FocusStackingWnd_Hide);
                                         }));
          break;
        case WindowsCmdConsts.All_Close:
          Dispatcher.Invoke(new Action(delegate
                                         {
                                           Hide();
                                           Close();
                                         }));
          break;
        case WindowsCmdConsts.LiveView_Zoom_All:
          SelectedPortableDevice.LiveViewImageZoomRatio = 0;
          break;
        case WindowsCmdConsts.LiveView_Zoom_25:
          SelectedPortableDevice.LiveViewImageZoomRatio = 1;
          break;
        case WindowsCmdConsts.LiveView_Zoom_33:
          SelectedPortableDevice.LiveViewImageZoomRatio = 2;
          break;
        case WindowsCmdConsts.LiveView_Zoom_50:
          SelectedPortableDevice.LiveViewImageZoomRatio = 3;
          break;
        case WindowsCmdConsts.LiveView_Zoom_66:
          SelectedPortableDevice.LiveViewImageZoomRatio = 4;
          break;
        case WindowsCmdConsts.LiveView_Zoom_100:
          SelectedPortableDevice.LiveViewImageZoomRatio = 5;
          break;
        case WindowsCmdConsts.LiveView_Focus_M:
          btn_focusm_Click(null, null);
          break;
        case WindowsCmdConsts.LiveView_Focus_P:
          btn_focusp_Click(null, null);
          break;
        case WindowsCmdConsts.LiveView_Focus_MM:
          btn_focusmm_Click(null, null);
          break;
        case WindowsCmdConsts.LiveView_Focus_PP:
          btn_focuspp_Click(null, null);
          break;
        case WindowsCmdConsts.LiveView_Focus_MMM:
          btn_focusmm_Click(null, null);
          break;
        case WindowsCmdConsts.LiveView_Focus_PPP:
          btn_focuspp_Click(null, null);
          break;
        case WindowsCmdConsts.LiveView_Focus:
          button1_Click(null, null);
          break;
      }
    }

    #endregion

    private void SetFocus(int step)
    {
      if (LockA)
      {
        if (FocusCounter + step < 0)
          step = -FocusCounter;
      }
      if (LockB)
      {
        if (FocusCounter + step > FocusValue)
          step = FocusValue - FocusCounter;
      }
      selectedPortableDevice.Focus(step);
      FocusCounter += step;
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      if (IsVisible)
      {
        e.Cancel = true;
        ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.LiveViewWnd_Hide);
      }
    }

    private void button3_Click(object sender, RoutedEventArgs e)
    {
      FocusStackingWnd wnd = (FocusStackingWnd) ServiceProvider.WindowsManager.Get(typeof (FocusStackingWnd));
      wnd.FocusCounter = FocusCounter;
      wnd.FocusValue = FocusValue;
      ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.FocusStackingWnd_Show);
    }

    #region Implementation of INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;
    public virtual void NotifyPropertyChanged(String info)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
    }
    #endregion

    private void button4_Click(object sender, RoutedEventArgs e)
    {
      FocusCounter = 0;
    }

    private void btn_movea_Click(object sender, RoutedEventArgs e)
    {
      SetFocus(-FocusCounter);
    }

    private void btn_moveb_Click(object sender, RoutedEventArgs e)
    {
      SetFocus(FocusValue - FocusCounter);
    }
  }
}
