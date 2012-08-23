using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using CameraControl.Devices;

namespace CameraControl.Classes
{
  public class CameraPreset
  {
    public string Name { get; set; }
    
    public List<ValuePair> Values { get; set; }

    public CameraPreset()
    {
      Values = new List<ValuePair>();
    }

    public void Get(ICameraDevice camera)
    {
      
    }

    public void Set(ICameraDevice camera)
    {

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
