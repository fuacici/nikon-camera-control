using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using FreeImageAPI;

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

    private BitmapImage _thumbnail;

    public BitmapImage Thumbnail
    {
      get
      {
        if (_thumbnail == null)
        {
          ServiceProvider.ThumbWorker.AddItem(this);
          //GetExtendedThumb();
          //using (BackgroundWorker bg = new BackgroundWorker())
          //{
          //  bg.DoWork += (sender, args) => GetExtendedThumb();
          //  bg.RunWorkerAsync();
          //}
        }
        return _thumbnail;
      }
      set
      {
        _thumbnail = value;
        NotifyPropertyChanged("Thumbnail");
      }
    }

    public FileItem(string file)
    {
      FileName = file;
      Name = Path.GetFileName(file);
    }

    private void CreateHistogramBlack(FIBITMAP dib)
    {
      int height = 100;
      int[] histo = new int[256];
      Bitmap image = new Bitmap(256, height);
      Graphics g = Graphics.FromImage(image);
      g.FillRectangle(new SolidBrush(SystemColors.Window), 0, 0, 256, height);
      double maxValue = 0;


      FreeImage.GetHistogram(dib, histo, FREE_IMAGE_COLOR_CHANNEL.FICC_RGB);

      foreach (int i in histo)
      {
        if (i > maxValue)
          maxValue = i;
      }

      for (int i = 0; i < 256; i++)
      {
        int x = Convert.ToInt32((histo[i] / maxValue) * height);
        g.DrawLine(new Pen(new SolidBrush(Color.Black), 1), i, height, i, height - x);
        //if ((i+1) % 52 == 0)
        //  g.DrawLine(new Pen(new SolidBrush(Color.Black), 1), i, height, i, 0);
      }

      _histogramBlack = byteArrayToImageEx(ImageToByteArray(image));
    }

    private void CreateHistogram(FIBITMAP dib)
    {
      int height = 100;
      int[] histo = new int[256];
      Bitmap image = new Bitmap(256, height);
      Graphics g = Graphics.FromImage(image);
      g.FillRectangle(new SolidBrush(SystemColors.Window), 0, 0, 256, height);
      double maxValue= 0;
      
     
      FreeImage.GetHistogram(dib, histo, FREE_IMAGE_COLOR_CHANNEL.FICC_RED);

      foreach (int i in histo)
      {
        if (i > maxValue)
          maxValue = i;
      }

      
      for (int i = 0; i < 256; i++)
      {
        try
        {
          int x = (int)((histo[i] / maxValue) * height);
          g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(100, 255, 0, 0)), 1), i, height, i, height - x);
        }
        catch (Exception ex)
        {

        }
      }

      maxValue = 0;
      FreeImage.GetHistogram(dib, histo, FREE_IMAGE_COLOR_CHANNEL.FICC_GREEN);

      foreach (int i in histo)
      {
        if (i > maxValue)
          maxValue = i;
      }

      for (int i = 0; i < 256; i++)
      {
        int x = Convert.ToInt32((histo[i] / maxValue) * height);
        g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(100, 0, 255, 0)), 1), i, height, i, height - x);
      }

      maxValue = 0;
      FreeImage.GetHistogram(dib, histo, FREE_IMAGE_COLOR_CHANNEL.FICC_BLUE);
      foreach (int i in histo)
      {
        if (i > maxValue)
          maxValue = i;
      }

      for (int i = 0; i < 256; i++)
      {
        int x = Convert.ToInt32((histo[i]/maxValue)*height);
        g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(100, 0, 0, 255)), 1), i, height, i, height - x);
      }


     _histogram = byteArrayToImageEx(ImageToByteArray(image));
    }

    private BitmapImage _histogram;
    public BitmapImage Histogram
    {
      get { return _histogram; }
      set { _histogram = value; }
    }

    private BitmapImage _histogramBlack;
    public BitmapImage HistogramBlack
    {
      get { return _histogramBlack; }
      set { _histogramBlack = value; }
    }


    public BitmapImage GetBitmap()
    {
      BitmapImage res = null;
      try
      {
        FIBITMAP dib = FreeImage.LoadEx(FileName);
        if (FreeImage.GetFileType(FileName, 0) == FREE_IMAGE_FORMAT.FIF_RAW)
        {
          FIBITMAP bmp = FreeImage.ToneMapping(dib, FREE_IMAGE_TMO.FITMO_REINHARD05, 0, 0); // ConvertToType(dib, FREE_IMAGE_TYPE.FIT_BITMAP, false);
          res = byteArrayToImageEx(ImageToByteArray(FreeImage.GetBitmap(bmp)));
          CreateHistogram(bmp);
          CreateHistogramBlack(bmp);
          FreeImage.UnloadEx(ref dib);
          FreeImage.UnloadEx(ref bmp);
        }
        else
        {
          res = byteArrayToImageEx(ImageToByteArray(FreeImage.GetBitmap(dib)));
          CreateHistogram(dib);
          CreateHistogramBlack(dib);
          FreeImage.UnloadEx(ref dib);
        }

      }
      catch (Exception)
      {
      }
      return res;
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
          }
          else
          {
            dib = FreeImage.LoadEx(FileName);
          }
          FIBITMAP bit = FreeImage.MakeThumbnail(dib, 255, true);
          Thumbnail = byteArrayToImageEx(ImageToByteArray(FreeImage.GetBitmap(bit)));
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
          FreeImage.UnloadEx(ref dib);
          NotifyPropertyChanged("Thumbnail");
        }
      }
      catch (Exception)
      {
      }
    }

    public byte[] ImageToByteArray(Image p_ImageIn)
    {
      byte[] aRet = null;

      using (MemoryStream oMS = new MemoryStream())
      {
        p_ImageIn.Save(oMS, System.Drawing.Imaging.ImageFormat.Jpeg);
        aRet = oMS.ToArray();
      }
      return aRet;
    }

    public BitmapImage byteArrayToImageEx(byte[] byteArrayIn)
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
