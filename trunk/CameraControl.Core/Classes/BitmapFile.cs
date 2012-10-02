using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
      if(!File.Exists(FileItem.FileName))
      {
        Log.Error("File not found " + FileItem.FileName);
        StaticHelper.Instance.SystemMessage = "File not found " + FileItem.FileName;
      }
      BitmapImage res = null;
      //Metadata.Clear();
      try
      {
        if (FileItem.IsRaw)
        {
          BitmapDecoder bmpDec = BitmapDecoder.Create(new Uri(FileItem.FileName), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
      
          DisplayImage = bmpDec.Frames.Single();
        }
        else
        {
          Bitmap image = (Bitmap) Image.FromFile(FileItem.FileName);
          var exif = new EXIFextractor(ref image, "n");
          if (exif["Orientation"] != null)
          {
            RotateFlipType flip = EXIFextractorEnumerator.OrientationToFlipType(exif["Orientation"].ToString());

            if (flip != RotateFlipType.RotateNoneFlipNone)  // don't flip of orientation is correct
            {
              image.RotateFlip(flip);
            }
            if (ServiceProvider.Settings.Rotate != RotateFlipType.RotateNoneFlipNone)
            {
              image.RotateFlip(ServiceProvider.Settings.Rotate);
            }
          }
          DisplayImage = BitmapSourceConvert.ToBitmapSource(image);
          image.Dispose();
          Thread threadPhoto = new Thread(GetAdditionalData);
          threadPhoto.Start();
        }
      }
      catch (Exception exception)
      {
        Log.Error(exception);
      }
      if (BitmapLoaded != null)
        BitmapLoaded(this);
      return res;
    }

    private void GetAdditionalData()
    {
      try
      {
        using (Bitmap bmp = new Bitmap(FileItem.FileName))
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
        Log.Error(ex);
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

    public void OnBitmapLoaded()
    {
      if (BitmapLoaded != null)
        BitmapLoaded(this);
    }


    public void SetFileItem(FileItem item)
    {
      FileItem = item;
      IsLoaded = false;
      if (DisplayImage == null)
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
