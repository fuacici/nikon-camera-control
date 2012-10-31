﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using CameraControl.Core.Classes;
using Path = System.IO.Path;
using CameraControl.Core;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for SettingsWnd.xaml
  /// </summary>
  public partial class SettingsWnd 
  {

    public AsyncObservableCollection<RotateFlipType> RotateFlipTypesValues { get; set; }

    public SettingsWnd()
    {
      InitializeComponent();
      foreach (Key key in Enum.GetValues(typeof(System.Windows.Forms.Keys)))
      {
        cmb_keys.Items.Add(key);
      }
      RotateFlipTypesValues = new AsyncObservableCollection<RotateFlipType>(Enum.GetValues(typeof(RotateFlipType)).Cast<RotateFlipType>().Distinct());
      ServiceProvider.Settings.ApplyTheme(this);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      //cmb_themes.ItemsSource = ThemeManager.GetThemes();
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
        Log.Error("Error to show file in explorer", exception);
      }
    }

    private void btn_browse_file_Click(object sender, RoutedEventArgs e)
    {
      OpenFileDialog dialog = new OpenFileDialog();
      dialog.FileName = ServiceProvider.Settings.ExternalViewer;
      if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        ServiceProvider.Settings.ExternalViewer = dialog.FileName;
      }
    }

  }
}
