using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using CameraControl.Devices;

namespace CameraControl.Core.Classes
{
  public enum FocusPointType
  {
    Square,
    HRectangle,
    VRectangle
  }

  public class FocusPointDefinition
  {
    public double XRat { get; set; }
    public double YRat { get; set; }
    public FocusPointType FocusPointType { get; set; }
  }

  public class Exiv2Data
  {
    public string Tag { get; set; }
    public string Type { get; set; }
    public string Length { get; set; }
    public string Value { get; set; }
   

    public override string ToString()
    {
      return Tag + "|" + Type + "|" + Length + "|" + Value;
    }
  }

  public class Exiv2Helper
  {
    public Dictionary<string, Exiv2Data> Tags { get; set; }
    public List<Rect> Focuspoints { get; set; }
    public Dictionary<string, FocusPointDefinition> FocusPoints11 { get; set; }
    public int Width;
    public int Height;

    public Exiv2Helper()
    {
      Tags = new Dictionary<string, Exiv2Data>();
      Focuspoints = new List<Rect>();
      FocusPoints11 = new Dictionary<string, FocusPointDefinition>
                        {
                          {"1", new FocusPointDefinition(){FocusPointType = FocusPointType.Square,XRat =  0.50, YRat =  0.50}},
                          {"2", new FocusPointDefinition(){FocusPointType = FocusPointType.VRectangle,XRat =  0.50, YRat = 0.28}},
                          {"3", new FocusPointDefinition(){FocusPointType = FocusPointType.VRectangle,XRat =  0.50, YRat =  0.72}},
                          {"4", new FocusPointDefinition(){FocusPointType = FocusPointType.HRectangle,XRat =  0.31, YRat =  0.50}},
                          {"5", new FocusPointDefinition(){FocusPointType = FocusPointType.HRectangle,XRat =  0.31, YRat =  0.36}},
                          {"6", new FocusPointDefinition(){FocusPointType = FocusPointType.HRectangle,XRat = 0.31, YRat =  0.62}},
                          {"7", new FocusPointDefinition(){FocusPointType = FocusPointType.HRectangle,XRat = 0.21, YRat =  0.50}},
                          {"8", new FocusPointDefinition(){FocusPointType = FocusPointType.HRectangle,XRat = 0.67, YRat =  0.50}},
                          {"9", new FocusPointDefinition(){FocusPointType = FocusPointType.HRectangle,XRat =  0.67, YRat =  0.36}},
                          {"10", new FocusPointDefinition(){FocusPointType = FocusPointType.HRectangle,XRat =  0.67, YRat = 0.62}},
                          {"11", new FocusPointDefinition(){FocusPointType = FocusPointType.HRectangle,XRat =  0.80, YRat = 0.50}},
                        };
    }

    public void Load(string filename)
    {
      var startInfo = new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Tools", "exiv2.exe"))
      {
        Arguments ="\""+ filename+"\"" + " -p a",
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true,
        WindowStyle = ProcessWindowStyle.Minimized
      };

      var process = Process.Start(startInfo);
      process.OutputDataReceived += new DataReceivedEventHandler(process_OutputDataReceived);
      process.BeginOutputReadLine();
      //string outstr = process.StandardOutput.ReadToEnd();

      process.WaitForExit();
      if (Tags.ContainsKey("Exif.Photo.PixelXDimension"))
      {
        int.TryParse(Tags["Exif.Photo.PixelXDimension"].Value, out Width);
        int.TryParse(Tags["Exif.Photo.PixelYDimension"].Value, out Height);
      }
      Focuspoints = new List<Rect>();
      if (Tags.ContainsKey("Exif.NikonAf2.ContrastDetectAF"))
      {
        if (Tags["Exif.NikonAf2.ContrastDetectAF"].Value == "On")
        {
          int x = ToSize(Tags["Exif.NikonAf2.AFAreaXPosition"].Value);
          int y = ToSize(Tags["Exif.NikonAf2.AFAreaYPosition"].Value);
          int w = ToSize(Tags["Exif.NikonAf2.AFAreaWidth"].Value);
          int h = ToSize(Tags["Exif.NikonAf2.AFAreaHeight"].Value);
          Focuspoints.Add(new Rect(x - (w/2), y - (h/2), w, h));
        }
      }
      if (Tags["Exif.NikonAf2.PhaseDetectAF"].Value == "On (11-point)")
      {
        if (Tags["Exif.NikonAf2.AFPointsUsed"].Value.Contains(" "))
        {
          byte focuspoints = 0;
          if (byte.TryParse(Tags["Exif.NikonAf2.AFPointsUsed"].Value.Split(' ')[0], out focuspoints))
          {
            for (var i = 1; i < 12; i++)
            {
              if (StaticHelper.GetBit(focuspoints,i-1))
                Focuspoints.Add(ToRect(Width, Height, FocusPoints11[i.ToString()]));
            }
          }
        }
      }


      //StaticHelper.GetBit
      return ;
    }

    void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
      if (e.Data != null)
        Tags.Add(e.Data.Substring(0, 45).Trim(), new Exiv2Data() { Tag = e.Data.Substring(0, 45).Trim(), Type = e.Data.Substring(45, 11), Length = e.Data.Substring(45 + 11, 4), Value = e.Data.Substring( 45+11+4).Trim() });
    }

    private int ToSize(string s)
    {
      int val = 0;
      if (int.TryParse(s, out val))
      {
        byte[] bytval = BitConverter.GetBytes(val);
        return BitConverter.ToInt16(new[] {bytval[1], bytval[0]},0);
      }
      return 0;
    }

    private Rect ToRect(int w,int h, FocusPointDefinition definition )
    {
      switch (definition.FocusPointType)
      {
        case FocusPointType.Square:
          return new Rect(w*definition.XRat - 150, h*definition.YRat - 100, 300, 200);
          break;
        case FocusPointType.VRectangle:
          return new Rect(w * definition.XRat - 150, h * definition.YRat - 50, 300, 100);
          break;
        case FocusPointType.HRectangle:
          return new Rect(w * definition.XRat - 50, h * definition.YRat - 150, 100, 300);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      return new Rect(); 
    }
// live view in focus 
//Exif.NikonAf2.Version                        Undefined   4  1.00
//Exif.NikonAf2.ContrastDetectAF               Byte        1  On
//Exif.NikonAf2.AFAreaMode                     Byte        1  2
//Exif.NikonAf2.PhaseDetectAF                  Byte        1  Off
//Exif.NikonAf2.PrimaryAFPoint                 Byte        1  0
//Exif.NikonAf2.AFPointsUsed                   Byte        7  0 0 0 0 0 0 0
//Exif.NikonAf2.AFImageWidth                   Short       1  16403
//Exif.NikonAf2.AFImageHeight                  Short       1  49164
//Exif.NikonAf2.AFAreaXPosition                Short       1  13835
//Exif.NikonAf2.AFAreaYPosition                Short       1  61953
//Exif.NikonAf2.AFAreaWidth                    Short       1  12291
//Exif.NikonAf2.AFAreaHeight                   Short       1  40962
//Exif.NikonAf2.ContrastDetectAFInFocus        Short       1  1

// no liveview 3 point focus
//Exif.NikonAf2.ContrastDetectAF               Byte        1  Off
//Exif.NikonAf2.AFAreaMode                     Byte        1  8
//Exif.NikonAf2.PhaseDetectAF                  Byte        1  On (11-point)
//Exif.NikonAf2.PrimaryAFPoint                 Byte        1  1
//Exif.NikonAf2.AFPointsUsed                   Byte        7  35 0 0 0 0 0 0
//Exif.NikonAf2.AFImageWidth                   Short       1  0
//Exif.NikonAf2.AFImageHeight                  Short       1  0
//Exif.NikonAf2.AFAreaXPosition                Short       1  0
//Exif.NikonAf2.AFAreaYPosition                Short       1  0
//Exif.NikonAf2.AFAreaWidth                    Short       1  0
//Exif.NikonAf2.AFAreaHeight                   Short       1  0
//Exif.NikonAf2.ContrastDetectAFInFocus        Short       1  0
  }
}
