using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CameraControl.Classes;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;
using CameraControl.Core.Wpf;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using CameraControl.Translation;
using HelpProvider = CameraControl.Classes.HelpProvider;
using MessageBox = System.Windows.Forms.MessageBox;
using Path = System.IO.Path;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for DownloadPhotosWnd.xaml
  /// </summary>
  public partial class DownloadPhotosWnd : INotifyPropertyChanged, IWindow
  {
    private ProgressWindow dlg = new ProgressWindow();

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
                                           if (dlg.IsVisible)
                                             return;
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

    private void btn_download_Click(object sender, RoutedEventArgs e)
    {
      if (chk_delete.IsChecked == true && MessageBox.Show(TranslationStrings.LabelAskForDelete, "", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
        return;
      dlg.Show();
      ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.DownloadPhotosWnd_Hide);
      Thread thread = new Thread(TransferFiles); 
      thread.Start();
    }

    void TransferFiles()
    {
      AsyncObservableCollection<FileItem> itemstoExport = new AsyncObservableCollection<FileItem>(Items.Where(x => x.IsChecked));
      dlg.MaxValue = itemstoExport.Count;
      dlg.Progress = 0;
      int i = 0;
      PhotoSession session = (PhotoSession)CameraDevice.AttachedPhotoSession ?? ServiceProvider.Settings.DefaultSession;
      foreach (FileItem fileItem in itemstoExport)
      {
        dlg.Label = fileItem.FileName;
        dlg.ImageSource = fileItem.Thumbnail;
        string fileName = Path.Combine(session.Folder, fileItem.FileName);
        if (File.Exists(fileName))
          fileName =
            StaticHelper.GetUniqueFilename(
              Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_", 0,
              Path.GetExtension(fileName));
        CameraDevice.TransferFile(fileItem.DeviceObject.Handle, fileName);
        // double check if file was transferred
        if(File.Exists(fileName))
        {
          CameraDevice.DeleteObject(fileItem.DeviceObject);
        }
        session.AddFile(fileName);
        i++;
        dlg.Progress = i;
      }
      dlg.Hide();
    }
  }
}
