using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CameraControl.Classes;
using CameraControl.Interfaces;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for FocusStackingWnd.xaml
  /// </summary>
  public partial class FocusStackingWnd : Window, IWindow, INotifyPropertyChanged
  {
    private bool _isBusy;
    public bool IsBusy
    {
      get { return _isBusy; }
      set
      {
        _isBusy = value;
        NotifyPropertyChanged("IsBusy");
        NotifyPropertyChanged("IsFree");
      }
    }

    public bool IsFree
    {
      get { return !_isBusy; }
    }

    private int _photoNo;
    public int PhotoNo
    {
      get { return _photoNo; }
      set
      {
        _photoNo = value;
        NotifyPropertyChanged("PhotoNo");
      }
    }

    private int _focusStep;
    public int FocusStep
    {
      get { return _focusStep; }
      set
      {
        _focusStep = value;
        NotifyPropertyChanged("FocusStep");
        PhotoNo = FocusValue / FocusStep;
      }
    }


    private int _photoCount;
    public int PhotoCount
    {
      get { return _photoCount; }
      set
      {
        _photoCount = value;
        NotifyPropertyChanged("PhotoCount");
      }
    }

    private int _focusCounter;
    public int FocusCounter
    {
      get { return _focusCounter; }
      set
      {
        _focusCounter = value;
        NotifyPropertyChanged("FocusCounter");
      }
    }

    private int _focusValue;

    public int FocusValue
    {
      get { return _focusValue; }
      set
      {
        _focusValue = value;
        PhotoNo = FocusValue/FocusStep;
        NotifyPropertyChanged("FocusValue");
      }
    }


    private bool preview = false;
    public FocusStackingWnd()
    {
      InitializeComponent();
      IsBusy = false;
      PhotoCount = 0;
      FocusStep = 15;
    }

    void Manager_PhotoTakenDone(WIA.Item imageFile)
    {
      if (PhotoCount <= PhotoNo)
      {
        Thread thread = new Thread(TakePhoto);
        thread.Start();
      }
      else
      {
        IsBusy = false;
      }
    }

    private void TakePhoto()
    {
      if (IsBusy)
      {
        PhotoCount++;
        Thread.Sleep(200);
        ServiceProvider.DeviceManager.SelectedCameraDevice.StartLiveView();
        Thread.Sleep(800);
        ServiceProvider.DeviceManager.SelectedCameraDevice.Focus(FocusStep);
        Thread.Sleep(1000);
        if (!preview)
        {
          ServiceProvider.DeviceManager.SelectedCameraDevice.TakePictureNoAf();
        }
        else
        {
          if (PhotoCount <= PhotoNo)
          {
            Thread.Sleep(1000);
            TakePhoto();
          }
          else
          {
            IsBusy = false;
          }
        }
      }
      else
      {
        ServiceProvider.DeviceManager.SelectedCameraDevice.StartLiveView();
      }
    }

    private void btn_preview_Click(object sender, RoutedEventArgs e)
    {
      PhotoCount = 0;
      IsBusy = true;
      preview = true;
      Thread thread = new Thread(TakePhoto);
      thread.Start(); 
    }

    #region Implementation of IWindow

    public void ExecuteCommand(string cmd)
    {
      switch (cmd)
      {
        case WindowsCmdConsts.FocusStackingWnd_Show:
          Dispatcher.BeginInvoke(new Action(delegate
                                              {
                                                Show();
                                                Activate();
                                                Topmost = true;
                                                Topmost = false;
                                                Focus();
                                                ServiceProvider.Settings.Manager.PhotoTakenDone += Manager_PhotoTakenDone;
                                              }));
          break;
        case WindowsCmdConsts.FocusStackingWnd_Hide:
          Dispatcher.Invoke(new Action(delegate
                                         {
                                           Hide();
                                           ServiceProvider.Settings.Manager.PhotoTakenDone -= Manager_PhotoTakenDone;
                                         }));
          break;
        case WindowsCmdConsts.All_Close:
          Dispatcher.Invoke(new Action(delegate
                                         {
                                           Hide();
                                           Close();
                                         }));
          break;
      }
    }

    #endregion

    #region Implementation of INotifyPropertyChanged

    public virtual event PropertyChangedEventHandler PropertyChanged;

    public virtual void NotifyPropertyChanged(String info)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
    }

    #endregion

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (IsVisible)
      {
        e.Cancel = true;
        ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.FocusStackingWnd_Hide);
      }
    }

    private void btn_stop_Click(object sender, RoutedEventArgs e)
    {
      IsBusy = false;
    }

    private void btn_takephoto_Click(object sender, RoutedEventArgs e)
    {
      //ServiceProvider.DeviceManager.SelectedCameraDevice.StopLiveView();
      PhotoCount = 0;
      IsBusy = true;
      preview = false;
      Thread thread = new Thread(TakePhoto);
      thread.Start(); 
    }
  }
}
