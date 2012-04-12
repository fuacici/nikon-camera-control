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

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for FullScreenWnd.xaml
  /// </summary>
  public partial class FullScreenWnd : Window
  {
    public FullScreenWnd()
    {
      InitializeComponent();
    }

    private void image1_KeyDown(object sender, KeyEventArgs e)
    {
      if(e.Key==Key.Escape)
      {
        Close();
      }
    }

    private void image1_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ClickCount >= 2 && e.LeftButton == MouseButtonState.Pressed)
        Close();
    }
  }
}
