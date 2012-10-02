using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using CameraControl.Core.Classes.Queue;
using CameraControl.Core.Exif.EXIF;
using FreeImageAPI;

namespace CameraControl.Core.Classes
{
  public class FileItem:BaseFieldClass
  {
    private BitmapSource _defaulImage;

    private string _fileName;
    public string FileName
    {
      get { return _fileName; }
      set
      {
        _fileName = value;
        if (File.Exists(_fileName))
        {
          FileDate = File.GetLastWriteTime(_fileName);
        }

      }
    }

    public bool IsRaw
    {
      get { return !string.IsNullOrEmpty(FileName) && Path.GetExtension(FileName).ToLower() == ".nef"; }
    }

    public DateTime FileDate { get; set; }

    public string Name { get; set; }

    [XmlIgnore]
    public string ToolTip
    {
      get { return string.Format("File name: {0}\nFile date :{1}",Name,FileDate.ToShortDateString()); }
    }

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

    private BitmapImage _bitmapImage;
    [XmlIgnore]
    public BitmapImage BitmapImage
    {
      get
      {
        return _bitmapImage;
      }
      set
      {
        if(value==null)
        {
          
        }
        _bitmapImage = value;
        NotifyPropertyChanged("BitmapImage");
      }
    }

    public FileItem()
    {
      
    }

    public FileItem(string file)
    {
      FileName = file;
      Name = Path.GetFileName(file);
    }

    private BitmapSource _thumbnail;
    [XmlIgnore]
    public BitmapSource Thumbnail
    {
      get
      {
        if (_thumbnail == null)
        {
          _thumbnail = BitmapLoader.Instance.DefaultThumbnail;
          if (!ServiceProvider.Settings.DontLoadThumbnails)
            ServiceProvider.QueueManager.Add(new QueueItemFileItem() {FileItem = this});
        }
        return _thumbnail;
      }
      set
      {
        _thumbnail = value;
        NotifyPropertyChanged("Thumbnail");
      }
    }

    public void GetExtendedThumb()
    {
      try
      {
        if (FreeImage.GetFileType(FileName, 0) != FREE_IMAGE_FORMAT.FIF_UNKNOWN)
        {

          FIBITMAP dib = new FIBITMAP();
          if (FreeImage.GetFileType(FileName, 0) == FREE_IMAGE_FORMAT.FIF_RAW)
          {
            dib = FreeImage.LoadEx(FileName, FREE_IMAGE_LOAD_FLAGS.RAW_PREVIEW);
            FIBITMAP bit = FreeImage.MakeThumbnail(dib, 255, true);
            Thumbnail = BitmapSourceConvert.ToBitmapSource(FreeImage.GetBitmap(bit));
            FreeImage.UnloadEx(ref dib);
          }
          else
          {
            Image.GetThumbnailImageAbort myCallback = ThumbnailCallback;
            Stream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); // or any stream
            Image tempImage = Image.FromStream(fs, true, false);
            var exif = new EXIFextractor(ref tempImage, "n");
            if (exif["Orientation"] != null)
            {
              RotateFlipType flip = EXIFextractorEnumerator.OrientationToFlipType(exif["Orientation"].ToString());

              if (flip != RotateFlipType.RotateNoneFlipNone)  // don't flip of orientation is correct
              {
                tempImage.RotateFlip(flip);
              }
            }
            Thumbnail =
              BitmapSourceConvert.ToBitmapSource((Bitmap) tempImage.GetThumbnailImage(160, 120, myCallback, IntPtr.Zero));
            tempImage.Dispose();
            fs.Close();
          }
        }
      }
      catch (Exception)
      {
      }
    }

    private bool ThumbnailCallback()
    {
      return false;
    }
  }
}
