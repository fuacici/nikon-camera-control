using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Core;
using CameraControl.Core.Interfaces;
using CameraControl.Plugins.ExportPlugins;

namespace CameraControl.Plugins
{
  public class Plugins : IPlugin
  {
    #region Implementation of IPlugin

    public bool Register()
    {
      ServiceProvider.PluginManager.ExportPlugins.Add(new ExportToZip());
      ServiceProvider.PluginManager.ExportPlugins.Add(new ExportToFolder());
      return true;
    }

    #endregion
  }
}
