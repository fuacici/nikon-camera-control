using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Core;
using CameraControl.Core.Interfaces;

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
      }
      catch (Exception)
      {


      }
      return true;
    }

    #endregion
  }
}
