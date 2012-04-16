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
    private const string AppName = "CameraControl";
    private const int AppMajorVersionNumber = 1;
    private const int AppMinorVersionNumber = 0;
    private NikonD5100 selectedPortableDevice;
    private Timer _timer = new Timer(1000/25);
    /// <summary>
    /// Gets the <see cref="PortableDevice"/> connected
    /// </summary>
    public ObservableCollection<PortableDevice> PortableDevices
    {
      get;
      private set;
    }

    public NikonD5100 SelectedPortableDevice
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

    public LiveViewWnd()
    {
      InitializeComponent();
      _timer.Stop();
      _timer.AutoReset = true;
      _timer.Elapsed += _timer_Elapsed;
    }

    void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
      _timer.Enabled = false;
      Dispatcher.BeginInvoke(new ThreadStart(GetLiveImage));
      _timer.Enabled = true;
    }

    private void GetLiveImage()
    {

      byte[] result = SelectedPortableDevice.GetLiveViewImage();
      if (result == null)
        return;

      MemoryStream stream = new MemoryStream(result, 0, result.Length);

      JpegBitmapDecoder decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

      decoder.Frames[0].Freeze();
      if (decoder.Frames.Count > 0)
      {
          image1.Source = decoder.Frames[0];
      }
     stream.Close();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      PortableDevices = new ObservableCollection<PortableDevice>();
      if (PortableDeviceCollection.Instance == null)
      {
        PortableDeviceCollection.CreateInstance(AppName, AppMajorVersionNumber, AppMinorVersionNumber);
        PortableDeviceCollection.Instance.AutoConnectToPortableDevice = false;
      }

      foreach (var device in PortableDeviceCollection.Instance.Devices)
      {
        this.PortableDevices.Add(device);
        NikonD5100 portableDevice = new NikonD5100(device.DeviceId);
        this.SelectedPortableDevice = portableDevice;
        break;
      }

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
      selectedPortableDevice.AutoFocus();
    }
  }
}
