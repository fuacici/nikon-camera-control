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
  /// Interaction logic for PropertyWnd.xaml
  /// </summary>
  public partial class PropertyWnd : Window
  {
    public PropertyWnd()
    {
      InitializeComponent();
      CommandBindings.Add(new CommandBinding(ApplicationCommands.Close,
          new ExecutedRoutedEventHandler(delegate(object sender, ExecutedRoutedEventArgs args) { this.Close(); })));
    }
 
    public void DragWindow(object sender, MouseButtonEventArgs args)
    {
      DragMove();
    }

  }
}
