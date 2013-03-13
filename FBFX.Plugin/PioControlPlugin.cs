using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Core.Interfaces;
using FBFX.Plugin.Plugins;

namespace FBFX.Plugin
{
  public class PioControlPlugin : IToolPlugin
  {
   
    #region Implementation of IToolPlugin

    public bool Execute()
    {
      PioControl wnd = new PioControl();
      wnd.Show();
      return true;
    }

    public string Title { get; set; }

    #endregion
    
    public PioControlPlugin()
    {
      Title = "Pio control";
    }
   
  }
}
