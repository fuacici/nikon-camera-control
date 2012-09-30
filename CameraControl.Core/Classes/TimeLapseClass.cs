using System;
using System.Timers;
using System.Xml.Serialization;
using CameraControl.Core.Devices.Classes;

namespace CameraControl.Core.Classes
{
  public class TimeLapseClass : BaseFieldClass
  {
    private Timer _timer = new Timer(1000);
    private int _timecounter = 0;

    private int _period;
    public int Period
    {
      get { return _period; }
      set
      {
        _period = value;
        _timePeriod = new DateTime().AddSeconds(Period * NumberOfPhotos);
        NotifyPropertyChanged("TimePeriod");
        NotifyPropertyChanged("Period");
      }
    }

    private int _numberOfPhotos;
    public int NumberOfPhotos
    {
      get { return _numberOfPhotos; }
      set
      {
        _numberOfPhotos = value;
        _timePeriod = new DateTime().AddSeconds(Period*NumberOfPhotos);
        if (Fps > 0)
        {
          _movieLength = new DateTime().AddSeconds(_numberOfPhotos/_fps);
          NotifyPropertyChanged("MovieLength");
        }
        NotifyPropertyChanged("TimePeriod");
        NotifyPropertyChanged("NumberOfPhotos");
      }
    }

    private int _photosTaken;

    private VideoType _videoType;
    public VideoType VideoType
    {
      get
      {
        if (_videoType == null || _videoType.Width==0)
        {
          if (ServiceProvider.Settings != null && ServiceProvider.Settings.VideoTypes.Count > 0)
            _videoType = ServiceProvider.Settings.VideoTypes[0];
        }
        return _videoType;
      }
      set
      {
        _videoType = value;
        NotifyPropertyChanged("VideoType");
      }
    }

    private DateTime _timePeriod;
    public DateTime  TimePeriod
    {
      get { return _timePeriod; }
      set
      {
        _timePeriod = value;
        _numberOfPhotos = (int) ((_timePeriod.Ticks/TimeSpan.TicksPerSecond)/Period);
        NotifyPropertyChanged("TimePeriod");
        NotifyPropertyChanged("NumberOfPhotos");
      }
    }

    private DateTime _movieLength;
    public DateTime MovieLength
    {
      get { return _movieLength; }
      set
      {
        _movieLength = value;
        _numberOfPhotos = (int)((_movieLength.Ticks / TimeSpan.TicksPerSecond) * Fps);
        NotifyPropertyChanged("NumberOfPhotos");
        NotifyPropertyChanged("MovieLength");
      }
    }


    private string _outputFIleName;
    public string OutputFIleName
    {
      get
      {
        return _outputFIleName;
      }
      set
      {
        _outputFIleName = value;
        NotifyPropertyChanged("OutputFIleName");
      }
    }

    private int _fps;
    public int Fps
    {
      get { return _fps; }
      set
      {
        _fps = value;
        if (_fps > 0)
        {
          _movieLength = new DateTime().AddSeconds(_numberOfPhotos / _fps);
          NotifyPropertyChanged("MovieLength");
        }

        NotifyPropertyChanged("MovieLength");
        NotifyPropertyChanged("Fps");
      }
    }

    private bool _noAutofocus;
    public bool NoAutofocus
    {
      get { return _noAutofocus; }
      set
      {
        _noAutofocus = value;
        NotifyPropertyChanged("NoAutofocus");
      }
    }

    [XmlIgnore]
    public int PhotosTaken
    {
      get { return _photosTaken; }
      set
      {
        _photosTaken = value;
        NotifyPropertyChanged("PhotosTaken");
      }
    }

    private bool _isDisabled;

    [XmlIgnore]
    public bool IsDisabled
    {
      get { return _isDisabled; }
      set
      {
        _isDisabled = value;
        NotifyPropertyChanged("IsDisabled");
      }
    }

    private bool _fillImage;
    public bool FillImage
    {
      get { return _fillImage; }
      set
      {
        _fillImage = value;
        NotifyPropertyChanged("FillImage");
      }
    }

    private bool _virtualMove;
    public bool VirtualMove
    {
      get { return _virtualMove; }
      set
      {
        _virtualMove = value;
        NotifyPropertyChanged("VirtualMove");
      }
    }

    private int _movePercent;
    public int MovePercent
    {
      get { return _movePercent; }
      set
      {
        _movePercent = value;
        NotifyPropertyChanged("MovePercent");
      }
    }

    private int _moveDirection;
    public int MoveDirection
    {
      get { return _moveDirection; }
      set
      {
        _moveDirection = value;
        NotifyPropertyChanged("MoveDirection");
      }
    }

    private int _moveAlignment;
    public int MoveAlignment
    {
      get { return _moveAlignment; }
      set
      {
        _moveAlignment = value;
        NotifyPropertyChanged("MoveAlignment");
      }
    }

    public TimeLapseClass()
    {
      Period = 5;
      NumberOfPhotos = 100;
      PhotosTaken = 0;
      IsDisabled = true;
      VideoType = new VideoType("",0,0);
      Fps = 15;
      FillImage = false;
      VirtualMove = false;
      MovePercent = 10;
      MoveDirection = 0;
      _timer.Elapsed += _timer_Elapsed;
    }

    void _timer_Elapsed(object sender, ElapsedEventArgs e)
    {
      _timecounter++;
      if(_timecounter>=Period)
      {
        try
        {
          _timer.Enabled = false;
          if (NoAutofocus)
            ServiceProvider.DeviceManager.SelectedCameraDevice.CapturePhotoNoAf();
          else
            ServiceProvider.DeviceManager.SelectedCameraDevice.CapturePhoto();
          //_timer.Enabled = true;
        }
        catch (DeviceException exception)
        {
          if(exception.ErrorCode==ErrorCodes.WIA_ERROR_UNABLE_TO_FOCUS)
          {
           StaticHelper.Instance.SystemMessage = " Camera is busy retrying ....";
            _timer.Enabled = true;
          }
          else
          {
            Log.Error(exception);
           StaticHelper.Instance.SystemMessage = exception.Message;
          }
        }
        _timecounter = 0;
        PhotosTaken++;
        if (PhotosTaken >= NumberOfPhotos)
        {
          Stop();
         StaticHelper.Instance.SystemMessage = " Time lapse done !";
        }
      }
      else
      {
       StaticHelper.Instance.SystemMessage = PhotosTaken == 0
                                                   ? string.Format("Time Lapse will start in {0} second(s)", (Period - _timecounter))
                                                   : string.Format("Next Time Lapse photo will be taken in  {0} second(s). Total photos :{1}/{2}", (Period - _timecounter), PhotosTaken, NumberOfPhotos);
      }
    }

    public TimeLapseClass Copy()
    {
      TimeLapseClass timeLapseClass = new TimeLapseClass {NumberOfPhotos = this.NumberOfPhotos, Period = this.Period};
      return timeLapseClass;
    }

    public void Start()
    {
      PhotosTaken = 0;
      _timer.AutoReset = true;
      IsDisabled = false;
      ServiceProvider.DeviceManager.PhotoCaptured += DeviceManager_PhotoCaptured;
      _timer.Start();
    }

    void DeviceManager_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
    {
      if (!IsDisabled)
        _timer.Start();
    }

    public void Stop()
    {
      _timer.Stop();
      IsDisabled = true;
      ServiceProvider.DeviceManager.PhotoCaptured -= DeviceManager_PhotoCaptured;
      StaticHelper.Instance.SystemMessage = "Timelapse done";
    }

  }
}
