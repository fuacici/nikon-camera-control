using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraControl.Classes
{
  public class CheckedListItem : BaseFieldClass
  {
    public int Id { get; set; }
    public string Name { get; set; }
    private bool _isChecked;

    public bool IsChecked
    {
      get { return _isChecked; }
      set
      {
        _isChecked = value;
        NotifyPropertyChanged("IsChecked");
      }
    }
  }
}
