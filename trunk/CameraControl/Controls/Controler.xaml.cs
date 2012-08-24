using System.Windows.Controls;
using System.Windows.Input;
using CameraControl.Classes;
using CameraControl.Devices;

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
    }

    private void cmb_shutter_GotFocus(object sender, System.Windows.RoutedEventArgs e)
    {
      ComboBox cmb = sender as ComboBox;
      if (cmb != null && cmb.IsFocused)
      {
        //cmb.IsDropDownOpen = !cmb.IsDropDownOpen;
      }
    }

    private void button1_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.CameraPropertyWnd_Show,
                                              ServiceProvider.DeviceManager.SelectedCameraDevice);
    }


  }
}
