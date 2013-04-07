using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml.Serialization;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using CameraControl.Devices.Xml;
using Microsoft.Win32;
using PortableDeviceLib;
using Path = System.Windows.Shapes.Path;

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
            //MenuItem_Click(null, null);
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
                    DeviceInfo.Manufacturer = SelectedDevice.Manufacturer;
                    DeviceInfo.Model = SelectedDevice.Model;
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
                    PopulateProperties();
                    InitUi();
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
            foreach (XmlPropertyDescriptor xmlPropertyDescriptor in DeviceInfo.AvaiableProperties)
            {
                try
                {
                    int index=0;
                    MTPDataResponse result = MTPCamera.ExecuteReadDataEx(BaseMTPCamera.CONST_CMD_GetDevicePropDesc, xmlPropertyDescriptor.Code);
                    ErrorCodes.GetException(result.ErrorCode);
                    uint dataType = BitConverter.ToUInt16(result.Data, 2);
                    xmlPropertyDescriptor.DataType = dataType;
                    int dataLength = 0;
                    switch (dataType)
                    {
                           //0x0001	INT8	Signed 8-bit integer
                        case 0x0001:
                            dataLength = 1;
                            break;
                          //0x0002	UINT8	Unsigned 8-bit integer
                        case 0x0002:
                            dataLength = 1;
                            break;
                          //0x0003	INT16	Signed 16-bit integer
                        case 0x0003:
                            dataLength = 2;
                            break;
                        //0x0004	UINT16	Unsigned 16-bit integer
                        case 0x0004:
                            dataLength = 2;
                            break;
                        //0x0005	INT32	Signed 32-bit integer
                        case 0x0005:
                            dataLength = 4;
                            break;
                        //0x0006	UINT32	Unsigned 32-bit integer
                        case 0x0006:
                            dataLength = 4;
                            break;
                        //0x0007	INT64	Signed 64-bit integer
                        case 0x0007:
                            dataLength = 8;
                            break;
                        //0x0008	UINT64	Unsigned 64-bit integer
                        case 0x0008:
                            dataLength = 8;
                            break;
                        //0x0009	INT128	Signed 128-bit integer
                        case 0x0009:
                            dataLength = 16;
                            break;
                        //0x000A	UINT128	Unsigned 128-bit integer
                        case 0x000A:
                            dataLength = 16;
                            break;
                        //0x4001	AINT8	Signed 8-bit integer array
                        case 0x4001:
                            dataLength = 1;
                            break;
                        //0x4002	AUINT8	Unsigned 8-bit integer array
                        case 0x4002:
                            dataLength = 1;
                            break;
                        //0x4003	AINT16	Signed 16-bit integer array
                        case 0x4003:
                            dataLength = 2;
                            break;
                        //0x4004	AUINT16	Unsigned 16-bit integer array
                        case 0x4004:
                            dataLength = 2;
                            break;
                        //0x4005	AINT32	Signed 32-bit integer array
                        case 0x4005:
                            dataLength = 4;
                            break;
                        //0x4006	AUINT32	Unsigned 32-bit integer array
                        case 0x4006:
                            dataLength = 4;
                            break;
                        //0x4007	AINT64	Signed 64-bit integer array
                        case 0x4007:
                            dataLength = 8;
                            break;
                        //0x4008	AUINT64	Unsigned 64-bit integer array
                        case 0x4008:
                            dataLength = 8;
                            break;
                        //0x4009	AINT128	Signed 128-bit integer array
                        case 0x4009:
                            dataLength = 16;
                            break;
                        //0x400A	AUINT128	Unsigned 128-bit integer array
                        case 0x400A:
                            dataLength = 16;
                            break;
                        //0xFFFF	STR	Variable length Unicode character string
                        case 0xFFFF:
                            dataLength = -1;
                            break;
                    }
                    if (dataLength < 1)
                        continue;
                    index += 4;
                    byte datareadonly = result.Data[index];
                    index += 1;
                    //factory def
                    index += dataLength;
                    // current value
                    index += dataLength;

                    byte formFlag = result.Data[index];
                    index += 1;
                    xmlPropertyDescriptor.DataForm = formFlag;
                    //UInt16 defval = BitConverter.ToUInt16(result.Data, 7);
                    if (formFlag == 2)
                    {
                        int length = BitConverter.ToInt16(result.Data, index);
                        index += 2;
                        for (int i = 0; i < length; i ++)
                        {
                            long val = GetValue(result, index, dataLength); ;
                            xmlPropertyDescriptor.Values.Add(new XmlPropertyValue(){Value = val});
                            index += dataLength;
                        }
                    }
                    if(formFlag==1)
                    {
                        long min = GetValue(result,index,dataLength);
                        index += dataLength;
                        long max = GetValue(result, index, dataLength);
                        index += dataLength;
                        long inc = GetValue(result, index, dataLength);
                        for (long i = min; i < max; i+=inc)
                        {
                            xmlPropertyDescriptor.Values.Add(new XmlPropertyValue() { Value = i });
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("Error process property "+exception.Message);
                }
            }
        }


        private long GetValue (MTPDataResponse result,int index, int dataLength )
        {
            long val = 0;
            switch (dataLength)
            {
                case 1:
                    val = result.Data[index];
                    break;
                case 2:
                    val = BitConverter.ToUInt16(result.Data, index);
                    break;
                case 4:
                    val = BitConverter.ToUInt32(result.Data, index);
                    break;
                default:
                    val = (long)BitConverter.ToUInt64(result.Data, index);
                    break;
            }
            return val;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Xml file (*.xml)|*.xml";
            if (dialog.ShowDialog() == true)
            {
                Save(dialog.FileName);
            }
        }

        public void Save(string filename)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(XmlDeviceData));
                // Create a FileStream to write with.

                Stream writer = new FileStream(filename, FileMode.Create);
                // Serialize the object, and close the TextWriter
                serializer.Serialize(writer, DeviceInfo);
                writer.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Unable to save data "+exception.Message);
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Xml file (*.xml)|*.xml";
                if (dialog.ShowDialog() == true)
                {
                    DeviceInfo = XmlDeviceData.Load(dialog.FileName);
                }
                InitUi();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error loading file " + exception.Message);
            }
        }


        private void InitUi()
        {
            lst_opers.ItemsSource = DeviceInfo.AvaiableCommands;
            lst_events.ItemsSource = DeviceInfo.AvaiableEvents;
            lst_prop.ItemsSource = DeviceInfo.AvaiableProperties;
        }

    }
}
