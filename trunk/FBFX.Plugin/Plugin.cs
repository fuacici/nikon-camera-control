using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Core;
using CameraControl.Core.Interfaces;
using CameraControl.Devices;
using FBFX.Plugin.Plugins;
using FBFX.Plugin.Windows;

namespace FBFX.Plugin
{
  public class Plugin : IPlugin
  {
    #region Implementation of IPlugin

    public bool Register()
    {
      try
      {
        ServiceProvider.WindowsManager.Remove("CameraControl.windows.DownloadPhotosWnd");
        ServiceProvider.WindowsManager.Add(new FBFXDownloadPhotosWnd());
        ServiceProvider.PluginManager.ToolPlugins.Add(new PioControlPlugin());
      }
      catch (Exception exception)
      {
        Log.Error("Error loading plugin FBFX", exception);
      }
      return true;
    }

    #endregion
  }
}
