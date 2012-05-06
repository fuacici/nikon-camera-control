using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CameraControl.Classes
{
  public class BraketingClass : BaseFieldClass
  {
    private bool isBusy;
    private int index = 0;

    private AsyncObservableCollection<string> _exposureValues;
    public AsyncObservableCollection<string> ExposureValues
    {
      get { return _exposureValues; }
      set
      {
        _exposureValues = value;
        NotifyPropertyChanged("ExposureValues");
      }
    }

    public BraketingClass()
    {
      isBusy = false;
      ExposureValues = new AsyncObservableCollection<string>();
      ExposureValues.Add("-1");
      ExposureValues.Add("0");
      ExposureValues.Add("+1");
    }

    public void TakePhoto()
    {
      if (ExposureValues.Count == 0)
        return;
      if (!isBusy)
      {
        ServiceProvider.Settings.Manager.PhotoTakenDone += Manager_PhotoTakenDone;
        isBusy = true;
      }
      ServiceProvider.DeviceManager.SelectedCameraDevice.ExposureCompensation.SetValue(ExposureValues[index]);
      ServiceProvider.DeviceManager.SelectedCameraDevice.TakePicture();
      index++;
    }

    void Manager_PhotoTakenDone(WIA.Item imageFile)
    {
      Thread.Sleep(200);
      if(index<ExposureValues.Count)
        TakePhoto();
      else
      {
        isBusy = false;
        ServiceProvider.Settings.Manager.PhotoTakenDone -= Manager_PhotoTakenDone;
      }
    }

  }
}
