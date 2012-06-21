using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Xml.Serialization;
using CameraControl.Devices.Classes;

namespace CameraControl.Classes
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


    public TimeLapseClass()
    {
      Period = 5;
      NumberOfPhotos = 100;
      PhotosTaken = 0;
      IsDisabled = true;
      VideoType = new VideoType("",0,0);
      Fps = 15;
      FillImage = false;
      _timer.Elapsed += _timer_Elapsed;
    }

    void Manager_PhotoTakenDone(WIA.Item imageFile)
    {
      if (!IsDisabled)
        _timer.Start();
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
            ServiceProvider.DeviceManager.SelectedCameraDevice.TakePictureNoAf();
          else
            ServiceProvider.DeviceManager.SelectedCameraDevice.TakePicture();
          //_timer.Enabled = true;
        }
        catch (DeviceException exception)
        {
          if(exception.ErrorCode==ErrorCodes.WIA_ERROR_UNABLE_TO_FOCUS)
          {
            ServiceProvider.Settings.SystemMessage = " Camera is busy retrying ....";
            _timer.Enabled = true;
          }
          else
          {
            ServiceProvider.Log.Error(exception);
            ServiceProvider.Settings.SystemMessage = exception.Message;
          }
        }
        _timecounter = 0;
        PhotosTaken++;
        if (PhotosTaken >= NumberOfPhotos)
        {
          Stop();
          ServiceProvider.Settings.SystemMessage = " Time lapse done !";
        }
      }
      else
      {
        ServiceProvider.Settings.SystemMessage = PhotosTaken == 0
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
      ServiceProvider.Settings.Manager.PhotoTakenDone += Manager_PhotoTakenDone;
      _timer.Start();
    }

    public void Stop()
    {
      _timer.Stop();
      IsDisabled = true;
      ServiceProvider.Settings.Manager.PhotoTakenDone -= Manager_PhotoTakenDone;
      ServiceProvider.Settings.SystemMessage = "Timelapse done";
    }

  }
}
