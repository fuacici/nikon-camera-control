using System;
using System.Collections.Generic;
using CameraControl.Interfaces;
using CameraControl.windows;

namespace CameraControl.Classes
{
  public class WindowsManager
  {
    public delegate void EventEventHandler(string cmd);
    public virtual event EventEventHandler Event;

    private List<IWindow> WindowsList; 
    public WindowsManager()
    {
      WindowsList = new List<IWindow>();
      WindowsList.Add(new FullScreenWnd());
      WindowsList.Add(new LiveViewWnd());
      WindowsList.Add(new MultipleCameraWnd());
      WindowsList.Add(new CameraPropertyWnd());
      WindowsList.Add(new BrowseWnd());
    }

    public void ExecuteCommand(string cmd)
    {
      ExecuteCommand(cmd, null);
    }

    public void ExecuteCommand(string cmd, object o)
    {
      ServiceProvider.Log.Debug("Window command received :" + cmd);
      foreach (IWindow window in WindowsList)
      {
        window.ExecuteCommand(cmd, o);
      }
      if (Event != null)
        Event(cmd);
    }

    public IWindow Get(Type t) 
    {
      foreach (IWindow window in WindowsList)
      {
        if (window.GetType() == t)
          return window;
      }
      return null;
    }

  }
}
