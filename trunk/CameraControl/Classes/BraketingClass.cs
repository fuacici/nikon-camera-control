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
    public int Index = 0;
    private string _defec = "0";

    public event EventHandler PhotoCaptured;
    public event EventHandler IsBusyChanged;
    public event EventHandler BracketingDone;

    private bool _isBusy;

    public bool IsBusy
    {
      get { return _isBusy; }
      set
      {
        _isBusy = value;
        NotifyPropertyChanged("IsBusy");
        if (IsBusyChanged != null)
          IsBusyChanged(this, new EventArgs());
      }
    }

    public int Mode { get; set; }

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

    private AsyncObservableCollection<string> _shutterValues;
    public AsyncObservableCollection<string> ShutterValues
    {
      get { return _shutterValues; }
      set
      {
        _shutterValues = value;
        NotifyPropertyChanged("ShutterValues");
      }
    }

    public BraketingClass()
    {
      IsBusy = false;
      ExposureValues = new AsyncObservableCollection<string> ();
      ShutterValues=new AsyncObservableCollection<string>();
      Mode = 0;
    }

    public void TakePhoto()
    {
      if (Mode == 0)
      {
        if (ExposureValues.Count == 0)
          return;
        Index = 0;
        try
        {
          _defec = ServiceProvider.DeviceManager.SelectedCameraDevice.ExposureCompensation.Value;
          ServiceProvider.Settings.Manager.PhotoTakenDone += Manager_PhotoTakenDone;
          IsBusy = true;
          ServiceProvider.DeviceManager.SelectedCameraDevice.ExposureCompensation.SetValue(ExposureValues[Index]);
          //Thread.Sleep(100);
          ServiceProvider.DeviceManager.SelectedCameraDevice.TakePicture();
          //Thread.Sleep(100);
          Index++;
        }
        catch (DeviceException exception)
        {
          ServiceProvider.Log.Error(exception);
          ServiceProvider.Settings.SystemMessage = exception.Message;
        }
      }
      else
      {
        if (ShutterValues.Count == 0)
          return;
        Index = 0;
        try
        {
          _defec = ServiceProvider.DeviceManager.SelectedCameraDevice.ShutterSpeed.Value;
          ServiceProvider.Settings.Manager.PhotoTakenDone += Manager_PhotoTakenDone;
          IsBusy = true;
          ServiceProvider.DeviceManager.SelectedCameraDevice.ShutterSpeed.SetValue(ShutterValues[Index]);
          //Thread.Sleep(100);
          ServiceProvider.DeviceManager.SelectedCameraDevice.TakePicture();
          //Thread.Sleep(100);
          Index++;
        }
        catch (DeviceException exception)
        {
          ServiceProvider.Log.Error(exception);
          ServiceProvider.Settings.SystemMessage = exception.Message;
        }
        
      }
    }

    private void CaptureNextPhoto()
    {
      if (Mode == 0)
      {
        try
        {
          Thread.Sleep(100);
          ServiceProvider.DeviceManager.SelectedCameraDevice.ExposureCompensation.SetValue(ExposureValues[Index]);
          Thread.Sleep(100);
          ServiceProvider.DeviceManager.SelectedCameraDevice.TakePictureNoAf();
          Thread.Sleep(100);
          Index++;
        }
        catch (DeviceException exception)
        {
          ServiceProvider.Log.Error(exception);
          ServiceProvider.Settings.SystemMessage = exception.Message;
        }
      }
      else
      {
        try
        {
          Thread.Sleep(100);
          ServiceProvider.DeviceManager.SelectedCameraDevice.ShutterSpeed.SetValue(ShutterValues[Index]);
          Thread.Sleep(100);
          ServiceProvider.DeviceManager.SelectedCameraDevice.TakePictureNoAf();
          Thread.Sleep(100);
          Index++;
        }
        catch (DeviceException exception)
        {
          ServiceProvider.Log.Error(exception);
          ServiceProvider.Settings.SystemMessage = exception.Message;
        }
      }
    }

    void Manager_PhotoTakenDone(WIA.Item imageFile)
    {
      if (!IsBusy)
        return;
      if (PhotoCaptured != null)
        PhotoCaptured(this, new EventArgs());
      if (Mode == 0)
      {
        if (Index < ExposureValues.Count)
        {
          Thread thread = new Thread(CaptureNextPhoto);
          thread.Start();
        }
        else
        {
          Stop();
        }
      }
      else
      {
        if (Index < ShutterValues.Count)
        {
          Thread thread = new Thread(CaptureNextPhoto);
          thread.Start();
        }
        else
        {
          Stop();
        }
      }
    }

    public void Stop()
    {
      IsBusy = false;
      ServiceProvider.Settings.Manager.PhotoTakenDone -= Manager_PhotoTakenDone;
      Thread thread = null;
      if (Mode == 0)
      {
        thread = new Thread(() => ServiceProvider.DeviceManager.SelectedCameraDevice.
                                    ExposureCompensation.SetValue(_defec));
      }
      else
      {
        thread = new Thread(() => ServiceProvider.DeviceManager.SelectedCameraDevice.
                                    ShutterSpeed.SetValue(_defec));
      }
      thread.Start();
      if (BracketingDone != null)
        BracketingDone(this, new EventArgs());
    }

  }
}
