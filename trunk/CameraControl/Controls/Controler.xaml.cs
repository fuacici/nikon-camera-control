using System.Windows.Controls;
using System.Windows.Input;

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
    }

    private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      //if(e.Key==Key.S)
      //{
      //  cmb_shutter.IsDropDownOpen = !cmb_shutter.IsDropDownOpen;
      //}
    }

    private void cmb_shutter_GotFocus(object sender, System.Windows.RoutedEventArgs e)
    {
      ComboBox cmb = sender as ComboBox;
      if (cmb != null && cmb.IsFocused)
      {
        cmb.IsDropDownOpen = !cmb.IsDropDownOpen;
      }
    }


  }
}
