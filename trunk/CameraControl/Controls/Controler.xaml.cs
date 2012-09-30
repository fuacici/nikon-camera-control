using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CameraControl.Classes;
using CameraControl.Core;
using CameraControl.Core.Devices;
using CameraControl.Core.Devices.Classes;

namespace CameraControl.Controls
{
  /// <summary>
  /// Interaction logic for Controler.xaml
  /// </summary>
  public partial class Controler : UserControl
  {

    public Controler()
    {
      InitializeComponent();
      CameraDeviceManager cameraDeviceManager = DataContext as CameraDeviceManager;
      if (ServiceProvider.DeviceManager != null)
        ServiceProvider.DeviceManager.PropertyChanged += DeviceManager_PropertyChanged;
    }

    void DeviceManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (ServiceProvider.DeviceManager == null || ServiceProvider.DeviceManager.SelectedCameraDevice == null)
        return;
      if (e.PropertyName == "SelectedCameraDevice")
      {
        Dispatcher.Invoke(new Action(delegate
                                       {
                                         chk_sdram.Visibility =
                                           ServiceProvider.DeviceManager.SelectedCameraDevice.GetCapability(
                                             CapabilityEnum.CaptureInRam)
                                             ? Visibility.Visible
                                             : Visibility.Hidden;
                                       }));
      }
    }

    private void cmb_shutter_GotFocus(object sender, RoutedEventArgs e)
    {
      ComboBox cmb = sender as ComboBox;
      if (cmb != null && cmb.IsFocused)
      {
        //cmb.IsDropDownOpen = !cmb.IsDropDownOpen;
      }
    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.CameraPropertyWnd_Show,
                                              ServiceProvider.DeviceManager.SelectedCameraDevice);
    }


  }
}
