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
    Line _line11=new Line();
    Line _line12 = new Line();
    Line _line21 = new Line();
    Line _line22 = new Line();
    private BackgroundWorker _worker = new BackgroundWorker();
    

    public LiveViewData LiveViewData { get; set; }

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

    public LiveViewWnd(ICameraDevice device)
    {
      InitializeComponent();
      SelectedPortableDevice = device;
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
      ServiceProvider.Settings.Manager.PhotoTakenDone += Manager_PhotoTaked;
      _worker.DoWork += delegate { GetLiveImage(); };
    }


    void Manager_PhotoTaked(WIA.Item imageFile)
    {
      StartLiveView();
      Thread.Sleep(200);
      _timer.Start();
    }

    void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      if (_retries > 100)
      {
        _timer.Stop();

        Dispatcher.BeginInvoke(new ThreadStart(delegate
                                                 {
                                                   image1.Visibility = Visibility.Hidden;
                                                   chk_grid.IsChecked = false;
                                                 }));
        return;
      }
      //_timer.Stop();
      if(!_worker.IsBusy)
        _worker.RunWorkerAsync();
      //Dispatcher.BeginInvoke(new ThreadStart(GetLiveImage));
      //_timer.Start();
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
        Dispatcher.BeginInvoke(new Action(delegate
                                            {
                                              MemoryStream stream = new MemoryStream(LiveViewData.ImageData, 0, LiveViewData.ImageData.Length);

                                              JpegBitmapDecoder decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                                              decoder.Frames[0].Freeze();

                                              if (decoder.Frames.Count > 0)
                                              {
                                                image1.Source = decoder.Frames[0];
                                              }
                                              stream.Close();
                                            }));
      }
      catch (Exception)
      {
        _retries++;
        oper_in_progress = false;
        return;
      }
      Dispatcher.BeginInvoke(new Action(delegate { DrawLines(); ; }));
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
        ServiceProvider.Settings.Manager.PhotoTakenDone -= Manager_PhotoTaked;
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
        oper_in_progress = false;
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

  }
}
