using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Core;
using CameraControl.Core.Interfaces;
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
      }
      catch (Exception)
      {


      }
      return true;
    }

    #endregion
  }
}
