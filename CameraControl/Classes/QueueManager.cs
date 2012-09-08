using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using CameraControl.Interfaces;

namespace CameraControl.Classes
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
      while (true)
      {
        var item = Queue.Take();
        if (!item.Execute(this))
          break;
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
