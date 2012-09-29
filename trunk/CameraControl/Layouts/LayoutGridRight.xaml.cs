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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CameraControl.Layouts
{
  /// <summary>
  /// Interaction logic for LayoutGridRight.xaml
  /// </summary>
  public partial class LayoutGridRight : LayoutBase
  {
    public LayoutGridRight()
    {
      InitializeComponent();
      ImageLIst = ImageLIstBox;
      InitServices();
    }
  }
}
