using System.Windows;
using CameraControl.Classes;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for AboutWnd.xaml
  /// </summary>
  public partial class AboutWnd : Window
  {
    public AboutWnd()
    {
      InitializeComponent();
      Title = "About " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
    }

    private void button2_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      PhotoUtils.Run("http://nccsoftware.blogspot.com/");
    }

    private void button3_Click(object sender, RoutedEventArgs e)
    {
      PhotoUtils.Run("http://www.gnu.org/licenses/gpl-3.0.txt");
    }

    private void btn_donate_Click(object sender, RoutedEventArgs e)
    {
      PhotoUtils.Run("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=2FE55TA7MK9DL");
    }
  }
}
