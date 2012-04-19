using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using CameraControl.Devices.Nikon;
using PortableDeviceLib;
using MessageBox = System.Windows.Forms.MessageBox;
using Timer = System.Timers.Timer;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for LiveViewWnd.xaml
  /// </summary>
  public partial class LiveViewWnd : Window
  {
    private int _retries = 0;
    private ICameraDevice selectedPortableDevice;
    private Rectangle _focusrect=new Rectangle(); 

    public LiveViewData LiveViewData { get; set; }

    private Timer _timer = new Timer(1000/30);
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

    public LiveViewWnd(ICameraDevice device)
    {
      InitializeComponent();
      SelectedPortableDevice = device;
      _timer.Stop();
      _timer.AutoReset = true;
      _timer.Elapsed += _timer_Elapsed;
      _focusrect.Stroke = new SolidColorBrush(Colors.Green);
      canvas.Children.Add(_focusrect);
      ServiceProvider.Settings.Manager.PhotoTaked += new Classes.WIAManager.PhotoTakedEventHandler(Manager_PhotoTaked);
    }

    void Manager_PhotoTaked(WIA.Item imageFile)
    {
      _timer.Start();
      StartLiveView();
    }

    void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      if(_retries>100)
      {
        _timer.Stop();

        Dispatcher.BeginInvoke(new ThreadStart(delegate { image1.Visibility = Visibility.Hidden; }));
        return;
      }
      _timer.Enabled = false;
      Dispatcher.BeginInvoke(new ThreadStart(GetLiveImage));
      _timer.Enabled = true;
    }

    private void GetLiveImage()
    {
      try
      {
        LiveViewData = SelectedPortableDevice.GetLiveViewImage();
      }
      catch (Exception)
      {

      }

      if (LiveViewData == null || LiveViewData.ImageData == null)
      {
        _retries++;
        return;
      }

      MemoryStream stream = new MemoryStream(LiveViewData.ImageData, 0, LiveViewData.ImageData.Length);

      JpegBitmapDecoder decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

      decoder.Frames[0].Freeze();
      if (decoder.Frames.Count > 0)
      {
        image1.Source = decoder.Frames[0];
      }


      stream.Close();
      _focusrect.BeginInit();
      _focusrect.Visibility = LiveViewData.HaveFocusData ? Visibility.Visible : Visibility.Hidden;
      double xt = canvas.ActualWidth/LiveViewData.ImageWidth;
      double yt = canvas.ActualHeight/LiveViewData.ImageHeight;
      _focusrect.Height = LiveViewData.FocusFrameXSize*xt;
      _focusrect.Width = LiveViewData.FocusFrameYSize*yt;
      _focusrect.SetValue(Canvas.LeftProperty, LiveViewData.FocusX * xt - (_focusrect.Height/2));
      _focusrect.SetValue(Canvas.TopProperty, LiveViewData.FocusY*yt - (_focusrect.Width/2));
      _focusrect.Stroke = new SolidColorBrush(LiveViewData.Focused ? Colors.Green : Colors.Red);
      _focusrect.EndInit();
      _retries = 0;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {

      StartLiveView();
      Thread.Sleep(500);
      _timer.Start();

      //SelectedPortableDevice.StoptLiveView();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      try
      {
        _timer.Stop();
        Thread.Sleep(100);
        SelectedPortableDevice.StopLiveView();
      }
      catch (Exception exception)
      {
        ServiceProvider.Log.Error("Unable to stop liveview", exception);
      }
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
        _retries = 0;
      }
      catch (Exception exception)
      {
        ServiceProvider.Log.Error("Unable to start liveview !", exception);
        MessageBox.Show("Unable to start liveview !");
        Close();
      }
      image1.Visibility = Visibility.Visible;
    }

    private void image1_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ButtonState == MouseButtonState.Pressed && e.ChangedButton == MouseButton.Left && LiveViewData != null && LiveViewData.HaveFocusData)
      {
        Point _initialPoint = e.MouseDevice.GetPosition(canvas);
        double xt = LiveViewData.ImageWidth/canvas.ActualWidth;
        double yt = LiveViewData.ImageHeight/canvas.ActualHeight;
        int posx = (int) (_initialPoint.X*xt);
        int posy = (int) (_initialPoint.Y*yt);
        selectedPortableDevice.Focus(posx, posy);
      }
    }

    private void btn_focusm_Click(object sender, RoutedEventArgs e)
    {
      selectedPortableDevice.Focus(-10);
    }

    private void btn_focusp_Click(object sender, RoutedEventArgs e)
    {
      selectedPortableDevice.Focus(10);
    }

    private void btn_focusmm_Click(object sender, RoutedEventArgs e)
    {
      selectedPortableDevice.Focus(-100);
    }

    private void btn_focuspp_Click(object sender, RoutedEventArgs e)
    {
      selectedPortableDevice.Focus(100);
    }

    private void btn_focusmmm_Click(object sender, RoutedEventArgs e)
    {
      selectedPortableDevice.Focus(-500);
    }

    private void btn_focusppp_Click(object sender, RoutedEventArgs e)
    {
      selectedPortableDevice.Focus(500);
    }

  }
}
