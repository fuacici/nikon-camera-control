using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using FreeImageAPI;
using FreeImageAPI.Metadata;

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

    public ObservableCollection<DictionaryItem> Metadata { get; set; }

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
      }

      HistogramBlack =ToBitmap(image);
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


      Histogram = ToBitmap(image);
    }




    public BitmapImage GetBitmap()
    {
      BitmapImage res = null;
      //Metadata.Clear();
      try
      {
        FIBITMAP dib = FreeImage.LoadEx(FileItem.FileName);
        if (FreeImage.GetFileType(FileItem.FileName, 0) == FREE_IMAGE_FORMAT.FIF_RAW)
        {
          FIBITMAP bmp = FreeImage.ToneMapping(dib, FREE_IMAGE_TMO.FITMO_REINHARD05, 0, 0); // ConvertToType(dib, FREE_IMAGE_TYPE.FIT_BITMAP, false);
          FileItem.Thumbnail = ToBitmap(FreeImage.GetBitmap(FreeImage.MakeThumbnail(dib, 255, true)));
          DisplayImage = ToBitmap(FreeImage.GetBitmap(bmp));
          CreateHistogram(bmp);
          CreateHistogramBlack(bmp);
          FreeImage.UnloadEx(ref dib);
          FreeImage.UnloadEx(ref bmp);
        }
        else
        {
          DisplayImage = BitmapSourceConvert.ToBitmapSource(FreeImage.GetBitmap(dib));
          FileItem.Thumbnail = BitmapSourceConvert.ToBitmapSource(FreeImage.GetBitmap(FreeImage.MakeThumbnail(dib, 255, true)));
          CreateHistogram(dib);
          CreateHistogramBlack(dib);
          FreeImage.UnloadEx(ref dib);
        }
        GetMetadata();
      }
      catch (Exception)
      {
      }
      return res;
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
        if (dictionaryItem.Name == tag.Description)
        {
          dictionaryItem.Value = tag.ToString();
          return;
        }
      }
      Metadata.Add(new DictionaryItem { Name = tag.Description, Value = tag.ToString()});
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

    public BitmapImage ToBitmap(Image image)
    {
      //Graphics g = Graphics.FromImage(image);
      //int w = image.Width/3;
      //int h = image.Height/3;
      //g.DrawLine(new Pen(new SolidBrush(Color.White), 2), w, 0, w, image.Height);
      //g.DrawLine(new Pen(new SolidBrush(Color.White), 2), w*2, 0, w*2, image.Height);
      //g.DrawLine(new Pen(new SolidBrush(Color.White), 2), 0, h, image.Width, h);
      //g.DrawLine(new Pen(new SolidBrush(Color.White), 2), 0, h*2, image.Width, h*2);
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

    public BitmapFile()
    {
      IsLoaded = false;
      Metadata = new ObservableCollection<DictionaryItem>();
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
