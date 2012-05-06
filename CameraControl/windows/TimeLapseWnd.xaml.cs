using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MessageBox = System.Windows.Forms.MessageBox;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for TimeLapseWnd.xaml
  /// </summary>
  public partial class TimeLapseWnd : Window
  {
    public TimeLapseWnd()
    {
      InitializeComponent();
    }

    private void btn_start_Click(object sender, RoutedEventArgs e)
    {
      ServiceProvider.Settings.DefaultSession.TimeLapse.Start();
      Close();
    }

    private void button2_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      if(!CheckCodec())
      {
        if(MessageBox.Show("Xvid codec not instaled !\nDo you want to download and install it ? ","Video codec problem",MessageBoxButtons.YesNo)==System.Windows.Forms.DialogResult.Yes)
        {
          System.Diagnostics.Process.Start("http://www.xvid.org/Downloads.15.0.html");
        }
        return;
      }
      Hide();
      CreateTimeLapseWnd wnd = new CreateTimeLapseWnd();
      wnd.ShowDialog();
      Show();
    }

    private void button3_Click(object sender, RoutedEventArgs e)
    {
      SaveFileDialog dialog = new SaveFileDialog();
      dialog.Filter = "Avi files (*.avi)|*.avi|All files (*.*)|*.*";
      dialog.AddExtension = true;
      dialog.FileName = ServiceProvider.Settings.DefaultSession.TimeLapse.OutputFIleName;
      if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        ServiceProvider.Settings.DefaultSession.TimeLapse.OutputFIleName = dialog.FileName;
      }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      ServiceProvider.Settings.Save(ServiceProvider.Settings.DefaultSession);
    }

    private bool CheckCodec()
    {
      ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_CodecFile");
      ManagementObjectCollection collection = searcher.Get();
     foreach (ManagementObject obj in collection)
      {
        if ((string)obj["Description"] == "Xvid MPEG-4 Video Codec")
        {
          return true;
        }
      }
      return false;
    }
  }
}
