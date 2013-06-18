using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using CameraControl.Core;
using CameraControl.Core.Interfaces;

namespace CameraControl.Plugins.ToolPlugin
{
    /// <summary>
    /// Interaction logic for WaterDropWnd.xaml
    /// </summary>
    public partial class WaterDropWnd : IToolPlugin
    {
        private SerialPort sp = new SerialPort();

        public WaterDropWnd()
        {
            InitializeComponent();
            Title = "Water drop control";
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                cmb_ports.Items.Add(port);
            }
            ServiceProvider.Settings.ApplyTheme(this);
        }

        #region Implementation of IToolPlugin

        public bool Execute()
        {
            WaterDropWnd wnd = new WaterDropWnd();
            wnd.Show();
            return true;
        }

        #endregion

        private void btn_start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenPort();
                sp.WriteLine(" ");
            }
            catch (Exception exception)
            {
                lst_message.Items.Add(exception.Message);
            }
        }

        private void OpenPort()
        {
            if (!sp.IsOpen)
            {
                sp.PortName = (string)cmb_ports.SelectionBoxItem;
                sp.Open();
                sp.DataReceived += sp_DataReceived;
            }
        }

        private void ClosePort()
        {
            if(sp.IsOpen)
            {
                sp.DataReceived -= sp_DataReceived;
                sp.Close();
            }
        }
         
        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort spL = (SerialPort)sender;
            string str = spL.ReadLine();
            if(str.Contains("="))
            {
                string command = str.Split('=')[0];
                int value = 0;
                if (int.TryParse(str.Split('=')[1], out value))
                {
                    switch (command)
                    {
                        case "camera_timer":
                            Dispatcher.Invoke(new Action(delegate { slider_cmera.Value = value; }));
                            break;
                        case "drop1_time":
                            Dispatcher.Invoke(new Action(delegate { slider_drop1.Value = value; }));
                            break;
                        case "drop_wait_time":
                            Dispatcher.Invoke(new Action(delegate { slider_drop_wait.Value = value; }));
                            break;
                        case "drop2_time":
                            Dispatcher.Invoke(new Action(delegate { slider_drop2.Value = value; }));
                            break;
                        case "flash_time":
                            Dispatcher.Invoke(new Action(delegate { slider_flash.Value = value; }));
                            break;
                    }
                }
            }
            Console.WriteLine(str);

            Console.WriteLine();
        }

        private void SendData()
        {
            try
            {
                OpenPort();
                sp.WriteLine("camera_timer=" + slider_cmera.Value);
                sp.WriteLine("drop1_time=" + slider_drop1.Value);
                sp.WriteLine("drop_wait_time=" + slider_drop_wait.Value);
                sp.WriteLine("drop2_time=" + slider_drop2.Value);
                sp.WriteLine("flash_time=" + slider_flash.Value);
            }
            catch (Exception exception)
            {
                lst_message.Items.Add(exception.Message);
            }

        }

        private void btn_set_Click(object sender, RoutedEventArgs e)
        {
            SendData();
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            ClosePort();
        }

        private void btn_get_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenPort();
                sp.Write("?");
            }
            catch (Exception exception)
            {
                lst_message.Items.Add(exception.Message);
            }
        }
    }
}
