using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using CameraControl.Devices.Classes;

namespace CameraControl.Classes
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
    public string PhotoSessionNmae
    {
      get { return _profileNmae; }
      set
      {
        _profileNmae = value;
        NotifyPropertyChanged("PhotoSessionNmae");
      }
    }

    [XmlIgnore]
    public PhotoSession PhotoSession { get; set; }


  }
}
