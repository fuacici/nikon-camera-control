using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AForge.Imaging;
using CameraControl.Core.Exif.EXIF;
using FreeImageAPI;
using FreeImageAPI.Metadata;
using Image = System.Drawing.Image;

namespace CameraControl.Core.Classes
{
  public class BitmapLoader
  {
    private bool _isworking = false;
    private BitmapFile _nextfile;
    private BitmapFile _currentfile;


    private static BitmapLoader _instance;
    public static BitmapLoader Instance
    {
      get { return _instance ?? (_instance = new BitmapLoader()); }
      set { _instance = value; }
    }

    private BitmapImage _defaultThumbnail;
    public BitmapImage DefaultThumbnail
    {
      get {
        return _defaultThumbnail ??
               (_defaultThumbnail = new BitmapImage(new Uri("pack://application:,,,/Images/logo.png")));
      }
      set { _defaultThumbnail = value; }
    }

    public void GetBitmap(BitmapFile bitmapFile)
    {
      if (_isworking)
      {
        _nextfile = bitmapFile;
        return;
      }
      _nextfile = null;
      _isworking = true;
      _currentfile = bitmapFile;
      _currentfile.RawCodecNeeded = false;
      if (!File.Exists(_currentfile.FileItem.FileName))
      {
        Log.Error("File not found " + _currentfile.FileItem.FileName);
        StaticHelper.Instance.SystemMessage = "File not found " + _currentfile.FileItem.FileName;
      }
      else
      {
        BitmapImage res = null;
        //Metadata.Clear();
        try
        {
          if (_currentfile.FileItem.IsRaw)
          {
            Log.Debug("Loading raw file.");
            BitmapDecoder bmpDec = BitmapDecoder.Create(new Uri(_currentfile.FileItem.FileName),
                                                        BitmapCreateOptions.None,
                                                        BitmapCacheOption.OnLoad);

            _currentfile.DisplayImage = bmpDec.Frames.Single();
            Log.Debug("Loading raw file done.");
          }
          else
          {
            Log.Debug("Loading bitmap file.");
            Bitmap image = (Bitmap) Image.FromFile(_currentfile.FileItem.FileName);
            var exif = new EXIFextractor(ref image, "n");
            if (exif["Orientation"] != null)
            {
              RotateFlipType flip = EXIFextractorEnumerator.OrientationToFlipType(exif["Orientation"].ToString());

              if (flip != RotateFlipType.RotateNoneFlipNone) // don't flip of orientation is correct
              {
                image.RotateFlip(flip);
              }
              if (ServiceProvider.Settings.Rotate != RotateFlipType.RotateNoneFlipNone)
              {
                image.RotateFlip(ServiceProvider.Settings.Rotate);
              }
            }
            _currentfile.DisplayImage = BitmapSourceConvert.ToBitmapSource(image);
            image.Dispose();
            Log.Debug("Loading bitmap file done.");
          }
        }
        catch (FileFormatException)
        {
          _currentfile.RawCodecNeeded = true;
        }
        catch (Exception exception)
        {
          Log.Error(exception);
        }
        if (_nextfile == null)
        {
          Thread threadPhoto = new Thread(GetAdditionalData);
          threadPhoto.Start(_currentfile);
          _currentfile.OnBitmapLoaded();
          _currentfile = null;
          _isworking = false;

        }
        else
        {
          _isworking = false;
          GetBitmap(_nextfile);
        }
      }
    }

    private void GetAdditionalData(object o)
    {
      BitmapFile file = o as BitmapFile;
      try
      {
        if (!file.FileItem.IsRaw)
        {
          using (Bitmap bmp = new Bitmap(file.FileItem.FileName))
          {
            // Luminance
            ImageStatisticsHSL hslStatistics = new ImageStatisticsHSL(bmp);
            file.LuminanceHistogramPoints = ConvertToPointCollection(hslStatistics.Luminance.Values);
            // RGB
            ImageStatistics rgbStatistics = new ImageStatistics(bmp);
            file.RedColorHistogramPoints = ConvertToPointCollection(rgbStatistics.Red.Values);
            file.GreenColorHistogramPoints = ConvertToPointCollection(rgbStatistics.Green.Values);
            file.BlueColorHistogramPoints = ConvertToPointCollection(rgbStatistics.Blue.Values);
          }
        }
        GetMetadata(file);
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    public void GetMetadata(BitmapFile file)
    {
      //Exiv2Net.Image image = new Exiv2Net.Image(FileItem.FileName);
      //foreach (KeyValuePair<string, Exiv2Net.Value> i in image)
      //{
      //  Console.WriteLine(i);
      //}
      using (FreeImageBitmap bitmap = new FreeImageBitmap(file.FileItem.FileName))
      {
        foreach (MetadataModel metadataModel in bitmap.Metadata)
        {
          foreach (MetadataTag metadataTag in metadataModel)
          {
            AddMetadataItem(metadataTag, file);
            if (metadataTag.Key == "AFInfo2")
            {
              byte[] b = metadataTag.Value as byte[];
              string hex = BitConverter.ToString(b);
              //b = b.Reverse().ToArray();

              ushort i6 = BitConverter.ToUInt16(b, 6);
            }
            //i += metadataTag.Length;
            //if (!string.IsNullOrEmpty(metadataTag.Description))
            //  Metadata.Add(new DictionaryItem { Name = metadataTag.Description, Value = metadataTag.ToString() });
          }
        }
        //Enumerable.OrderBy(Metadata, dict => dict.Name);
      }
    }

    public void AddMetadataItem(MetadataTag tag, BitmapFile bitmapFile)
    {
      if (string.IsNullOrEmpty(tag.Description))
        return;
      foreach (DictionaryItem dictionaryItem in bitmapFile.Metadata)
      {
        if (dictionaryItem.Name == tag.Description.Trim())
        {
          dictionaryItem.Value = tag.ToString();
          return;
        }
      }
      bitmapFile.Metadata.Add(new DictionaryItem {Name = tag.Description.Trim(), Value = tag.ToString()});
    }

    private PointCollection ConvertToPointCollection(int[] values)
    {
      //if (this.PerformHistogramSmoothing)
      //{
      values = SmoothHistogram(values);
      //}

      int max = values.Max();

      PointCollection points = new PointCollection();
      // first point (lower-left corner)
      points.Add(new System.Windows.Point(0, max));
      // middle points
      for (int i = 0; i < values.Length; i++)
      {
        points.Add(new System.Windows.Point(i, max - values[i]));
      }
      // last point (lower-right corner)
      points.Add(new System.Windows.Point(values.Length - 1, max));
      points.Freeze();
      return points;
    }

    private int[] SmoothHistogram(int[] originalValues)
    {
      int[] smoothedValues = new int[originalValues.Length];

      double[] mask = new double[] { 0.25, 0.5, 0.25 };

      for (int bin = 1; bin < originalValues.Length - 1; bin++)
      {
        double smoothedValue = 0;
        for (int i = 0; i < mask.Length; i++)
        {
          smoothedValue += originalValues[bin - 1 + i] * mask[i];
        }
        smoothedValues[bin] = (int)smoothedValue;
      }

      return smoothedValues;
    }
  }
}
