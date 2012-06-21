using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CameraControl.Devices.Classes;

namespace CameraControl.Classes
{
  public class BraketingClass : BaseFieldClass
  {
    private int index = 0;
    private string _defec = "0";

    private bool _isBusy;
    public bool IsBusy
    {
      get { return _isBusy; }
      set
      {
        _isBusy = value;
        NotifyPropertyChanged("IsBusy");
      }
    }

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
      IsBusy = false;
      ExposureValues = new AsyncObservableCollection<string> ();
    }

    public void TakePhoto()
    {
      if (ExposureValues.Count == 0)
        return;
      index = 0;
      try
      {
        _defec = ServiceProvider.DeviceManager.SelectedCameraDevice.ExposureCompensation.Value;
        ServiceProvider.Settings.Manager.PhotoTakenDone += Manager_PhotoTakenDone;
        IsBusy = true;
        ServiceProvider.DeviceManager.SelectedCameraDevice.ExposureCompensation.SetValue(ExposureValues[index]);
        Thread.Sleep(100);
        ServiceProvider.DeviceManager.SelectedCameraDevice.TakePicture();
        Thread.Sleep(100);
        index++;
      }
      catch (DeviceException exception)
      {
        ServiceProvider.Log.Error(exception);
        ServiceProvider.Settings.SystemMessage = exception.Message;
      }
    }

    private void TakeNextPhoto()
    {
      try
      {
        Thread.Sleep(100);
        ServiceProvider.DeviceManager.SelectedCameraDevice.ExposureCompensation.SetValue(ExposureValues[index]);
        Thread.Sleep(100);
        ServiceProvider.DeviceManager.SelectedCameraDevice.TakePictureNoAf();
        Thread.Sleep(100);
        index++;
      }
      catch (DeviceException exception)
      {
        ServiceProvider.Log.Error(exception);
        ServiceProvider.Settings.SystemMessage = exception.Message;
      }
    }

    void Manager_PhotoTakenDone(WIA.Item imageFile)
    {
      if (!IsBusy)
        return;
      if (index < ExposureValues.Count)
      {
        Thread thread = new Thread(TakeNextPhoto);
        thread.Start();
      }
      else
      {
        IsBusy = false;
        ServiceProvider.Settings.Manager.PhotoTakenDone -= Manager_PhotoTakenDone;
        Thread thread = new Thread(new ThreadStart(delegate
                                                     {
                                                       ServiceProvider.DeviceManager.SelectedCameraDevice.
                                                         ExposureCompensation.SetValue(_defec);
                                                     }));
        thread.Start();
      }
    }

  }
}
