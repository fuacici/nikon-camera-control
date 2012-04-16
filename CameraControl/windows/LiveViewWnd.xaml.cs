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
    private PortableDevice selectedPortableDevice;
    private Timer _timer = new Timer(1000/25);
    /// <summary>
    /// Gets the <see cref="PortableDevice"/> connected
    /// </summary>
    public ObservableCollection<PortableDevice> PortableDevices
    {
      get;
      private set;
    }

    public PortableDevice SelectedPortableDevice
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
      byte[] result = SelectedPortableDevice.GetLiveView();
      if (result == null)
        return;
      int cbBytesRead = result.Length;
      const int headerSize = 384;

      MemoryStream copy = new MemoryStream((int)cbBytesRead - headerSize);
      copy.Write(result, headerSize, (int)cbBytesRead - headerSize);

      byte[] buffer = copy.GetBuffer();

      MemoryStream stream = new MemoryStream(buffer, 0, buffer.Length);

      JpegBitmapDecoder decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

      decoder.Frames[0].Freeze();
      if (decoder.Frames.Count > 0)
      {
          image1.Source = decoder.Frames[0];
      }
     
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
        PortableDevice portableDevice = device;
        portableDevice.ConnectToDevice(AppName, AppMajorVersionNumber, AppMinorVersionNumber);
        this.SelectedPortableDevice = portableDevice;
        break;
      }

      SelectedPortableDevice.StartLiveView();
      Thread.Sleep(1000);
      _timer.Start();

      //SelectedPortableDevice.StoptLiveView();
    }
  }
}
