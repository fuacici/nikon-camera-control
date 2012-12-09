using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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

    public Exiv2Helper()
    {
      Tags = new Dictionary<string, Exiv2Data>();
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

      return ;
    }

    void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
      if (e.Data != null)
        Tags.Add(e.Data.Substring(0, 45), new Exiv2Data() { Tag = e.Data.Substring(0, 45), Type = e.Data.Substring(45, 11), Length = e.Data.Substring(45 + 11, 4), Value = e.Data.Substring( 45+11+4) });
    }
  }
}
