using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
using CameraControl.Devices;
using MahApps.Metro.Controls;
using White.Core;
using White.Core.UIItems;
using White.Core.UIItems.Finders;
using White.Core.UIItems.WindowItems;
using Window = White.Core.UIItems.WindowItems.Window;
using Button = White.Core.UIItems.UIItem;
using Path = System.IO;
namespace FBFX.Plugin.Plugins
{
  /// <summary>
  /// Interaction logic for PioControl.xaml
  /// </summary>
  public partial class PioControl : MetroWindow
  {
    public PioControl()
    {
      InitializeComponent();
      ServiceProvider.Settings.ApplyTheme(this);
    }

    private void btn_focus_Click(object sender, RoutedEventArgs e)
    {
      List<Window> windows = Desktop.Instance.Windows();
      foreach (Window window in windows)
      {
        if (window.Title == "fbfx if card control")
        {
          Log.Debug("fbfx if card control Windows found");
          //Button button = window.Get<Button>(SearchCriteria.ByText("Focus Set"));
          //window.DisplayState = DisplayState.Restored;
          window.WaitWhileBusy();
          foreach (IUIItem uiItem in window.Items)
          {
            if (uiItem.Name == "Focus Set" || uiItem.Name == "Set Focus")
            {
              Button item = uiItem as Button;
              item.RaiseClickEvent();
              //window.WaitWhileBusy();
              //window.DisplayState = DisplayState.Minimized;
            }
            Console.WriteLine(uiItem.Name);
          }
          //button.Click();
          break;
        }
      }
    }

    private void btn_shutter_Click(object sender, RoutedEventArgs e)
    {
      List<Window> windows = Desktop.Instance.Windows();
      foreach (Window window in windows)
      {
        if (window.Title == "fbfx if card control")
        {
          Log.Debug("fbfx if card control Windows found");
          //window.DisplayState = DisplayState.Restored;
          window.WaitWhileBusy();
          foreach (IUIItem uiItem in window.Items)
          {
            if (uiItem.Name == "Shutter")
            {
              Button item = uiItem as Button;
              item.RaiseClickEvent();
              //window.WaitWhileBusy();
              //window.DisplayState = DisplayState.Minimized;
            }
          }
          break;
        }
      }
    }

    private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
    {
      bool found = false;
      List<Window> windows = Desktop.Instance.Windows();
      foreach (Window window in windows)
      {
        if (window.Title == "fbfx if card control")
        {
          Log.Debug("fbfx if card control Windows found");
          found = true;
          break;
        }
      }
      if (!found)
      {
        StaticHelper.Instance.SystemMessage = "Pio control not started. Starting now ";
        PhotoUtils.Run(
          Path.Path.Combine(Path.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Pio Control.exe"), "",
          ProcessWindowStyle.Minimized);
      }
    }
  }
}
