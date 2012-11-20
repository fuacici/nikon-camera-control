using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Core.Interfaces;

namespace CameraControl.Plugins.ExportPlugins
{
  public class ExportToZip : IExportPlugin
  {
    #region Implementation of IExportPlugin

    private string _title;

    public bool Execute()
    {

      return true;
    }

    public string Title
    {
      get { return "Export to zip"; }
      set { _title = value; }
    }

    #endregion
  }
}
