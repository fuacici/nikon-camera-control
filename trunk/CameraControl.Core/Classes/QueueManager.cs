using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;
using CameraControl.Core.Interfaces;

namespace CameraControl.Core.Classes
{
  public class QueueManager
  {
    public BlockingCollection<IQueueItem> Queue { get; set; }
    private readonly BackgroundWorker _worker;
    private object _locker = new object();


    public QueueManager()
    {
      Queue =new BlockingCollection<IQueueItem>();
      _worker = new BackgroundWorker();
      _worker.DoWork += _worker_DoWork;
    }

    void _worker_DoWork(object sender, DoWorkEventArgs e)
    {
      IQueueItem item;
      while (Queue.TryTake(out item))
      {
        Thread.Sleep(70);
        if (!item.Execute(this))
          break;
      }
    }

    public void Clear()
    {
      while (Queue.Count > 0)
      {
        IQueueItem item;
        Queue.TryTake(out item);
      }
    }

    public void Add(IQueueItem item)
    {
      Queue.Add(item);
      if(!_worker.IsBusy)
        _worker.RunWorkerAsync();
    }

  }
}
