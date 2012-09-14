using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;

namespace CameraControl.Classes.Queue
{
  public class QueueItemFileItem : IQueueItem
  {
    public FileItem FileItem { get; set; }

    #region Implementation of IQueueItem

    public bool Execute(QueueManager manager)
    {
      try
      {
        FileItem.GetExtendedThumb();
      }
      catch (Exception e)
      {
        //ServiceProvider.Log.Error(e);
      }

      return true;
    }

    #endregion
  }
}
