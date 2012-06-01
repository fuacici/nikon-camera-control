using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
      System.Diagnostics.Process.Start("http://nccsoftware.blogspot.ro/");
    }

    private void button3_Click(object sender, RoutedEventArgs e)
    {
      System.Diagnostics.Process.Start("http://www.gnu.org/licenses/gpl-3.0.txt");
    }
  }
}
