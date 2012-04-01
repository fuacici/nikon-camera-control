using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CameraControl.Classes
{
  public class PhotoSession : BaseFieldClass
  {
    private string _name;
    public string Name
    {
      get { return _name; }
      set
      {
        _name = value;
        NotifyPropertyChanged("Name");
      }
    }

    private string _folder;
    public string Folder
    {
      get { return _folder; }
      set
      {
        _folder = value;
        NotifyPropertyChanged("Folder");
      }
    }

    private string _fileNameTemplate;

    public string FileNameTemplate
    {
      get { return _fileNameTemplate; }
      set
      {
        _fileNameTemplate = value;
        NotifyPropertyChanged("FileNameTemplate");
      }
    }

    private int _counter;
    public int Counter
    {
      get { return _counter; }
      set
      {
        _counter = value;
        NotifyPropertyChanged("Counter");
      }
    }

    [XmlIgnore]
    public List<FileItem> Files { get; set; }

    public string ConfigFile { get; set; }

    public PhotoSession()
    {
      Name = "Default";
      Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), Name);
      Files = new List<FileItem>();
      FileNameTemplate = "DSC_$C";
    }

    public string GetNextFileName(string ext)
    {
      Counter++;
      string fileName = Path.Combine(Folder, FormatFileName() + "." + ext);
      if (File.Exists(fileName))
        return GetNextFileName(ext);
      return fileName;
    }

    private string FormatFileName()
    {
      string res = FileNameTemplate;
      if (!res.Contains("$C"))
        res += "$C";
      res = res.Replace("$C", Counter.ToString("00000"));
      res = res.Replace("$N", Name.Trim());
      return res;
    }

    public void AddFile(string fileName)
    {
      FileItem item=new FileItem();
      Files.Add(item);
    }

    public override string ToString()
    {
      return Name;
    }

  }
}
