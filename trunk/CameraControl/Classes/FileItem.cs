﻿using System;
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

    private BitmapImage _thumbnail;

    public BitmapImage Thumbnail
    {
      get
      {
        if (_thumbnail == null)
        {
          GetExtendedThumb();
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

    public void GetExtendedThumb()
    {
      try
      {
        if (FreeImage.GetFileType(FileName, 0) != FREE_IMAGE_FORMAT.FIF_UNKNOWN)
        {
          FreeImageBitmap bitmap;
          if (FreeImage.GetFileType(FileName, 0) == FREE_IMAGE_FORMAT.FIF_RAW)
            bitmap = new FreeImageBitmap(FileName, FREE_IMAGE_LOAD_FLAGS.RAW_PREVIEW);
          else
            bitmap = new FreeImageBitmap(FileName);

          int dw = bitmap.Width;
          int dh = bitmap.Height;
          int tw = 160;
          int th = 160;
          double zw = (tw/(double) dw);
          double zh = (th/(double) dh);
          double z = (zw <= zh) ? zw : zh;
          dw = (int) (dw*z);
          dh = (int) (dh*z);
          //_thumbnail = byteArrayToImageEx(ImageToByteArray((Bitmap) bitmap.GetThumbnailImage(dw, dh, null, IntPtr.Zero)));
          _thumbnail = byteArrayToImageEx(ImageToByteArray((Bitmap) bitmap.GetThumbnailImage(160, true)));
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
          bitmap.Dispose();
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
      return img;
    }

  }
}
