using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Linq;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using CameraControl.Classes;
using CameraControl.windows;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using WIA;
using WPF.Themes;
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
      CommandBindings.Add(new CommandBinding(ApplicationCommands.Close,(sender, args) => this.Close()));

      ServiceProvider.Configure();
      ServiceProvider.Settings = new Settings();
      ServiceProvider.ThumbWorker = new ThumbWorker();
      ServiceProvider.Settings = ServiceProvider.Settings.Load();
      ServiceProvider.Settings.PropertyChanged += Settings_PropertyChanged;
      ServiceProvider.Settings.LoadSessionData();
      ServiceProvider.Trigger.Start();
      WiaManager = new WIAManager();
      ServiceProvider.Settings.Manager = WiaManager;
      WiaManager.PhotoTaked += WiaManager_PhotoTaked;
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
      ServiceProvider.WindowsManager = new WindowsManager();
      ServiceProvider.WindowsManager.Event += Trigger_Event;
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
        BackgroundWorker backgroundWorker = new BackgroundWorker();
        backgroundWorker.DoWork += delegate
                                     {
                                       try
                                       {
                                         //ServiceProvider.ThumbWorker.Start();
                                         foreach (FileItem fileItem in ServiceProvider.Settings.DefaultSession.Files)
                                         {
                                           if (fileItem.Thumbnail == null)
                                             fileItem.GetExtendedThumb();
                                         }
                                       }
                                       catch (Exception)
                                       {

                                       }
                                     };
        backgroundWorker.RunWorkerAsync();
      }
    }

    private void WiaManager_PhotoTaked(Item item)
    {
      Thread thread = new Thread(PhotoTaked);
      thread.Start(item);
    }

    void PhotoTaked(object o)
    {
      try
      {
        Item item = o as Item;
        ServiceProvider.Settings.SystemMessage = "Photo transfer begin.";
        string s = item.ItemID;
        ImageFile imageFile = null;

        imageFile = (ImageFile)item.Transfer("{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}");
        string fileName = ServiceProvider.Settings.DefaultSession.GetNextFileName(imageFile.FileExtension);
        //file exist : : 0x80070050
        // busy :  0x80210006
        imageFile.SaveFile(fileName);
        Dispatcher.Invoke(
          new Action(delegate { ImageLIst.SelectedValue = ServiceProvider.Settings.DefaultSession.AddFile(fileName); }));
        ServiceProvider.Settings.Save(ServiceProvider.Settings.DefaultSession);
        ServiceProvider.Settings.SystemMessage = "Photo transfer done.";
        if (ServiceProvider.Settings.Preview)
          ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.FullScreenWnd_ShowTimed);

        if (ServiceProvider.Settings.PlaySound)
        {
          string _basedir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
          var mplayer = new SoundPlayer(Path.Combine(_basedir, "Data", "takephoto.wav"));
          mplayer.Play();
        }
        WiaManager.OnPhotoTakenDone();
      }
      catch (Exception ex)
      {
        ServiceProvider.Settings.SystemMessage = "Transfer error !\nMessage :" + ex.Message;
        ServiceProvider.Log.Error("Transfer error !", ex);
      }
      
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      WiaManager.ConnectToCamera();
      ImagePanel.DataContext = ServiceProvider.Settings;
    }


    private void button3_Click(object sender, RoutedEventArgs e)
    {

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
        ServiceProvider.DeviceManager.SelectedCameraDevice.TakePicture();
      }
        catch(COMException comException)
        {
          if(comException.ErrorCode==-2147467259)
          {
            ServiceProvider.Settings.SystemMessage = "Unable to take photo. Unable to focus !";
          }
          else
          {
            ServiceProvider.Log.Error("Take photo", comException);
          }
        }
      catch (Exception exception)
      {
        MessageBox.Show("No picture was taken !\n" + exception.Message);
        ServiceProvider.Log.Error("Take photo", exception);
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
      WiaManager.DisconnectCamera();
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
      if (WiaManager.CameraDevice == null)
        return;
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
          if (fileItem.IsChecked)
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
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(fileItem.FileName, UIOption.OnlyErrorDialogs,
                                                               RecycleOption.SendToRecycleBin);
          }
          if (ImageLIst.Items.Count > 0)
            ImageLIst.SelectedIndex = 0;
        }
      }
      catch (Exception exception)
      {
        ServiceProvider.Log.Error("Error to delete file", exception);
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
        ServiceProvider.Log.Error("Error to show file in explorer", exception);
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
        MessageBox.Show("You application is up to date !"); 
      }
    }
  }
}
