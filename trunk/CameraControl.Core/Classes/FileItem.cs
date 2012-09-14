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
        if (_bitmapImage == null)
        {
          //_bitmapImage = new BitmapImage();
          //_bitmapImage.BeginInit();
          //_bitmapImage.UriSource = new Uri(FileName);
          //_bitmapImage.EndInit();
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
          _thumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/logo.png"));
          //TODO: remove
          ServiceProvider.QueueManager.Add(new QueueItemFileItem() { FileItem = this });
          //GetExtendedThumb();
          //ServiceProvider.ThumbWorker.AddItem(this);
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
            Thumbnail = byteArrayToImageEx(ImageToByteArray(FreeImage.GetBitmap(bit)));
            FreeImage.UnloadEx(ref dib);
          }
          else
          {
            Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);
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
            Thumbnail = byteArrayToImageEx(ImageToByteArray(tempImage.GetThumbnailImage(160, 120, myCallback, IntPtr.Zero)));
            fs.Close();
          }

          //int dw = bitmap.Width;
          //int dh = bitmap.Height;
          //int tw = 160;
          //int th = 160;
          //double zw = (tw/(double) dw);
          //double zh = (th/(double) dh);
          //double z = (zw <= zh) ? zw : zh;
          //dw = (int) (dw*z);
          //dh = (int) (dh*z);
          //_thumbnail = byteArrayToImageEx(ImageToByteArray((Bitmap) bitmap.GetThumbnailImage(dw, dh, null, IntPtr.Zero)));

          //switch (Photo.Orientation)
          //{
          //  case 3:
          //    _thumbnail.RotateFlip(RotateFlipType.Rotate180FlipNone);
          //    break;
          //  case 6:
          //    _thumbnail.RotateFlip(RotateFlipType.Rotate90FlipNone);
          //    break;
          //  case 8:
          //    _thumbnail.RotateFlip(RotateFlipType.Rotate270FlipNone);
          //    break;
          //  default:
          //    break;
          //}
          //_thumbData = ImageToByteArray(_thumbnail);
          //Size = 256;
          //NotifyPropertyChanged("Thumbnail");
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


    private byte[] ImageToByteArray(Image p_ImageIn)
    {
      byte[] aRet = null;

      using (MemoryStream oMS = new MemoryStream())
      {
        p_ImageIn.Save(oMS, System.Drawing.Imaging.ImageFormat.Jpeg);
        aRet = oMS.ToArray();
      }
      return aRet;
    }

    private BitmapImage byteArrayToImageEx(byte[] byteArrayIn)
    {
      if (byteArrayIn == null)
        return null;
      BitmapImage img = new BitmapImage();
      MemoryStream ms = new MemoryStream(byteArrayIn);
      img.BeginInit();
      img.StreamSource = ms;
      img.EndInit();
      img.Freeze();
      return img;
    }

  }
}
