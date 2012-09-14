using System;
using CameraControl.Core.Interfaces;

namespace CameraControl.Core.Classes.Queue
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
        //Log.Error(e);
      }

      return true;
    }

    #endregion
  }
}
