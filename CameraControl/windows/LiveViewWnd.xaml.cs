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
using CameraControl.Devices.Nikon;
using PortableDeviceLib;
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

    private Timer _timer = new Timer(1000/25);
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
          //this.RaisePropertyChanged("SelectedPortableDevice");
          //Messenger.Default.Send(new ShowDeviceMessage() { Device = value });
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
    }

    void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      if(_retries>100)
      {
        _timer.Stop();
        return;
      }
      _timer.Enabled = false;
      Dispatcher.BeginInvoke(new ThreadStart(GetLiveImage));
      _timer.Enabled = true;
    }

    private void GetLiveImage()
    {

      byte[] result = SelectedPortableDevice.GetLiveViewImage();
      if (result == null)
      {
        _retries++;
        return;
      }

      MemoryStream stream = new MemoryStream(result, 0, result.Length);

      JpegBitmapDecoder decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

      decoder.Frames[0].Freeze();
      if (decoder.Frames.Count > 0)
      {
        image1.Source = decoder.Frames[0];
      }
      stream.Close();
      _retries = 0;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      SelectedPortableDevice.StartLiveView();
      Thread.Sleep(1000);
      _timer.Start();

      //SelectedPortableDevice.StoptLiveView();
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      _timer.Stop();
      Thread.Sleep(100);
      SelectedPortableDevice.StopLiveView();
    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      _timer.Stop();
      Thread.Sleep(100);
      selectedPortableDevice.AutoFocus();
      _timer.Start();
    }

    private void button2_Click(object sender, RoutedEventArgs e)
    {
      _timer.Stop();
      Thread.Sleep(100);
      selectedPortableDevice.TakePictureNoAf();
      _timer.Start();
    }
  }
}
