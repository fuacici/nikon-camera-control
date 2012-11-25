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
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;
using CameraControl.Devices;
using CameraControl.Devices.Classes;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for DownloadPhotosWnd.xaml
  /// </summary>
  public partial class DownloadPhotosWnd : INotifyPropertyChanged, IWindow
  {
    private ICameraDevice _cameraDevice;
    public ICameraDevice CameraDevice
    {
      get { return _cameraDevice; }
      set
      {
        _cameraDevice = value;
        NotifyPropertyChanged("CameraDevice");
      }
    }

    private AsyncObservableCollection<FileItem> _items;
    public AsyncObservableCollection<FileItem> Items
    {
      get { return _items; }
      set
      {
        _items = value;
        NotifyPropertyChanged("Items");
      }
    }

    public DownloadPhotosWnd()
    {
      InitializeComponent();
      Items = new AsyncObservableCollection<FileItem>();
    }

    private void btn_help_Click(object sender, RoutedEventArgs e)
    {
      HelpProvider.Run(HelpSections.DownloadPhotos);
    }

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

    #region Implementation of IWindow

    public void ExecuteCommand(string cmd, object param)
    {
      switch (cmd)
      {
        case WindowsCmdConsts.DownloadPhotosWnd_Show:
          Dispatcher.Invoke(new Action(delegate
                                         {
                                           CameraDevice = param as ICameraDevice;
                                           if (param == null)
                                             return;
                                           Show();
                                           Activate();
                                           Topmost = true;
                                           Topmost = false;
                                           Focus();
                                           PopulateImageList();
                                         }));
          break;
        case WindowsCmdConsts.DownloadPhotosWnd_Hide:
          Hide();
          break;
        case CmdConsts.All_Close:
          Dispatcher.Invoke(new Action(delegate
          {
            Hide();
            Close();
          }));
          break;
      } 
    }

    #endregion

    private void MetroWindow_Closing(object sender, CancelEventArgs e)
    {
      if (IsVisible)
      {
        e.Cancel = true;
        ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.DownloadPhotosWnd_Hide);
      }
    }

    private void PopulateImageList()
    {
      Items.Clear();
      var images = CameraDevice.GetObjects(null);
      foreach (DeviceObject deviceObject in images)
      {
        Items.Add(new FileItem(deviceObject));
      }
    }

  }
}
