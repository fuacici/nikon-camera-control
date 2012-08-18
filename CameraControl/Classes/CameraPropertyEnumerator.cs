using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraControl.Classes
{
  public class CameraPropertyEnumerator
  {
    public AsyncObservableCollection<CameraProperty> Items { get; set; }

    public CameraPropertyEnumerator()
    {
      Items=new AsyncObservableCollection<CameraProperty>();
    }

  }
}
