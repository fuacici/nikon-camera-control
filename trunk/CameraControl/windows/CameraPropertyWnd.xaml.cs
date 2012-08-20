using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CameraControl.Classes;
using CameraControl.Devices;
using CameraControl.Interfaces;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for CameraPropertyWnd.xaml
  /// </summary>
  public partial class CameraPropertyWnd : Window, IWindow, INotifyPropertyChanged
  {
    private CameraProperty _cameraProperty;
    public CameraProperty CameraProperty
    {
      get { return _cameraProperty; }
      set
      {
        _cameraProperty = value;
        NotifyPropertyChanged("CameraProperty");
      }
    }

    public CameraPropertyWnd()
    {
      InitializeComponent();
    }

    #region Implementation of IWindow

    public void ExecuteCommand(string cmd, object param)
    {
      switch (cmd)
      {
        case WindowsCmdConsts.CameraPropertyWnd_Show:
          Dispatcher.Invoke(new Action(delegate
          {
            Show();
            Activate();
            Topmost = true;
            Topmost = false;
            Focus();
          }));
          ICameraDevice cameraDevice = param as ICameraDevice;
          CameraProperty=new CameraProperty(){SerialNumber = cameraDevice.SerialNumber};
          CameraProperty.BeginEdit();
          break;
        case WindowsCmdConsts.CameraPropertyWnd_Hide:
          Hide();
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

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      if (IsVisible)
      {
        e.Cancel = true;
        ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.CameraPropertyWnd_Hide);
      }
    }

    #region Implementation of INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;
    public virtual void NotifyPropertyChanged(String info)
    {
      if (PropertyChanged != null)
      {
        PropertyChanged(this, new PropertyChangedEventArgs(info));
      }
    }

    #endregion

    private void btn_save_Click(object sender, RoutedEventArgs e)
    {
      CameraProperty.EndEdit();
      ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.CameraPropertyWnd_Hide);
    }

    private void btn_cancel_Click(object sender, RoutedEventArgs e)
    {
      CameraProperty.CancelEdit();
      ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.CameraPropertyWnd_Hide);
    }
  }
}
