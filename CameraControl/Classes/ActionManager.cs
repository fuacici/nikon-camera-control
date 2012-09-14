using CameraControl.Actions;
using CameraControl.Actions.Enfuse;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;

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
      Actions = new AsyncObservableCollection<IMenuAction>
                  {new CmdFocusStackingCombineZP(), new CmdEnfuse(), new CmdToJpg(), new CmdExpJpg()};
    }
  }
}
