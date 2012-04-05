using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Serialization;

namespace CameraControl.Classes
{
  public class Settings: BaseFieldClass
  {
    private string AppName = "NikonCameraControl";
    private string ConfigFile = "";
    private List<string> supportedExtensions= new List<string>() {".jpg",".nef"};

    [XmlIgnore]
    public WIAManager Manager { get; set; }

    private PhotoSession _defaultSession;

    [XmlIgnore]
    public PhotoSession DefaultSession
    {
      get { return _defaultSession; }
      set
      {
        _defaultSession = value;
        LoadData(_defaultSession);
        NotifyPropertyChanged("DefaultSession");
      }
    }

    [XmlIgnore]
    public ObservableCollection<PhotoSession> PhotoSessions { get; set; }

    private BitmapFile _selectedBitmap;

    [XmlIgnore]
    public BitmapFile SelectedBitmap
    {
      get { return _selectedBitmap; }
      set
      {
        _selectedBitmap = value;
        NotifyPropertyChanged("DefaultSession");
      }
    }


    private Visibility _imageLoading;

    [XmlIgnore]
    public Visibility ImageLoading
    {
      get { return _imageLoading; }
      set
      {
        _imageLoading = value;
        NotifyPropertyChanged("ImageLoading");
      }
    }

    public Settings()
    {
      ConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppName,
                                "settings.xml");
      DefaultSession = new PhotoSession();
      PhotoSessions = new ObservableCollection<PhotoSession>();
      ImageLoading = Visibility.Hidden;
      SelectedBitmap = new BitmapFile();
    }

    public void Add(PhotoSession session)
    {
      Save(session);
      PhotoSessions.Add(session);
    }

    /// <summary>
    /// Load files atached to a session
    /// </summary>
    /// <param name="session"></param>
    public void LoadData(PhotoSession session)
    {
      if (session == null)
        return;
      //session.Files.Clear();
      if(!Directory.Exists(session.Folder))
      {
        Directory.CreateDirectory(session.Folder);
      }
      string[] files = Directory.GetFiles(session.Folder);
      foreach (string file in files)
      {
       if(supportedExtensions.Contains(Path.GetExtension(file).ToLower()))
       {
         if (!session.ContainFile(file))
           session.AddFile(file);
       }
      }
    }

    public void Save(PhotoSession session)
    {
      string filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), AppName,
                                     "Sessions", session.Name + ".xml");
      XmlSerializer serializer = new XmlSerializer(typeof(PhotoSession));
      // Create a FileStream to write with.

      Stream writer = new FileStream(filename, FileMode.Create);
      // Serialize the object, and close the TextWriter
      serializer.Serialize(writer, session);
      writer.Close();
    }

    public PhotoSession Load(string filename)
    {
      PhotoSession photoSession = new PhotoSession();
      if (File.Exists(filename))
      {
        XmlSerializer mySerializer =
          new XmlSerializer(typeof(PhotoSession));
        FileStream myFileStream = new FileStream(filename, FileMode.Open);
        photoSession = (PhotoSession)mySerializer.Deserialize(myFileStream);
        myFileStream.Close();
        photoSession.ConfigFile = filename;
      }
      return photoSession;
    }

    public Settings Load()
    {
      Settings settings = new Settings();
      if (!Directory.Exists(Path.GetDirectoryName(ConfigFile)))
      {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigFile));
      }
      if (File.Exists(ConfigFile))
      {
        XmlSerializer mySerializer =
          new XmlSerializer(typeof (Settings));
        FileStream myFileStream = new FileStream(ConfigFile, FileMode.Open);
        settings = (Settings) mySerializer.Deserialize(myFileStream);
        myFileStream.Close();
      }
      else
      {
        settings.Save();
      }
      return settings;
    }

    public void LoadSessionData()
    {
      string sesionFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                   AppName, "Sessions");
      if (!Directory.Exists(sesionFolder))
      {
        Directory.CreateDirectory(sesionFolder);
      }

      string[] sesions = Directory.GetFiles(sesionFolder, "*.xml");
      foreach (string sesion in sesions)
      {
        Add(Load(sesion));
      }
      if (PhotoSessions.Count > 0)
      {
        DefaultSession = PhotoSessions[0];
      }
      if (PhotoSessions.Count == 0)
      {
        Add(DefaultSession);
      }
    }

    public void Save()
    {
      XmlSerializer serializer = new XmlSerializer(typeof(Settings));
      // Create a FileStream to write with.

      Stream writer = new FileStream(ConfigFile, FileMode.Create);
      // Serialize the object, and close the TextWriter
      serializer.Serialize(writer, this);
      writer.Close();
    }
  }
}
