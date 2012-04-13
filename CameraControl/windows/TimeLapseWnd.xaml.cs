using System;
using System.Collections.Generic;
using System.Linq;
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

  }
}
