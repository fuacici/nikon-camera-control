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
using System.Windows.Shapes;
using CameraControl.Classes;
using CameraControl.Core;

namespace CameraControl.windows
{
    /// <summary>
    /// Interaction logic for ErrorReportWnd.xaml
    /// </summary>
    public partial class ErrorReportWnd
    {
        private string _type;
        public ErrorReportWnd(string type)
        {
            InitializeComponent();
            ServiceProvider.Settings.ApplyTheme(this);
            _type = type;
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_send_Click(object sender, RoutedEventArgs e)
        {
            HelpProvider.SendCrashReport(txt_message.Text, _type);
            this.Close();
        }
    }
}
