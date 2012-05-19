using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Actions;
using CameraControl.Actions.Enfuse;
using CameraControl.Interfaces;

namespace CameraControl.Classes
{
  public class ActionManager : BaseFieldClass
  {
    private AsyncObservableCollection<IMenuAction> _actions;

    public AsyncObservableCollection<IMenuAction> Actions
    {
      get { return _actions; }
      set
      {
        _actions = value;
        NotifyPropertyChanged("Actions");
      }
    }

    public ActionManager()
    {
      Actions = new AsyncObservableCollection<IMenuAction>();
      Actions.Add(new CmdFocusStacking());
      Actions.Add(new CmdEnfuse());
    }
  }
}
