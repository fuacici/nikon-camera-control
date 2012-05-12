using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
      WindowsList.Add(new FocusStackingWnd());
    }

    public void ExecuteCommand(string cmd)
    {
      foreach (IWindow window in WindowsList)
      {
        window.ExecuteCommand(cmd);
      }
      if (Event != null)
        Event(cmd);
    }

  }
}
