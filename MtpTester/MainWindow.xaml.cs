using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using CameraControl.Devices.Xml;
using PortableDeviceLib;

namespace MtpTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public PortableDevice SelectedDevice { get; set; }
        private BaseMTPCamera MTPCamera;
        private XmlDeviceData DeviceInfo;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeviceInfo = new XmlDeviceData();
            SelectDevice wnd=new SelectDevice();
            wnd.ShowDialog();
            if (wnd.DialogResult == true && wnd.SelectedDevice != null)
            {
                try
                {
                    SelectedDevice = wnd.SelectedDevice;
                    DeviceDescriptor descriptor = new DeviceDescriptor {WpdId = SelectedDevice.DeviceId};
                    MTPCamera = new BaseMTPCamera();
                    MTPCamera.Init(descriptor);
                    MTPDataResponse res = MTPCamera.ExecuteReadDataEx(0x1001);
                    ErrorCodes.GetException(res.ErrorCode);
                    int index = 2 + 4 + 2;
                    int vendorDescCount = res.Data[index];
                    index += vendorDescCount*2;
                    index += 3;
                    int comandsCount = res.Data[index];
                    index += 2;
                    // load commands
                    for (int i = 0; i < comandsCount; i++)
                    {
                        index += 2;
                        DeviceInfo.AvaiableCommands.Add(new XmlCommandDescriptor()
                                                            {Code = BitConverter.ToUInt16(res.Data, index)});
                    }
                    index += 2;
                    int eventcount = res.Data[index];
                    index += 2;
                    // load events
                    for (int i = 0; i < eventcount; i++)
                    {
                        index += 2;
                        DeviceInfo.AvaiableEvents.Add(new XmlEventDescriptor() { Code = BitConverter.ToUInt16(res.Data, index) });
                    }
                    index += 2;
                    int propertycount = res.Data[index];
                    index += 2;
                    // load properties codes
                    for (int i = 0; i < propertycount; i++)
                    {
                        index += 2;
                        DeviceInfo.AvaiableProperties.Add(new XmlPropertyDescriptor() { Code = BitConverter.ToUInt16(res.Data, index) });
                    }

                    lst_opers.ItemsSource = DeviceInfo.AvaiableCommands;
                    lst_events.ItemsSource = DeviceInfo.AvaiableEvents;
                    lst_prop.ItemsSource = DeviceInfo.AvaiableProperties;
                }
                catch (DeviceException exception)
                {
                    MessageBox.Show("Error getting device information" + exception.Message);
                }
                catch (Exception exception)
                {
                    MessageBox.Show("General error" + exception.Message);
                }
            }

        }

        private void PopulateProperties()
        {
            
        }

    }
}
