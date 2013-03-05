using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;
using CameraControl.Core.Translation;
using CameraControl.Core.Wpf;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using MahApps.Metro.Controls;
using MessageBox = System.Windows.Forms.MessageBox;
//using CameraControl.Classes;
//using CameraControl.Translation;
//using HelpProvider = CameraControl.Classes.HelpProvider;

namespace FBFX.Plugin.Windows
{

  /// <summary>
  /// Interaction logic for DownloadPhotosWnd.xaml
  /// </summary>
  public partial class FBFXDownloadPhotosWnd : INotifyPropertyChanged, IWindow
  {
    private bool delete;
    private bool format;
    private ProgressWindow dlg = new ProgressWindow();
    private Dictionary<ICameraDevice, int> _timeDif = new Dictionary<ICameraDevice, int>();
    private Dictionary<ICameraDevice, AsyncObservableCollection<FileItem>> _itembycamera = new Dictionary<ICameraDevice, AsyncObservableCollection<FileItem>>();
    private List<DateTime> _timeTable = new List<DateTime>();

    public ObservableCollection<DownloadableItems> Groups { get; set; }

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

    public FBFXDownloadPhotosWnd()
    {
      Groups = new ObservableCollection<DownloadableItems>();
      Items = new AsyncObservableCollection<FileItem>();
      InitializeComponent();
    }

    private void btn_help_Click(object sender, RoutedEventArgs e)
    {
      //HelpProvider.Run(HelpSections.DownloadPhotos);
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
                                           Title = TranslationStrings.DownloadWindowTitle + "-" +
                                                   ServiceProvider.Settings.CameraProperties.Get(CameraDevice).
                                                     DeviceName;
                                           if (param == null)
                                             return;
                                           Show();
                                           Activate();
                                           Topmost = true;
                                           Topmost = false;
                                           Focus();
                                           dlg.Show();
                                           Items.Clear();
                                           Thread thread = new Thread(PopulateImageList);
                                           thread.Start();
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
      if (ServiceProvider.DeviceManager.ConnectedDevices.Count == 0)
        return;
      _timeDif.Clear();
      _itembycamera.Clear();
      Items.Clear();
      _timeTable.Clear();
      int threshold = 0;
      bool checkset = false;
      Dispatcher.Invoke(new Action(() => int.TryParse(lbl_thresh.Text, out threshold)));
      Dispatcher.Invoke(new Action(() => checkset = chk_set.IsChecked == true));
      foreach (ICameraDevice cameraDevice in ServiceProvider.DeviceManager.ConnectedDevices)
      {
        CameraProperty property = ServiceProvider.Settings.CameraProperties.Get(cameraDevice);
        cameraDevice.DisplayName = property.DeviceName;
        dlg.Label = cameraDevice.DisplayName;
        try
        {
          var images = cameraDevice.GetObjects(null);
          if(images.Count>0)
          {
            foreach (DeviceObject deviceObject in images)
            {
              if (!_itembycamera.ContainsKey(cameraDevice))
                _itembycamera.Add(cameraDevice, new AsyncObservableCollection<FileItem>());
              _itembycamera[cameraDevice].Add(new FileItem(deviceObject, cameraDevice));
              //Items.Add(new FileItem(deviceObject, cameraDevice));
            }
          }
        }
        catch (Exception exception)
        {
          StaticHelper.Instance.SystemMessage = TranslationStrings.LabelErrorLoadingFileList;
          Log.Error("Error loading file list", exception);
        }
      }

      if (checkset)
      {
        DateTime basetime = _itembycamera[ServiceProvider.DeviceManager.ConnectedDevices[0]][0].FileDate;
        foreach (
          ICameraDevice connectedDevice in
            ServiceProvider.DeviceManager.ConnectedDevices.Where(
              connectedDevice => _itembycamera.ContainsKey(connectedDevice)))
        {
          if (!_timeDif.ContainsKey(connectedDevice))
            _timeDif.Add(connectedDevice, 0);
          _timeDif[connectedDevice] = (int) (_itembycamera[connectedDevice][0].FileDate - basetime).TotalSeconds;
        }


        foreach (
          ICameraDevice connectedDevice in
            ServiceProvider.DeviceManager.ConnectedDevices.Where(
              connectedDevice => _itembycamera.ContainsKey(connectedDevice)))
        {
          for (int i = 0; i < _itembycamera[connectedDevice].Count; i++)
          {
            DateTime date = _itembycamera[connectedDevice][i].FileDate.AddSeconds(-_timeDif[connectedDevice]);
            if (_timeTable.Contains(date))
              continue;
            bool found = _timeTable.Any(t => Math.Abs((t - date).TotalSeconds) < threshold);
            if (!found)
              _timeTable.Add(date);
          }
        }

        _timeTable.Sort();
        foreach (
          ICameraDevice connectedDevice in
            ServiceProvider.DeviceManager.ConnectedDevices.Where(
              connectedDevice => _itembycamera.ContainsKey(connectedDevice)))
        {
          for (int i = 0; i < _timeTable.Count; i++)
          {
            bool found = false;
            for (int j = 0; j < _itembycamera[connectedDevice].Count; j++)
            {
              DateTime date = _itembycamera[connectedDevice][j].FileDate.AddSeconds(-_timeDif[connectedDevice]);
              if (Math.Abs((_timeTable[i] - date).TotalSeconds) < threshold)
              {
                Items.Add(_itembycamera[connectedDevice][j]);
                found = true;
                break;
              }
            }
            if (!found)
              Items.Add(new FileItem(connectedDevice, _timeTable[i]));
          }
        }
        //-------------------------------------------
        bool[] checks = new bool[_timeTable.Count];
        foreach (ICameraDevice connectedDevice in ServiceProvider.DeviceManager.ConnectedDevices)
        {
          int index = 1;
          foreach (FileItem fileItem in Items)
          {
            if (fileItem.Device == connectedDevice)
            {
              if (fileItem.ItemType == FileItemType.Missing)
                checks[index - 1] = true;
              index++;
            }
          }
        }
        //-------------------------------------------
        // select only complete sets 
        for (int i = 0; i < checks.Count(); i++)
        {
          if (checks[i])
            SetIndex(i+1);
        }
        //-------------------------------------------
      }
      else
      {
        foreach (var fileItem in _itembycamera.Values.SelectMany(lists => lists))
        {
          Items.Add(fileItem);
        }
      }

      CollectionView myView;
      myView = (CollectionView)CollectionViewSource.GetDefaultView(Items);
      if (myView.CanGroup == true)
      {
        PropertyGroupDescription groupDescription
            = new PropertyGroupDescription("Device");
        myView.GroupDescriptions.Add(groupDescription);
      }
      Dispatcher.Invoke(new Action(() => lst_items.ItemsSource = myView));
      dlg.Hide();
    }

    private void btn_download_Click(object sender, RoutedEventArgs e)
    {
      if ((chk_delete.IsChecked == true || chk_format.IsChecked==true) && MessageBox.Show(TranslationStrings.LabelAskForDelete, "", MessageBoxButtons.YesNo) != System.Windows.Forms.DialogResult.Yes)
        return;
      dlg.Show();
      delete = chk_delete.IsChecked == true;
      format = chk_format.IsChecked == true;
      ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.DownloadPhotosWnd_Hide);
      Thread thread = new Thread(TransferFiles); 
      thread.Start();
    }

    void TransferFiles()
    {
      DateTime starttime = DateTime.Now;
      long totalbytes = 0;
      bool somethingwrong = false;
      AsyncObservableCollection<FileItem> itemstoExport = new AsyncObservableCollection<FileItem>(Items.Where(x => x.IsChecked));
      dlg.MaxValue = itemstoExport.Count;
      dlg.Progress = 0;
      int i = 0;
      foreach (FileItem fileItem in itemstoExport)
      {
        if (fileItem.ItemType == FileItemType.Missing)
          continue;
        dlg.Label = fileItem.FileName;
        dlg.ImageSource = fileItem.Thumbnail;
        PhotoSession session = (PhotoSession)fileItem.Device.AttachedPhotoSession ?? ServiceProvider.Settings.DefaultSession;
        string fileName = "";

        if (!session.UseOriginalFilename )
        {
          fileName =
            session.GetNextFileName(Path.GetExtension(fileItem.FileName),
                                    fileItem.Device);
        }
        else
        {
          fileName = Path.Combine(session.Folder, fileItem.FileName);
          if (File.Exists(fileName))
            fileName =
              StaticHelper.GetUniqueFilename(
                Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_", 0,
                Path.GetExtension(fileName));
        }

        try
        {
          fileItem.Device.TransferFile(fileItem.DeviceObject.Handle, fileName);
        }
        catch (Exception exception)
        {
          somethingwrong = true;
          Log.Error("Transfer error", exception);
        }

        // double check if file was transferred
        if (File.Exists(fileName))
        {
          if (delete)
            fileItem.Device.DeleteObject(fileItem.DeviceObject);
        }
        else
        {
          somethingwrong = true;
        }
        totalbytes += new FileInfo(fileName).Length;
        session.AddFile(fileName);
        i++;
        dlg.Progress = i;
      }

      if (format)
      {
        if (!somethingwrong)
        {
          foreach (ICameraDevice connectedDevice in ServiceProvider.DeviceManager.ConnectedDevices)
          {
            try
            {
              connectedDevice.FormatStorage(null);
            }
            catch (Exception exception)
            {
              Log.Error("Unable to format device ", exception);
            }
          }
        }
        else
        {
          Log.Debug("File transfer failed, format aborted!");
          StaticHelper.Instance.SystemMessage = "File transfer failed, format aborted!";
        }
      }
      dlg.Hide();
      double transfersec = (DateTime.Now - starttime).TotalSeconds;
      Log.Debug(string.Format("[BENCHMARK]Total byte transferred ;{0} Total seconds :{1} Speed : {2} Mbyte/sec ", totalbytes,
                                    transfersec, (totalbytes/transfersec/1024/1024).ToString("0000.00")));
      ServiceProvider.Settings.Save();
    }

    private void btn_all_Click(object sender, RoutedEventArgs e)
    {
      foreach (FileItem fileItem in Items)
      {
        fileItem.IsChecked = true;
      }
    }

    private void btn_none_Click(object sender, RoutedEventArgs e)
    {
      foreach (FileItem fileItem in Items)
      {
        fileItem.IsChecked = false;
      }
    }

    private void btn_invert_Click(object sender, RoutedEventArgs e)
    {
      foreach (FileItem fileItem in Items)
      {
        fileItem.IsChecked = !fileItem.IsChecked;
      }

    }

    private void btn_select_Click(object sender, RoutedEventArgs e)
    {
      int selectedidx = 0;
      if (int.TryParse(txt_indx.Text, out selectedidx))
      {
        SetIndex(selectedidx);
      }
    }

    private void SetIndex(int selectedidx)
    {
      foreach (ICameraDevice connectedDevice in ServiceProvider.DeviceManager.ConnectedDevices)
      {
        int index = 1;
        foreach (FileItem fileItem in Items)
        {
          if (fileItem.Device == connectedDevice)
          {
            if (index == selectedidx)
              fileItem.IsChecked = !fileItem.IsChecked;
            index++;
          }
        }
      }
    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      dlg.Show();
      Thread thread = new Thread(PopulateImageList);
      thread.Start();
    }
  }

  public class DownloadableItems
  {
    public AsyncObservableCollection<FileItem> Items { get; set; }
    public ICameraDevice Device { get; set; }

    public DownloadableItems()
    {
      Items = new AsyncObservableCollection<FileItem>();
    }
  }

}

