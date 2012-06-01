using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FreeImageAPI;
using FreeImageAPI.Metadata;
using AForge.Imaging;
using Color = System.Drawing.Color;
using Image = System.Drawing.Image;
using Pen = System.Drawing.Pen;

namespace CameraControl.Classes
{
  public class BitmapFile:BaseFieldClass
  {
    private PointCollection luminanceHistogramPoints = null;
    private PointCollection redColorHistogramPoints = null;
    private PointCollection greenColorHistogramPoints = null;
    private PointCollection blueColorHistogramPoints = null;

    public delegate void BitmapLoadedEventHandler(object sender);

    public virtual event BitmapLoadedEventHandler BitmapLoaded;
    
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

    private BitmapSource _displayImage;
    public BitmapSource DisplayImage
    {
      get { return _displayImage; }
      set
      {
        if(_displayImage==null && FileItem!=null)
        {
          _displayImage = FileItem.Thumbnail;
        }
        _displayImage = value;
        NotifyPropertyChanged("DisplayImage");
      }
    }

    public PointCollection LuminanceHistogramPoints
    {
      get
      {
        return this.luminanceHistogramPoints;
      }
      set
      {
        if (this.luminanceHistogramPoints != value)
        {
          this.luminanceHistogramPoints = value;
          NotifyPropertyChanged("LuminanceHistogramPoints");
        }
      }
    }

    public PointCollection RedColorHistogramPoints
    {
      get
      {
        return this.redColorHistogramPoints;
      }
      set
      {
        if (this.redColorHistogramPoints != value)
        {
          this.redColorHistogramPoints = value;
          NotifyPropertyChanged("RedColorHistogramPoints");
        }
      }
    }

    public PointCollection GreenColorHistogramPoints
    {
      get
      {
        return this.greenColorHistogramPoints;
      }
      set
      {
        if (this.greenColorHistogramPoints != value)
        {
          this.greenColorHistogramPoints = value;
          NotifyPropertyChanged("GreenColorHistogramPoints");
        }
      }
    }

    public PointCollection BlueColorHistogramPoints
    {
      get
      {
        return this.blueColorHistogramPoints;
      }
      set
      {
        if (this.blueColorHistogramPoints != value)
        {
          this.blueColorHistogramPoints = value;
          NotifyPropertyChanged("BlueColorHistogramPoints");
        }
      }
    }

    public AsyncObservableCollection<DictionaryItem> Metadata { get; set; }

    public BitmapImage GetBitmap()
    {
      BitmapImage res = null;
      //Metadata.Clear();
      try
      {
        
        if (FreeImage.GetFileType(FileItem.FileName, 0) == FREE_IMAGE_FORMAT.FIF_RAW)
        {
          FIBITMAP dib = FreeImage.LoadEx(FileItem.FileName);
          FIBITMAP bmp = FreeImage.ToneMapping(dib, FREE_IMAGE_TMO.FITMO_REINHARD05, 0, 0); // ConvertToType(dib, FREE_IMAGE_TYPE.FIT_BITMAP, false);
          FileItem.Thumbnail = ToBitmap(FreeImage.GetBitmap(FreeImage.MakeThumbnail(dib, 255, true)));
          DisplayImage = ToBitmap(FreeImage.GetBitmap(bmp));
          //CreateHistogram(bmp);
          //CreateHistogramBlack(bmp);
          FreeImage.UnloadEx(ref dib);
          FreeImage.UnloadEx(ref bmp);
        }
        else
        {
          DisplayImage = ToBitmap(Image.FromFile(FileItem.FileName));
          Thread thread_photo = new Thread(GetAdditionalData);
          thread_photo.Start();
        }
      }
      catch (Exception)
      {
      }
      if (BitmapLoaded != null)
        BitmapLoaded(this);
      return res;
    }

    private void GetAdditionalData()
    {
      try
      {
        using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(FileItem.FileName))
        {
          // Luminance
          ImageStatisticsHSL hslStatistics = new ImageStatisticsHSL(bmp);
          this.LuminanceHistogramPoints = ConvertToPointCollection(hslStatistics.Luminance.Values);
          // RGB
          ImageStatistics rgbStatistics = new ImageStatistics(bmp);
          this.RedColorHistogramPoints = ConvertToPointCollection(rgbStatistics.Red.Values);
          this.GreenColorHistogramPoints = ConvertToPointCollection(rgbStatistics.Green.Values);
          this.BlueColorHistogramPoints = ConvertToPointCollection(rgbStatistics.Blue.Values);
        }
        GetMetadata();
      }
      catch (Exception ex)
      {
        ServiceProvider.Log.Error(ex);
      }
    }

    public void GetMetadata()
    {
      //Exiv2Net.Image image = new Exiv2Net.Image(FileItem.FileName);
      //foreach (KeyValuePair<string, Exiv2Net.Value> i in image)
      //{
      //  Console.WriteLine(i);
      //}
      using (FreeImageBitmap bitmap = new FreeImageBitmap(FileItem.FileName))
      {

        long i = 0;
        foreach (MetadataModel metadataModel in bitmap.Metadata)
        {
          foreach (MetadataTag metadataTag in metadataModel)
          {
            AddMetadataItem(metadataTag);
            if(metadataTag.Key=="AFInfo2")
            {
              byte[] b=metadataTag.Value as byte[];
              string hex =BitConverter.ToString(b);
              //b = b.Reverse().ToArray();

              ushort i6=BitConverter.ToUInt16(b, 6);
            }
            //i += metadataTag.Length;
            //if (!string.IsNullOrEmpty(metadataTag.Description))
            //  Metadata.Add(new DictionaryItem { Name = metadataTag.Description, Value = metadataTag.ToString() });
          }
        }
        //Enumerable.OrderBy(Metadata, dict => dict.Name);
      }
    }

    /*
http://www.exiv2.org/tags-nikon.html

Click on a column header to sort the table.

Tag (hex)	Tag (dec)	IFD	Key	Type	Tag description

0x0000	0	NikonAf2	Exif.NikonAf2.Version	Undefined	Version
0x0004	4	NikonAf2	Exif.NikonAf2.ContrastDetectAF	Byte	Contrast detect AF
0x0005	5	NikonAf2	Exif.NikonAf2.AFAreaMode	Byte	AF area mode
0x0006	6	NikonAf2	Exif.NikonAf2.PhaseDetectAF	Byte	Phase detect AF
0x0007	7	NikonAf2	Exif.NikonAf2.PrimaryAFPoint	Byte	Primary AF point
0x0008	8	NikonAf2	Exif.NikonAf2.AFPointsUsed	Byte	AF points used
0x0010	16	NikonAf2	Exif.NikonAf2.AFImageWidth	Short	AF image width
0x0012	18	NikonAf2	Exif.NikonAf2.AFImageHeight	Short	AF image height
0x0014	20	NikonAf2	Exif.NikonAf2.AFAreaXPosition	Short	AF area x position
0x0016	22	NikonAf2	Exif.NikonAf2.AFAreaYPosition	Short	AF area y position
0x0018	24	NikonAf2	Exif.NikonAf2.AFAreaWidth	Short	AF area width
0x001a	26	NikonAf2	Exif.NikonAf2.AFAreaHeight	Short	AF area height
0x001c	28	NikonAf2	Exif.NikonAf2.ContrastDetectAFInFocus	Short	Contrast detect AF in focus
     * 
*/
    public void AddMetadataItem(MetadataTag tag )
    {
      if (string.IsNullOrEmpty(tag.Description))
        return;
      foreach (DictionaryItem dictionaryItem in Metadata)
      {
        if (dictionaryItem.Name == tag.Description.Trim())
        {
          dictionaryItem.Value = tag.ToString();
          return;
        }
      }
      Metadata.Add(new DictionaryItem { Name = tag.Description.Trim(), Value = tag.ToString()});
    }

    private void SetBitmap(BitmapImage bi,Image image)
    {
      try
      {
        MemoryStream ms = new MemoryStream();
        image.Save(ms, ImageFormat.Bmp);
        ms.Position = 0;
        if(bi.IsFrozen)
        {
          BitmapImage bic = bi.Clone();
          bic.BeginInit();
          bic.StreamSource = ms;
          bic.EndInit();
        }
        else
        {
          bi.BeginInit();
          bi.StreamSource = ms;
          bi.EndInit();
          bi.Freeze();
        }
      }
      catch (Exception ex)
      {
        
        
      }
    }

    public BitmapImage ToBitmap(Bitmap image)
    {
      MemoryStream ms = new MemoryStream();
      image.Save(ms, ImageFormat.Bmp);
      ms.Position = 0;
      BitmapImage bi = new BitmapImage();
      bi.BeginInit();
      bi.StreamSource = ms;
      bi.EndInit();
      bi.Freeze();
      return bi;
    }

    public BitmapImage ToBitmap(Image image)
    {
      MemoryStream ms = new MemoryStream();
      image.Save(ms, ImageFormat.Bmp);
      ms.Position = 0;
      BitmapImage bi = new BitmapImage();
      bi.BeginInit();
      bi.StreamSource = ms;
      bi.EndInit();
      bi.Freeze();
      return bi;
    }

    private byte[] ImageToByteArray(Image p_ImageIn)
    {
      byte[] aRet = null;

      using (MemoryStream oMS = new MemoryStream())
      {
        p_ImageIn.Save(oMS, System.Drawing.Imaging.ImageFormat.Bmp);
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

    public BitmapFile()
    {
      IsLoaded = false;
      Metadata = new AsyncObservableCollection<DictionaryItem>();
      Metadata.Add(new DictionaryItem() {Name = "Exposure mode"});
      Metadata.Add(new DictionaryItem() {Name = "Exposure program"});
      Metadata.Add(new DictionaryItem() {Name = "Exposure time"});
      Metadata.Add(new DictionaryItem() {Name = "F number"});
      Metadata.Add(new DictionaryItem() {Name = "Lens focal length"});
      Metadata.Add(new DictionaryItem() {Name = "ISO speed rating"});
      Metadata.Add(new DictionaryItem() {Name = "Metering mode"});
      Metadata.Add(new DictionaryItem() {Name = "White balance"});
    }
  }
}
