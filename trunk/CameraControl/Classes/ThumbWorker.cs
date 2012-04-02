using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace CameraControl.Classes
{
  public class ThumbWorker
  {
    private Queue<FileItem> ItemList;
    private BackgroundWorker _backgroundWorker = new BackgroundWorker();
    
    public ThumbWorker()
    {
      ItemList = new Queue<FileItem>();
      _backgroundWorker.DoWork += _backgroundWorker_DoWork;
    }

    public void AddItem(FileItem o)
    {
      ItemList.Enqueue(o);
      //if (!_backgroundWorker.IsBusy)
      //  _backgroundWorker.RunWorkerAsync();
    }

    public void Start()
    {
      if (!_backgroundWorker.IsBusy)
        _backgroundWorker.RunWorkerAsync();
    }

    void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
    {
      ProcedeItem();
    }

    public void ProcedeItem()
    {
      try
      {
        if (ItemList.Count == 0)
          return;
        FileItem fileItem = ItemList.Dequeue();
        if (fileItem != null)
        {
          fileItem.GetExtendedThumb();
          //Dispatcher.CurrentDispatcher.BeginInvoke(new ThreadStart(fileItem.GetExtendedThumb));
        }
        Thread.Sleep(500);
        ProcedeItem();
      }
      catch (Exception)
      {


      }
    }

  }
}
