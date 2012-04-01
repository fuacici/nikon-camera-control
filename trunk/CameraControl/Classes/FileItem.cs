using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace CameraControl.Classes
{
  public class FileItem:BaseFieldClass
  {
    public string FileName { get; set; }
    public string Name { get; set; }
    private BitmapImage _bitmapImage;

    public BitmapImage BitmapImage
    {
      get
      {
        if (_bitmapImage == null)
        {
          _bitmapImage = new BitmapImage();
          _bitmapImage.BeginInit();
          _bitmapImage.UriSource = new Uri(FileName);
          _bitmapImage.EndInit();
          //http://stackoverflow.com/questions/1738978/loading-image-in-thread-with-wpf
          //using (BackgroundWorker bg = new BackgroundWorker())
          //{
          //  bg.DoWork += (sender, args) => FetchImages(viewModelObjectsNeedingImages);
          //  bg.RunWorkerAsync();
          //}
        }
        return _bitmapImage;
      }
      set
      {
        _bitmapImage = value;
        NotifyPropertyChanged("BitmapImage");
      }
    }


    public FileItem(string file)
    {
      FileName = file;
      Name = Path.GetFileName(file);
    }
  }
}
