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

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for SavePresetWnd.xaml
  /// </summary>
  public partial class SavePresetWnd : Window
  {
    public CameraPreset CameraPreset { get; set; }

    public SavePresetWnd(CameraPreset cameraPreset)
    {
      InitializeComponent();
      CameraPreset = cameraPreset;
      CameraPreset.BeginEdit();
    }

    private void btn_save_Click(object sender, RoutedEventArgs e)
    {
      CameraPreset.EndEdit();
      DialogResult = true;
      Close();
    }

    private void btn_cancel_Click(object sender, RoutedEventArgs e)
    {
      CameraPreset.CancelEdit(); 
      Close();
    }
  }
}
