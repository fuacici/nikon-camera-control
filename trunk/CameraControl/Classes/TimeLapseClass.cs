using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Xml.Serialization;

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
      get { return _videoType; }
      set
      {
        _videoType = value;
        NotifyPropertyChanged("VideoType");
      }
    }

    private string _outputFIleName;
    public string OutputFIleName
    {
      get { return _outputFIleName; }
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

    public TimeLapseClass()
    {
      Period = 5;
      NumberOfPhotos = 100;
      PhotosTaken = 0;
      IsDisabled = true;
      VideoType = new VideoType("",0,0);
      Fps = 15;
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
          ServiceProvider.Settings.Manager.TakePicture();
          _timer.Enabled = true;
        }
        catch (Exception ex)
        {
          if (ex.Message == "Exception from HRESULT: 0x80210006")
          {
            ServiceProvider.Settings.SystemMessage = " Camera is busy retrying ....";
            return;
          }
          ServiceProvider.Settings.SystemMessage = " Error to take photo !";
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
      _timer.Start();
      _timer.AutoReset = true;
      IsDisabled = false;
    }

    public void Stop()
    {
      _timer.Stop();
      IsDisabled = true;
    }

  }
}
