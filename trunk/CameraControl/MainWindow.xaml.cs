using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using CameraControl.Classes;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Classes.Queue;
using CameraControl.Core.Devices;
using CameraControl.Core.Devices.Classes;
using CameraControl.windows;
using Microsoft.VisualBasic.FileIO;
using WIA;
using Clipboard = System.Windows.Clipboard;
using EditSession = CameraControl.windows.EditSession;
using MessageBox = System.Windows.Forms.MessageBox;
using Path = System.IO.Path;

namespace CameraControl
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {

    public PropertyWnd PropertyWnd { get; set; }

    public WIAManager WiaManager { get; set; }

    public MainWindow()
    {
      CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (sender, args) => this.Close()));
      SelectDeviceCommand = new RelayCommand<ICameraDevice>(SelectCamera);
      SelectPresetCommand = new RelayCommand<CameraPreset>(SelectPreset);
      WiaManager = new WIAManager();
      //ServiceProvider.Settings.Manager = WiaManager;
      ServiceProvider.DeviceManager.PhotoCaptured += DeviceManager_PhotoCaptured;
      InitializeComponent();
      DataContext = ServiceProvider.Settings;
      if (ServiceProvider.Settings.DefaultSession.Files.Count > 0)
        ImageLIst.SelectedIndex = 0;
      if ((DateTime.Now - ServiceProvider.Settings.LastUpdateCheckDate).TotalDays > 7)
      {
        ServiceProvider.Settings.LastUpdateCheckDate = DateTime.Now;
        ServiceProvider.Settings.Save();
        CheckForUpdate();
      }
      ServiceProvider.Settings.SessionSelected += Settings_SessionSelected;
      ServiceProvider.Settings.PropertyChanged += Settings_PropertyChanged;
      ServiceProvider.WindowsManager.Event += Trigger_Event;
      ServiceProvider.DeviceManager.CameraConnected += DeviceManager_CameraConnected;
      ServiceProvider.DeviceManager.CameraSelected += DeviceManager_CameraSelected;
    }

    void Settings_SessionSelected(PhotoSession oldvalue, PhotoSession newvalue)
    {
      if (oldvalue != null)
        ServiceProvider.Settings.Save(oldvalue);
      ServiceProvider.QueueManager.Clear();
      if (ServiceProvider.DeviceManager.SelectedCameraDevice != null)
        ServiceProvider.DeviceManager.SelectedCameraDevice.AttachedPhotoSession = newvalue;
    }

    void DeviceManager_CameraSelected(ICameraDevice oldcameraDevice, ICameraDevice newcameraDevice)
    {
      // load session data only if not sessiom attached to the selected camera
      if (newcameraDevice.AttachedPhotoSession == null)
      {
        CameraProperty property = ServiceProvider.Settings.CameraProperties.Get(newcameraDevice);
        newcameraDevice.AttachedPhotoSession = ServiceProvider.Settings.GetSession(property.PhotoSessionName);
      }
      if (newcameraDevice.AttachedPhotoSession != null)
        ServiceProvider.Settings.DefaultSession = newcameraDevice.AttachedPhotoSession;
      btn_capture_noaf.IsEnabled = newcameraDevice.GetCapability(CapabilityEnum.CaptureNoAf);
      btn_liveview.IsEnabled = newcameraDevice.GetCapability(CapabilityEnum.LiveView);
    }

    void DeviceManager_CameraConnected(ICameraDevice cameraDevice)
    {
      CameraProperty property = ServiceProvider.Settings.CameraProperties.Get(cameraDevice);
      cameraDevice.DisplayName = property.DeviceName;
      cameraDevice.AttachedPhotoSession = ServiceProvider.Settings.GetSession(property.PhotoSessionName);
    }

    void DeviceManager_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
    {
      Thread thread = new Thread(PhotoCaptured);
      thread.Start(eventArgs);
    }

    void Trigger_Event(string cmd)
    {
      Dispatcher.Invoke(new Action(delegate
                                     {
                                       switch (cmd)
                                       {
                                         case WindowsCmdConsts.Next_Image:
                                           if (ImageLIst.SelectedIndex < ImageLIst.Items.Count - 1)
                                           {
                                             ImageLIst.SelectedIndex++;
                                           }
                                           break;
                                         case WindowsCmdConsts.Prev_Image:
                                           if (ImageLIst.SelectedIndex > 0)
                                           {
                                             ImageLIst.SelectedIndex--;
                                           }
                                           break;
                                       }
                                     }));
    }

    private void CheckForUpdate()
    {
      if(PhotoUtils.CheckForUpdate())
        Close();
    }

    private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "DefaultSession")
      {
        ImageLIst.SelectedIndex = 0;
        if (ImageLIst.Items.Count == 0)
          ServiceProvider.Settings.SelectedBitmap.DisplayImage = null;
      }
    }


    void PhotoCaptured(object o)
    {
      try
      {
        StaticHelper.Instance.SystemMessage = "Photo transfer begin.";
        Log.Debug("Photo transfer begin.");
        PhotoCapturedEventArgs eventArgs = o as PhotoCapturedEventArgs;
        if (eventArgs == null)
          return;
        PhotoSession session = eventArgs.CameraDevice.AttachedPhotoSession;
        if (session == null)
          session = ServiceProvider.Settings.DefaultSession;
        if ((session.NoDownload && !eventArgs.CameraDevice.CaptureInSdRam))
        {
          eventArgs.CameraDevice.IsBusy = false;
          return;
        }
        string fileName = "";
        if (!session.UseOriginalFilename || eventArgs.CameraDevice.CaptureInSdRam)
        {
          fileName =
            session.GetNextFileName(Path.GetExtension(eventArgs.FileName),
                                    eventArgs.CameraDevice);
        }
        else
        {
          fileName = Path.Combine(session.Folder, eventArgs.FileName);
          if (File.Exists(fileName))
            fileName =
              PhotoUtils.GetUniqueFilename(
                Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_", 0,
                Path.GetExtension(fileName));
        }
        if (!Directory.Exists(Path.GetDirectoryName(fileName)))
        {
          Directory.CreateDirectory(Path.GetDirectoryName(fileName));
        }
        Log.Debug("Transfer started :" + fileName);
        eventArgs.CameraDevice.TransferFile(eventArgs.EventArgs, fileName);
        Log.Debug("Transfer done :" + fileName);
        if (ServiceProvider.Settings.AutoPreview)
        {
          Dispatcher.Invoke(
            new Action(
              delegate { ImageLIst.SelectedValue = session.AddFile(fileName); }));
        }
        else
        {
          session.AddFile(fileName);
        }
        ServiceProvider.Settings.Save(session);
        StaticHelper.Instance.SystemMessage = "Photo transfer done.";
        eventArgs.CameraDevice.IsBusy = false;
        if (ServiceProvider.Settings.Preview)
          ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.FullScreenWnd_ShowTimed);
        if (ServiceProvider.Settings.PlaySound)
        {
          PhotoUtils.PlayCaptureSound();
        }
      }
      catch (Exception ex)
      {
        StaticHelper.Instance.SystemMessage = "Transfer error !\nMessage :" + ex.Message;
        Log.Error("Transfer error !", ex);
      }
    }

    /// <summary>
    /// Gets the command for selecting a device
    /// </summary>
    public RelayCommand<ICameraDevice> SelectDeviceCommand
    {
      get;
      private set;
    }

    public RelayCommand<CameraPreset> SelectPresetCommand
    {
      get;
      private set;
    }

    private void SelectCamera(ICameraDevice cameraDevice)
    {
      ServiceProvider.DeviceManager.SelectedCameraDevice = cameraDevice;
    }

    private void SelectPreset(CameraPreset preset)
    {
      preset.Set(ServiceProvider.DeviceManager.SelectedCameraDevice);
    }
    
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      WiaManager.ConnectToCamera();
      ImagePanel.DataContext = ServiceProvider.Settings;
    }


    private void button3_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("Main window capture started"); 
      if (!ServiceProvider.Settings.DefaultSession.TimeLapse.IsDisabled)
      {
        if (
          MessageBox.Show("A time lapse photo session runnig !\n Do you want to stop it  ?",
                          "", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
        {
          ServiceProvider.Settings.DefaultSession.TimeLapse.Stop();
          return;
        }
      }

      try
      {
        if (ServiceProvider.DeviceManager.SelectedCameraDevice.ShutterSpeed.Value == "Bulb")
        {
          if (ServiceProvider.DeviceManager.SelectedCameraDevice.GetCapability(CapabilityEnum.Bulb))
          {
            BulbWnd wnd = new BulbWnd();
            wnd.ShowDialog();
            return;
          }
          else
          {
           StaticHelper.Instance.SystemMessage = "Bulb mode not supported !";
            return;
          }
        }
        ServiceProvider.DeviceManager.SelectedCameraDevice.CapturePhoto();
      }
      catch (DeviceException exception)
        {
         StaticHelper.Instance.SystemMessage = exception.Message;
          Log.Error("Take photo", exception);
        }
      catch (Exception exception)
      {
        MessageBox.Show("No picture was taken !\n" + exception.Message);
        Log.Error("Take photo", exception);
      }

    }

    private void btn_edit_Sesion_Click(object sender, RoutedEventArgs e)
    {
      if (File.Exists(ServiceProvider.Settings.DefaultSession.ConfigFile))
      {
        File.Delete(ServiceProvider.Settings.DefaultSession.ConfigFile);
      }
      EditSession editSession = new EditSession(ServiceProvider.Settings.DefaultSession);
      editSession.ShowDialog();
      ServiceProvider.Settings.Save(ServiceProvider.Settings.DefaultSession);
    }

    private void btn_add_Sesion_Click(object sender, RoutedEventArgs e)
    {
      EditSession editSession = new EditSession(new PhotoSession());
      if (editSession.ShowDialog() == true)
      {
        ServiceProvider.Settings.Add(editSession.Session);
        ServiceProvider.Settings.DefaultSession = editSession.Session;
      }
    }

    private void ImageLIst_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

      if (e.AddedItems.Count > 0)
      {
        BackgroundWorker worker = new BackgroundWorker();
        worker.DoWork += worker_DoWork;
        FileItem item = e.AddedItems[0] as FileItem;
        if (item != null)
        {
          //image1.Source = item.Thumbnail;
          ServiceProvider.Settings.SelectedBitmap.SetFileItem(item);
          worker.RunWorkerAsync(item);
        }
      }
    }

    private void worker_DoWork(object sender, DoWorkEventArgs e)
    {
      Thread.Sleep(200);
      FileItem fileItem = e.Argument as FileItem;
      if (ServiceProvider.Settings.SelectedBitmap.FileItem.FileName != fileItem.FileName)
        return;
      ServiceProvider.Settings.ImageLoading = Visibility.Visible;
      ServiceProvider.Settings.SelectedBitmap.GetBitmap();
      ServiceProvider.Settings.ImageLoading = Visibility.Hidden;
    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      if (button1.IsChecked == true)
      {
        if (PropertyWnd == null)
        {
          PropertyWnd = new PropertyWnd();
        }
        PropertyWnd.IsVisibleChanged -= PropertyWnd_IsVisibleChanged;
        PropertyWnd.Show();
        PropertyWnd.IsVisibleChanged += PropertyWnd_IsVisibleChanged;
      }
      else
      {
        if (PropertyWnd != null && PropertyWnd.Visibility == Visibility.Visible)
        {
          PropertyWnd.IsVisibleChanged -= PropertyWnd_IsVisibleChanged;
          PropertyWnd.Hide();
        }
      }
    }

    private void PropertyWnd_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      button1.IsChecked = !button1.IsChecked;
    }

    private void button2_Click(object sender, RoutedEventArgs e)
    {
      if (PropertyWnd != null && PropertyWnd.IsVisible)
        PropertyWnd.Topmost = false;
      SettingsWnd wnd = new SettingsWnd();
      wnd.ShowDialog();
      if (PropertyWnd != null && PropertyWnd.IsVisible)
        PropertyWnd.Topmost = true;
    }

    private void Window_Closed(object sender, EventArgs e)
    {
      if (PropertyWnd != null)
      {
        PropertyWnd.Hide();
        PropertyWnd.Close();
      }
      ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.All_Close);
      //WiaManager.DisconnectCamera();
    }

    private void but_timelapse_Click(object sender, RoutedEventArgs e)
    {
      if (PropertyWnd != null && PropertyWnd.IsVisible)
        PropertyWnd.Topmost = false;
      TimeLapseWnd wnd = new TimeLapseWnd();
      wnd.ShowDialog();
      if (PropertyWnd != null && PropertyWnd.IsVisible)
        PropertyWnd.Topmost = true;
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      if (!ServiceProvider.Settings.DefaultSession.TimeLapse.IsDisabled)
      {
        if (
          MessageBox.Show("A time lapse photo session runnig !\n Do you want to stop it and exit from application ?",
                          "Closing", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
        {
          e.Cancel = true;
        }
        else
        {
          ServiceProvider.Settings.DefaultSession.TimeLapse.Stop();
        }
      }
    }

    private void but_fullscreen_Click(object sender, RoutedEventArgs e)
    {
      ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.FullScreenWnd_Show);
    }

    private void image1_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ClickCount >= 2 && e.LeftButton == MouseButtonState.Pressed)
        ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.FullScreenWnd_Show);
    }

    private void btn_liveview_Click(object sender, RoutedEventArgs e)
    {
      ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.LiveViewWnd_Show);
    }

    private void btn_about_Click(object sender, RoutedEventArgs e)
    {
      AboutWnd wnd=new AboutWnd();
      wnd.ShowDialog();
    }

    private void mnu_delete_file_Click(object sender, RoutedEventArgs e)
    {
      List<FileItem> filestodelete = new List<FileItem>();
      try
      {
        foreach (FileItem fileItem in ServiceProvider.Settings.DefaultSession.Files)
        {
          if (fileItem.IsChecked || !File.Exists(fileItem.FileName))
            filestodelete.Add(fileItem);
        }

        if (ServiceProvider.Settings.SelectedBitmap != null && ServiceProvider.Settings.SelectedBitmap.FileItem != null &&
            filestodelete.Count == 0)
          filestodelete.Add(ServiceProvider.Settings.SelectedBitmap.FileItem);

        if (filestodelete.Count == 0)
          return;

        if (
          MessageBox.Show("Do you really want to delete selected file(s) ?", "Delete file", MessageBoxButtons.YesNo) ==
          System.Windows.Forms.DialogResult.Yes)
        {
          foreach (FileItem fileItem in filestodelete)
          {
            if ((ServiceProvider.Settings.SelectedBitmap != null &&
                 ServiceProvider.Settings.SelectedBitmap.FileItem != null &&
                 fileItem.FileName == ServiceProvider.Settings.SelectedBitmap.FileItem.FileName))
            {
              ServiceProvider.Settings.SelectedBitmap.DisplayImage = null;
            }
            if (File.Exists(fileItem.FileName))
              FileSystem.DeleteFile(fileItem.FileName, UIOption.OnlyErrorDialogs,
                                    RecycleOption.SendToRecycleBin);
            else
            {
              ServiceProvider.Settings.DefaultSession.Files.Remove(fileItem);
            }
          }
          if (ImageLIst.Items.Count > 0)
            ImageLIst.SelectedIndex = 0;
        }
      }
      catch (Exception exception)
      {
        Log.Error("Error to delete file", exception);
      }
    }

    private void mnu_show_explorer_Click(object sender, RoutedEventArgs e)
    {
      if (ServiceProvider.Settings.SelectedBitmap == null || ServiceProvider.Settings.SelectedBitmap.FileItem == null)
        return;
      try
      {
        ProcessStartInfo processStartInfo = new ProcessStartInfo();
        processStartInfo.FileName = "explorer";
        processStartInfo.UseShellExecute = true;
        processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
        processStartInfo.Arguments =
            string.Format("/e,/select,\"{0}\"", ServiceProvider.Settings.SelectedBitmap.FileItem.FileName);
        Process.Start(processStartInfo);
      }
      catch (Exception exception)
      {
        Log.Error("Error to show file in explorer", exception);
      }
    }

    private void mnu_open_Click(object sender, RoutedEventArgs e)
    {
      if (ServiceProvider.Settings.SelectedBitmap == null || ServiceProvider.Settings.SelectedBitmap.FileItem == null)
        return;
      Process.Start(ServiceProvider.Settings.SelectedBitmap.FileItem.FileName);
    }

    private void btn_br_Click(object sender, RoutedEventArgs e)
    {
      BraketingWnd wnd = new BraketingWnd(ServiceProvider.DeviceManager.SelectedCameraDevice, ServiceProvider.Settings.DefaultSession);
      wnd.ShowDialog();
    }


    private void mnu_select_none_Click(object sender, RoutedEventArgs e)
    {
      ServiceProvider.Settings.DefaultSession.SelectNone();
    }

    private void mnu_select_all_Click(object sender, RoutedEventArgs e)
    {
      ServiceProvider.Settings.DefaultSession.SelectAll();
    }

    private void mnu_copypath_Click(object sender, RoutedEventArgs e)
    {
      Clipboard.SetText(ServiceProvider.Settings.SelectedBitmap.FileItem.FileName);
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
      PhotoUtils.Run("http://nccsoftware.blogspot.com/", "");
    }

    private void MenuItem_Click_1(object sender, RoutedEventArgs e)
    {
      if(PhotoUtils.CheckForUpdate())
      {
        Close();
      }
      else
      {
        MessageBox.Show("Your application is up to date !"); 
      }
    }

    private void MenuItem_Click_2(object sender, RoutedEventArgs e)
    {
      ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.MultipleCameraWnd_Show);
    }

    private void mnu_reconnect_Click(object sender, RoutedEventArgs e)
    {
      WiaManager.ConnectToCamera();
    }

    private void MenuItem_Click_3(object sender, RoutedEventArgs e)
    {
      ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.CameraPropertyWnd_Show,
                                                    ServiceProvider.DeviceManager.SelectedCameraDevice);
    }

    private void MenuItem_Click_4(object sender, RoutedEventArgs e)
    {
      CameraPreset cameraPreset = new CameraPreset();
      SavePresetWnd wnd=new SavePresetWnd(cameraPreset);
      if (wnd.ShowDialog() == true)
      {
        foreach (CameraPreset preset in ServiceProvider.Settings.CameraPresets)
        {
          if(preset.Name==cameraPreset.Name)
          {
            cameraPreset = preset;
            break;
          }
        }
        cameraPreset.Get(ServiceProvider.DeviceManager.SelectedCameraDevice);
        if (!ServiceProvider.Settings.CameraPresets.Contains(cameraPreset))
          ServiceProvider.Settings.CameraPresets.Add(cameraPreset);
        ServiceProvider.Settings.Save();
      }
    }

    private void MenuItem_Click_5(object sender, RoutedEventArgs e)
    {
      PresetEditWnd wnd = new PresetEditWnd();
      wnd.ShowDialog();
    }

    private void mnu_donate_Click(object sender, RoutedEventArgs e)
    {
      PhotoUtils.Donate();
    }

    private void btn_browse_Click(object sender, RoutedEventArgs e)
    {
      ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.BrowseWnd_Show);
    }

    private void btn_capture_noaf_Click(object sender, RoutedEventArgs e)
    {
      Log.Debug("Main window capture no af started");
      try
      {
        if (ServiceProvider.DeviceManager.SelectedCameraDevice.ShutterSpeed.Value == "Bulb")
        {
          if (ServiceProvider.DeviceManager.SelectedCameraDevice.GetCapability(CapabilityEnum.Bulb))
          {
            BulbWnd wnd = new BulbWnd();
            wnd.ShowDialog();
            return;
          }
          else
          {
            StaticHelper.Instance.SystemMessage = "Bulb mode not supported !";
            return;
          }
        }
        ServiceProvider.DeviceManager.SelectedCameraDevice.CapturePhotoNoAf();
      }
      catch (DeviceException exception)
      {
        StaticHelper.Instance.SystemMessage = exception.Message;
        Log.Error("Take photo", exception);
      }
      catch (Exception exception)
      {
        MessageBox.Show("No picture was taken !\n" + exception.Message);
        Log.Error("Take photo", exception);
      }
    }

  }
}
