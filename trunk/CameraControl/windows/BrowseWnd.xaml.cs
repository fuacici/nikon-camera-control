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
using CameraControl.Devices.Classes;
using CameraControl.Interfaces;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for BrowseWnd.xaml
  /// </summary>
  public partial class BrowseWnd : Window, INotifyPropertyChanged,IWindow 
  {
    private PhotoSession _selectedPhotoSession;
    public PhotoSession SelectedPhotoSession
    {
      get { return _selectedPhotoSession; }
      set
      {
        _selectedPhotoSession = value;
        NotifyPropertyChanged("SelectedPhotoSession");
      }
    }

    public BrowseWnd()
    {
      InitializeComponent();
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
        case WindowsCmdConsts.BrowseWnd_Show:
          Dispatcher.Invoke(new Action(delegate
                                         {
                                           SelectedPhotoSession = ServiceProvider.Settings.DefaultSession;
                                           Show();
                                           Activate();
                                           Topmost = true;
                                           Topmost = false;
                                           Focus();
                                         }));
          break;
        case WindowsCmdConsts.BrowseWnd_Hide:
          {
            Dispatcher.Invoke(new Action(Hide));
          }
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

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      if (IsVisible)
      {
        e.Cancel = true;
        ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.BrowseWnd_Hide);
      }
    }

    private void lst_profiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (lst_profiles.SelectedItem != null)
      {
        ServiceProvider.Settings.DefaultSession = SelectedPhotoSession;
        ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.BrowseWnd_Hide);
      }
    }

  }
}
