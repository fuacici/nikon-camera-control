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
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;

namespace CameraControl.Plugins.MainWindowPlugins
{
  /// <summary>
  /// Interaction logic for UserControl1.xaml
  /// </summary>
  public partial class SimpleMainWindow : IMainWindowPlugin
  {
    public string DisplayName { get; set; }

    public SimpleMainWindow()
    {
      DisplayName = "Simple Capture";
      InitializeComponent();
    }

    private void MetroWindow_Closed(object sender, EventArgs e)
    {
      ServiceProvider.WindowsManager.ExecuteCommand(CmdConsts.All_Close);
    }
  }
}
