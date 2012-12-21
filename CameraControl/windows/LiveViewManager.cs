using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Classes;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;

namespace CameraControl.windows
{
  public class LiveViewManager : IWindow
  {
    #region Implementation of IWindow

    private Dictionary<object, LiveViewWnd> _register;

    public LiveViewManager()
    {
      _register = new Dictionary<object, LiveViewWnd>();
    }

    public void ExecuteCommand(string cmd, object param)
    {
      switch (cmd)
      {
        case WindowsCmdConsts.LiveViewWnd_Show:
          if (!_register.ContainsKey(param))
          {
            LiveViewWnd wnd = new LiveViewWnd();
            ServiceProvider.Settings.ApplyTheme(wnd);
            _register.Add(param, wnd);
          }
          _register[param].ExecuteCommand(cmd, param);
          break;
        case WindowsCmdConsts.LiveViewWnd_Hide:
          if (_register.ContainsKey(param))
            _register[param].ExecuteCommand(cmd, param);
          break;
        case CmdConsts.All_Close:
          foreach (var liveViewWnd in _register)
          {
            liveViewWnd.Value.ExecuteCommand(cmd, param);
          }
          break;
          default:
          foreach (var liveViewWnd in _register)
          {
            if (cmd.StartsWith("LiveView"))
            liveViewWnd.Value.ExecuteCommand(cmd, param);
          }
          break;
     }

    }

    public bool IsVisible { get; private set; }

    #endregion
  }
}
