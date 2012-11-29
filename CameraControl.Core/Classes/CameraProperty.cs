using System.Xml.Serialization;
using CameraControl.Devices.Classes;

namespace CameraControl.Core.Classes
{
  public class CameraProperty:BaseFieldClass
  {
    private string _serialNumber;
    public string SerialNumber
    {
      get { return _serialNumber; }
      set
      {
        _serialNumber = value;
        NotifyPropertyChanged("SerialNumber");
      }
    }

    private string _deviceName;
    public string DeviceName
    {
      get { return _deviceName; }
      set
      {
        _deviceName = value;
        NotifyPropertyChanged("DeviceName");
      }
    }

    private string _profileNmae;
    public string PhotoSessionName
    {
      get { return _profileNmae; }
      set
      {
        _profileNmae = value;
        NotifyPropertyChanged("PhotoSessionName");
      }
    }

    [XmlIgnore]
    public PhotoSession PhotoSession { get; set; }

    private bool _noDownload;
    public bool NoDownload
    {
      get { return _noDownload; }
      set
      {
        _noDownload = value;
        NotifyPropertyChanged("NoDownload");
      }
    }

    public CameraProperty()
    {
      NoDownload = false;
    }

  }
}
