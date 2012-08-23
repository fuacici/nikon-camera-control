using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using CameraControl.Devices;

namespace CameraControl.Classes
{
  public class CameraPreset:BaseFieldClass
  {
    public string Name { get; set; }
    
    public List<ValuePair> Values { get; set; }

    public CameraPreset()
    {
      Values = new List<ValuePair>();
    }

    public void Get(ICameraDevice camera)
    {
      Add(GetFrom(camera.CompressionSetting, "CompressionSetting"));
      Add(GetFrom(camera.ExposureCompensation, "ExposureCompensation"));
      Add(GetFrom(camera.ExposureMeteringMode, "ExposureMeteringMode"));
      Add(GetFrom(camera.FNumber, "FNumber"));
      Add(GetFrom(camera.IsoNumber, "IsoNumber"));
      Add(GetFrom(camera.ShutterSpeed, "ShutterSpeed"));
      Add(GetFrom(camera.WhiteBalance, "WhiteBalance"));
    }

    public void Set(ICameraDevice camera)
    {
      SetTo(camera.CompressionSetting, "CompressionSetting");
      SetTo(camera.ExposureCompensation, "ExposureCompensation");
      SetTo(camera.ExposureMeteringMode, "ExposureMeteringMode");
      SetTo(camera.FNumber, "FNumber");
      SetTo(camera.IsoNumber, "IsoNumber");
      SetTo(camera.ShutterSpeed, "ShutterSpeed");
      SetTo(camera.WhiteBalance, "WhiteBalance");
    }

    public void SetTo(PropertyValue<int> value, string name)
    {
      foreach (ValuePair valuePair in Values)
      {
        if (valuePair.Name == name && value.IsEnabled)
        {
          value.SetValue(valuePair.Value);
          return;
        }
      }
    }

    public void SetTo(PropertyValue<long> value, string name)
    {
      foreach (ValuePair valuePair in Values)
      {
        if (valuePair.Name == name && value.IsEnabled)
        {
          value.SetValue(valuePair.Value);
          return;
        }
      }
    }



    private ValuePair GetFrom(PropertyValue<int> value, string name )
    {
      return new ValuePair() {Name = name, IsDisabled = value.IsEnabled, Value = value.Value};
    }

    private ValuePair GetFrom(PropertyValue<long> value, string name)
    {
      return new ValuePair() { Name = name, IsDisabled = value.IsEnabled, Value = value.Value };
    }

    public void Add(ValuePair pair)
    {
      foreach (ValuePair valuePair in Values)
      {
        if(pair.Name==valuePair.Name)
        {
          valuePair.Value = pair.Value;
          valuePair.IsDisabled = pair.IsDisabled;
          return;
        }
      }
      Values.Add(pair);
    }

    public void Save(string filename)
    {
      XmlSerializer serializer = new XmlSerializer(typeof(CameraPreset));
      // Create a FileStream to write with.

      Stream writer = new FileStream(filename, FileMode.Create);
      // Serialize the object, and close the TextWriter
      serializer.Serialize(writer, this);
      writer.Close(); 
     
    }

    static public CameraPreset Load(string filename)
    {
      CameraPreset cameraPreset = new CameraPreset();
      if (File.Exists(filename))
      {
        XmlSerializer mySerializer =
          new XmlSerializer(typeof (CameraPreset));
        FileStream myFileStream = new FileStream(filename, FileMode.Open);
        cameraPreset = (CameraPreset) mySerializer.Deserialize(myFileStream);
        myFileStream.Close();
      }
      return cameraPreset;
    }


  }
}
