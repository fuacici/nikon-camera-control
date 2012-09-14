using System;
using System.Collections.Generic;
using CameraControl.Core.Interfaces;

namespace CameraControl.Core.Classes
{
  public class WindowsManager
  {
    public delegate void EventEventHandler(string cmd);
    public virtual event EventEventHandler Event;

    private List<IWindow> WindowsList; 
    public WindowsManager()
    {
      WindowsList = new List<IWindow>();
    }

    public void Add(IWindow window)
    {
      WindowsList.Add(window);
    }

    public void ExecuteCommand(string cmd)
    {
      ExecuteCommand(cmd, null);
    }

    public void ExecuteCommand(string cmd, object o)
    {
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
