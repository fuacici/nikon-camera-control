using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using CameraControl.Core.Classes;
using CameraControl.Core.Devices.Classes;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for PresetEditWnd.xaml
  /// </summary>
  public partial class PresetEditWnd : Window, INotifyPropertyChanged
  {
    private CameraPreset _selectedCameraPreset;
    public CameraPreset SelectedCameraPreset
    {
      get { return _selectedCameraPreset; }
      set
      {
        _selectedCameraPreset = value;
        NotifyPropertyChanged("SelectedCameraPreset");
      }
    }

    public PresetEditWnd()
    {
      InitializeComponent();
    }

    public virtual event PropertyChangedEventHandler PropertyChanged;

    public virtual void NotifyPropertyChanged(String info)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
    }

    private void btn_del_preset_Click(object sender, RoutedEventArgs e)
    {
      if (lst_preset.SelectedItem != null)
        ServiceProvider.Settings.CameraPresets.Remove((CameraPreset) lst_preset.SelectedItem);
    }

    private void btn_del_prop_Click(object sender, RoutedEventArgs e)
    {
      if (lst_properties.SelectedItem != null)
        SelectedCameraPreset.Values.Remove((ValuePair) lst_properties.SelectedItem);
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      ServiceProvider.Settings.Save();
    }

  }
}
