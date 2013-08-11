using System;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using CameraControl.Core;
using CameraControl.Core.Interfaces;
using CameraControl.Devices.Classes;
using CameraControl.Devices.Nikon;
using MahApps.Metro.Controls;

namespace Plugin.DeviceControl
{
    /// <summary>
    /// Interaction logic for WaterDropWnd.xaml
    /// </summary>
    public partial class WaterDropWnd :MetroWindow, IToolPlugin
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
                NikonBase camera = ServiceProvider.DeviceManager.SelectedCameraDevice as NikonBase;
                if(camera!=null)
                {
                    camera.StopEventTimer();
                    DelayedDelegate.Add(camera.StartEventTimer, GetTotalLength()+100);
                }
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
                sp.PortName = (string)cmb_ports.SelectedItem;
                sp.BaudRate = 9600;
                sp.WriteTimeout = 500;
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
            Dispatcher.Invoke(new Action(delegate { lst_message.Items.Add(str); }));
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
                        case "drop2_wait_time":
                            Dispatcher.Invoke(new Action(delegate { slider_drop2_wait.Value = value; }));
                            break;
                        case "drop2_time":
                            Dispatcher.Invoke(new Action(delegate { slider_drop2.Value = value; }));
                            break;
                        case "drop3_time":
                            Dispatcher.Invoke(new Action(delegate { slider_drop3.Value = value; }));
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


        private int GetTotalLength()
        {
            return slider_cmera.Value + slider_drop1.Value + slider_drop_wait.Value + slider_drop2_wait.Value +
                   slider_drop2.Value + slider_drop3.Value + slider_flash.Value;
        }

        private void SendData()
        {
            try
            {
                lst_message.Items.Clear();
                OpenPort();
                sp.WriteLine("c=" + slider_cmera.Value);
                sp.WriteLine("dw=" + slider_drop_wait.Value);
                sp.WriteLine("dw2=" + slider_drop_wait.Value);
                sp.WriteLine("d1=" + slider_drop1.Value);
                sp.WriteLine("d2=" + slider_drop2.Value);
                sp.WriteLine("d3=" + slider_drop3.Value);
                sp.WriteLine("f=" + slider_flash.Value.ToString());
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
            try
            {
                ClosePort();
            }
            catch (Exception)
            {
            }
        }

        private void btn_get_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("?");
        }

        private void cmb_ports_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SendCommand("?");
        }

        private void btn_valve_Click(object sender, RoutedEventArgs e)
        {
            SendCommand(">");
        }

        private void SendCommand(string cmd)
        {
            try
            {
                ClosePort();
                OpenPort();
                sp.Write(cmd);
            }
            catch (Exception exception)
            {
                lst_message.Items.Add(exception.Message);
            }
   
        }

        private void btn_drop_Click(object sender, RoutedEventArgs e)
        {
            SendCommand("<");
        }

    }
}
