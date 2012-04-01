using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CameraControl.Classes
{
  public class FileItem
  {
    public string FileName { get; set; }
    public string Name { get; set; }

    public FileItem(string file)
    {
      FileName = file;
      Name = Path.GetFileName(file);
    }
  }
}
