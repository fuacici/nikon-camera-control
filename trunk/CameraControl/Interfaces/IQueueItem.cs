using CameraControl.Classes;

namespace CameraControl.Interfaces
{
  public interface IQueueItem
  {
    bool Execute(QueueManager manager);
  }
}