using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace CameraControl.Core.Classes
{

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

    public Exiv2Helper()
    {
      Tags = new Dictionary<string, Exiv2Data>();
      Focuspoints=new List<Rect>();
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
      Focuspoints=new List<Rect>();
      if (Tags.ContainsKey("Exif.NikonAf2.ContrastDetectAF"))
      {
        if(Tags["Exif.NikonAf2.ContrastDetectAF"].Value=="On")
        {
          int x = ToSize(Tags["Exif.NikonAf2.AFAreaXPosition"].Value);
          int y = ToSize(Tags["Exif.NikonAf2.AFAreaYPosition"].Value);
          int w = ToSize(Tags["Exif.NikonAf2.AFAreaWidth"].Value);
          int h = ToSize(Tags["Exif.NikonAf2.AFAreaHeight"].Value);
          Focuspoints.Add(new Rect(x-(w/2),y-(h/2),w,h));
        }
      }
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

  }
}
