using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Core.Interfaces;

namespace CameraControl.Plugins.ExportPlugins
{
  class ExportToFolder : IExportPlugin
  {
    #region Implementation of IExportPlugin

    public bool Execute()
    {
      return true;
    }

    private string _title;
    public string Title
    {
      get { return "Export To Folder"; }
      set { _title = value; }
    }

    #endregion
  }
}
