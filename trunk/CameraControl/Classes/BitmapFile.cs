using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using FreeImageAPI;

namespace CameraControl.Classes
{
  public class BitmapFile:BaseFieldClass
  {
    private FileItem _fileItem;
    public FileItem FileItem
    {
      get { return _fileItem; }
      set
      {
        _fileItem = value;
        NotifyPropertyChanged("FileItem");
      }
    }

    private bool _isLoaded;
    public bool IsLoaded
    {
      get { return _isLoaded; }
      set
      {
        _isLoaded = value;
        NotifyPropertyChanged("IsLoaded");
      }
    }

    private BitmapImage _displayImage;
    public BitmapImage DisplayImage
    {
      get { return _displayImage; }
      set
      {
        if(_displayImage==null || FileItem!=null)
        {
          _displayImage = FileItem.Thumbnail;
        }
        _displayImage = value;
        NotifyPropertyChanged("DisplayImage");
      }
    }

    private BitmapImage _histogram;
    public BitmapImage Histogram
    {
      get { return _histogram; }
      set
      {
        _histogram = value;
        NotifyPropertyChanged("Histogram");
      }
    }

    private BitmapImage _histogramBlack;
    public BitmapImage HistogramBlack
    {
      get { return _histogramBlack; }
      set
      {
        _histogramBlack = value;
        NotifyPropertyChanged("HistogramBlack");
      }
    }

    private void CreateHistogramBlack(FIBITMAP dib)
    {
      int height = 100;
      int[] histo = new int[256];
      Bitmap image = new Bitmap(256, height);
      Graphics g = Graphics.FromImage(image);
      g.FillRectangle(new SolidBrush(Color.FromArgb(100, 255, 255, 255)), 0, 0, 256, height);
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

      HistogramBlack = byteArrayToImageEx(ImageToByteArray(image));
    }

    private void CreateHistogram(FIBITMAP dib)
    {
      int height = 100;
      int[] histo = new int[256];
      Bitmap image = new Bitmap(256, height);
      Graphics g = Graphics.FromImage(image);
      g.FillRectangle(new SolidBrush(SystemColors.Window), 0, 0, 256, height);
      double maxValue = 0;


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
          g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(126, 255, 0, 0)), 1), i, height, i, height - x);
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
        g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(126, 0, 255, 0)), 1), i, height, i, height - x);
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
        int x = Convert.ToInt32((histo[i] / maxValue) * height);
        g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(126, 0, 0, 255)), 1), i, height, i, height - x);
      }


      Histogram = byteArrayToImageEx(ImageToByteArray(image));
    }




    public BitmapImage GetBitmap()
    {
      BitmapImage res = null;
      try
      {
        FIBITMAP dib = FreeImage.LoadEx(FileItem.FileName);
        if (FreeImage.GetFileType(FileItem.FileName, 0) == FREE_IMAGE_FORMAT.FIF_RAW)
        {
          FIBITMAP bmp = FreeImage.ToneMapping(dib, FREE_IMAGE_TMO.FITMO_REINHARD05, 0, 0); // ConvertToType(dib, FREE_IMAGE_TYPE.FIT_BITMAP, false);
          FileItem.Thumbnail = byteArrayToImageEx(ImageToByteArray(FreeImage.GetBitmap(FreeImage.MakeThumbnail(dib, 255, true))));
          DisplayImage = byteArrayToImageEx(ImageToByteArray(FreeImage.GetBitmap(bmp)));
          CreateHistogram(bmp);
          CreateHistogramBlack(bmp);
          FreeImage.UnloadEx(ref dib);
          FreeImage.UnloadEx(ref bmp);
        }
        else
        {
          DisplayImage = byteArrayToImageEx(ImageToByteArray(FreeImage.GetBitmap(dib)));
          FileItem.Thumbnail = byteArrayToImageEx(ImageToByteArray(FreeImage.GetBitmap(FreeImage.MakeThumbnail(dib, 255, true))));
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

    public void SetFileItem(FileItem item)
    {
      FileItem = item;
      IsLoaded = false;
      DisplayImage = FileItem.Thumbnail;
    }

    public BitmapFile()
    {
      IsLoaded = false;
    }
  }
}
