using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;

namespace CameraControl.Classes
{
  public class Settings: BaseFieldClass
  {
    private string AppName = "NikonCameraControl";
    private string ConfigFile = "";
   
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

    [XmlIgnore]
    private Visibility _imageLoading;
    
    [XmlIgnore]
    public ObservableCollection<VideoType> VideoTypes { get; set; }

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

    private bool _disableNativeDrivers;
    public bool DisableNativeDrivers
    {
      get { return _disableNativeDrivers; }
      set
      {
        _disableNativeDrivers = value;
        NotifyPropertyChanged("DisableNativeDrivers");
      }
    }


    private string _currentTheme;
    public string CurrentTheme
    {
      get { return _currentTheme; }
      set
      {
        _currentTheme = value;
        NotifyPropertyChanged("CurrentTheme");
      }
    }

    private int _liveViewFreezeTimeOut;
    public int LiveViewFreezeTimeOut
    {
      get { return _liveViewFreezeTimeOut; }
      set
      {
        _liveViewFreezeTimeOut = value;
        NotifyPropertyChanged("LiveViewFreezeTimeOut");
      }
    }

    private bool _previewLiveViewImage;
    public bool PreviewLiveViewImage
    {
      get { return _previewLiveViewImage; }
      set
      {
        _previewLiveViewImage = value;
        NotifyPropertyChanged("PreviewLiveViewImage");
      }
    }

    public DateTime LastUpdateCheckDate { get; set; }

    private bool _triggerKeyAlt;
    public bool TriggerKeyAlt
    {
      get { return _triggerKeyAlt; }
      set
      {
        _triggerKeyAlt = value;
        NotifyPropertyChanged("TriggerKeyAlt");
      }
    }

    private bool _triggerKeyCtrl;

    public bool TriggerKeyCtrl
    {
      get { return _triggerKeyCtrl; }
      set
      {
        _triggerKeyCtrl = value;
        NotifyPropertyChanged("TriggerKeyCtrl");
      }
    }

    private bool _triggerKeyShift;
    public bool TriggerKeyShift
    {
      get { return _triggerKeyShift; }
      set
      {
        _triggerKeyShift = value;
        NotifyPropertyChanged("TriggerKeyShift");
      }
    }

    private Key _triggerKey;
    public Key TriggerKey
    {
      get { return _triggerKey; }
      set
      {
        _triggerKey = value;
        NotifyPropertyChanged("TriggerKey");
      }
    }

    private bool _usetriggerKey;
    public bool UseTriggerKey
    {
      get { return _usetriggerKey; }
      set
      {
        _usetriggerKey = value;
        NotifyPropertyChanged("UseTriggerKey");
      }
    }

    private bool _useWebserver;
    public bool UseWebserver
    {
      get { return _useWebserver; }
      set
      {
        _useWebserver = value;
        NotifyPropertyChanged("UseWebserver");
      }
    }

    private int _webserverPort;
    public int WebserverPort
    {
      get { return _webserverPort; }
      set
      {
        _webserverPort = value;
        NotifyPropertyChanged("WebserverPort");
        NotifyPropertyChanged("Webaddress");
      }
    }

    private bool _playSound;
    public bool PlaySound
    {
      get { return _playSound; }
      set
      {
        _playSound = value;
        NotifyPropertyChanged("PlaySound");
      }
    }

    public string Webaddress
    {
      get
      {
        var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
        var ip = (
                   from addr in hostEntry.AddressList
                   where addr.AddressFamily.ToString() == "InterNetwork"
                   select addr.ToString()
                 ).FirstOrDefault();
        return string.Format("http://{0}:{1}", ip, WebserverPort);
      }
    }

    private bool _preview;
    /// <summary>
    /// preview in full screen
    /// </summary>
    public bool Preview
    {
      get { return _preview; }
      set
      {
        _preview = value;
        NotifyPropertyChanged("Preview");
      }
    }

    private int _previewSeconds;
    public int PreviewSeconds
    {
      get { return _previewSeconds; }
      set
      {
        _previewSeconds = value;
        NotifyPropertyChanged("PreviewSeconds");
      }
    }

    private bool _autoPreview;
    public bool AutoPreview
    {
      get { return _autoPreview; }
      set
      {
        _autoPreview = value;
        NotifyPropertyChanged("AutoPreview");
      }
    }

    private string _systemMessage;
    [XmlIgnore]
    public string SystemMessage
    {
      get { return _systemMessage; }
      set
      {
        _systemMessage = value;
        NotifyPropertyChanged("SystemMessage");
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
      CurrentTheme = "ExpressionDark";
      SystemMessage = "";
      DisableNativeDrivers = false;
      AutoPreview = true;
      VideoTypes = new ObservableCollection<VideoType>
                     {
                       new VideoType("HD 1080 16:9", 1920, 1080),
                       new VideoType("UXGA 4:3", 1600, 1200),
                       new VideoType("HD 720 16:9", 1280, 720),
                       new VideoType("Super VGA 4:3", 800, 600),
                     };
      LastUpdateCheckDate = DateTime.MinValue;
      UseTriggerKey = false;
      UseWebserver = false;
      WebserverPort = 5513;
      Preview = false;
      PreviewSeconds = 3;
      LiveViewFreezeTimeOut = 3;
      PreviewLiveViewImage = true;
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
       if(session.SupportedExtensions.Contains(Path.GetExtension(file).ToLower()))
       {
         if (!session.ContainFile(file))
           session.AddFile(file);
       }
      }
      session.Files=new AsyncObservableCollection<FileItem>(session.Files.OrderBy(x => x.FileDate));
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
