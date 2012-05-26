using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
using CameraControl.Classes;
using WPF.Themes;
using Path = System.IO.Path;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for SettingsWnd.xaml
  /// </summary>
  public partial class SettingsWnd : Window
  {
    public SettingsWnd()
    {
      InitializeComponent();
      foreach (Key key in Enum.GetValues(typeof(System.Windows.Forms.Keys)))
      {
        cmb_keys.Items.Add(key);
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      cmb_themes.ItemsSource = ThemeManager.GetThemes();
      ServiceProvider.Settings.BeginEdit();
    }

    private void button2_Click(object sender, RoutedEventArgs e)
    {
      ServiceProvider.Settings.CancelEdit();
      this.Close();
    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      ServiceProvider.Settings.EndEdit();
      ServiceProvider.Settings.Save();
      this.Close();
    }


    private void button3_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        ProcessStartInfo processStartInfo = new ProcessStartInfo();
        processStartInfo.FileName = "explorer";
        processStartInfo.UseShellExecute = true;
        processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
        processStartInfo.Arguments =
          string.Format("/e,/select,\"{0}\"",
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                     ServiceProvider.AppName, "Log",
                                     "app.log"));
        Process.Start(processStartInfo);
      }
      catch (Exception exception)
      {
        ServiceProvider.Log.Error("Error to show file in explorer", exception);
      }
    }

  }
}
