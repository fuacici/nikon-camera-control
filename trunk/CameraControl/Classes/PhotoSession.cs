using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace CameraControl.Classes
{
  public class PhotoSession : BaseFieldClass
  {
    [XmlIgnore]
    public List<string> SupportedExtensions = new List<string>() {".jpg", ".nef", ".tif"};

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
        if (_folder != value)
        {
          if(!Directory.Exists(value))
          {
            Directory.CreateDirectory(value);
          }
          _systemWatcher.Path = value;
          _systemWatcher.EnableRaisingEvents = true;
        }
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

    private AsyncObservableCollection<FileItem> _files;

    [XmlIgnore]
    public AsyncObservableCollection<FileItem> Files
    {
      get { return _files; }
      set
      {
        _files = value;
        NotifyPropertyChanged("Files");
      }
    }

    private TimeLapseClass _timeLapse;
    public TimeLapseClass TimeLapse
    {
      get { return _timeLapse; }
      set
      {
        _timeLapse = value;
        NotifyPropertyChanged("TimeLapse");
      }
    }

    private BraketingClass _braketing;
    public BraketingClass Braketing
    {
      get { return _braketing; }
      set
      {
        _braketing = value;
        NotifyPropertyChanged("Braketing");
      }
    }

    public string ConfigFile { get; set; }
    private FileSystemWatcher _systemWatcher;

    public PhotoSession()
    {
      _systemWatcher = new FileSystemWatcher();
      _systemWatcher.EnableRaisingEvents = false;
      _systemWatcher.Deleted += _systemWatcher_Deleted;
      _systemWatcher.Created += new FileSystemEventHandler(_systemWatcher_Created);

      Name = "Default";
      Braketing = new BraketingClass();
      Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), Name);
      Files = new AsyncObservableCollection<FileItem>();
      FileNameTemplate = "DSC_$C";
      TimeLapse = new TimeLapseClass();
      if (ServiceProvider.Settings!=null && ServiceProvider.Settings.VideoTypes.Count > 0)
        TimeLapse.VideoType = ServiceProvider.Settings.VideoTypes[0];
      TimeLapse.OutputFIleName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                                              Name + ".avi");
    }

    void _systemWatcher_Created(object sender, FileSystemEventArgs e)
    {
      try
      {
        //AddFile(e.FullPath);
      }
      catch (Exception exception)
      {
        ServiceProvider.Log.Error("Add file error", exception);
      }
    }

    void _systemWatcher_Deleted(object sender, FileSystemEventArgs e)
    {
      FileItem deletedItem = null;
      lock (this)
      {
        foreach (FileItem fileItem in Files)
        {
          if (fileItem.FileName == e.FullPath)
            deletedItem = fileItem;
        }
      }
      try
      {
        if (deletedItem != null)
          Files.Remove(deletedItem);
          //Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => Files.Remove(
          //  deletedItem)));
      }
      catch (Exception exception)
      {


      }
    }

    public string GetNextFileName(string ext)
    {
      if (string.IsNullOrEmpty(ext))
        ext = "nef";
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
      if(ServiceProvider.DeviceManager.SelectedCameraDevice.ExposureCompensation.Value!="0")
      {
        res = res.Replace("$E", ServiceProvider.DeviceManager.SelectedCameraDevice.ExposureCompensation.Value);
      }
      else
      {
        res = res.Replace("$E", "");
      }
      return res;
    }

    public FileItem AddFile(string fileName)
    {
      FileItem oitem = GetFile(fileName);
      if (oitem != null)
        return oitem;
      FileItem item = new FileItem(fileName);
      Files.Add(item);
      return item;
    }

    public bool ContainFile(string fileName)
    {
      foreach (FileItem fileItem in Files)
      {
        if (fileItem.FileName.ToUpper() == fileName.ToUpper())
          return true;
      }
      return false;
    }

    public FileItem GetFile(string fileName)
    {
      foreach (FileItem fileItem in Files)
      {
        if (fileItem.FileName.ToUpper() == fileName.ToUpper())
          return fileItem;
      }
      return null;
    }

    public override string ToString()
    {
      return Name;
    }

    public AsyncObservableCollection<FileItem> GetSelectedFiles()
    {
      AsyncObservableCollection<FileItem> list = new AsyncObservableCollection<FileItem>();
      foreach (FileItem fileItem in Files)
      {
        if (fileItem.IsChecked)
          list.Add(fileItem);
      }
      return list;
    }

    public void SelectAll()
    {
      foreach (FileItem fileItem in Files)
      {
        fileItem.IsChecked = true;
      }
    }

    public void SelectNone()
    {
      foreach (FileItem fileItem in Files)
      {
        fileItem.IsChecked = false;
      }
    }


  }
}
