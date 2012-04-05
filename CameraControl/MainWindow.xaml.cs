using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CameraControl.Classes;
using CameraControl.windows;
using WIA;
using WPF.Themes;
using MessageBox = System.Windows.Forms.MessageBox;

namespace CameraControl
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    
    public WIAManager WiaManager { get; set; }

    public MainWindow()
    {
      ServiceProvider.Settings = new Settings();
      ServiceProvider.ThumbWorker = new ThumbWorker();
      ServiceProvider.Settings = ServiceProvider.Settings.Load();
      ServiceProvider.Settings.PropertyChanged += Settings_PropertyChanged;
      ServiceProvider.Settings.LoadSessionData();
      WiaManager = new WIAManager();
      ServiceProvider.Settings.Manager = WiaManager;
      WiaManager.PhotoTaked += WiaManager_PhotoTaked;
      InitializeComponent();
      ThemeManager.ApplyTheme(Application.Current, "ExpressionDark");
      DataContext = ServiceProvider.Settings;
      controler1.Manager = WiaManager;
    }

    void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if(e.PropertyName=="DefaultSession")
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

    void WiaManager_PhotoTaked(Item item)
    {
      try
      {
        string s = item.ItemID;
        ImageFile imageFile = (ImageFile)item.Transfer("{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}");
        string fileName = ServiceProvider.Settings.DefaultSession.GetNextFileName(imageFile.FileExtension);
        //file exist : : 0x80070050
        imageFile.SaveFile(fileName);

        ImageLIst.SelectedValue = ServiceProvider.Settings.DefaultSession.AddFile(fileName);
        ServiceProvider.Settings.Save(ServiceProvider.Settings.DefaultSession);
      }
      catch (Exception ex)
      {
        MessageBox.Show("Transfer error !\nMessage :" + ex.Message);

      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      WiaManager.ConnectToCamera();
      SessionPanel.DataContext = ServiceProvider.Settings;
      ImagePanel.DataContext = ServiceProvider.Settings;
    }

    //private void button1_Click(object sender, RoutedEventArgs e)
    //{
    //  if (WiaManager.ConnectToCamera())
    //  {
    //    //listBox1.Items.Clear();
    //    foreach (Property property in WiaManager.Device.Properties)
    //    {
    //      textBox1.Text += property.Name + "|" + property.get_Value().ToString() + "|" + property.IsReadOnly.ToString() +
    //                       property.IsVector.ToString() + "|" + property.SubType.ToString() + "|";
    //      try
    //      {
    //        textBox1.Text += property.SubTypeMax.ToString();
    //        //  + property.SubTypeMin.ToString() + "|" +
    //        //property.SubTypeStep.ToString() + "|" + property.SubTypeValues.ToString() +
    //        //property.Type.ToString() + "|";
    //      }
    //      catch (Exception)
    //      {
            
            
    //      }
    //      textBox1.Text += "\n";
    //    }
    //  }
    //}

    private void button3_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        WiaManager.TakePicture();
      }
      catch (Exception exception)
      {
        MessageBox.Show("No picture was teked !\n" + exception.Message);
      }
      
    }

    private void btn_edit_Sesion_Click(object sender, RoutedEventArgs e)
    {
      if(File.Exists(ServiceProvider.Settings.DefaultSession.ConfigFile))
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
        BackgroundWorker worker=new BackgroundWorker();
        worker.DoWork += worker_DoWork;
        FileItem item = e.AddedItems[0] as FileItem;
        if (item != null)
        {
          image1.Source = item.Thumbnail;
          worker.RunWorkerAsync(item);
        }
      }
    }

    void worker_DoWork(object sender, DoWorkEventArgs e)
    {
      ServiceProvider.Settings.ImageLoading = Visibility.Visible;
      FileItem fileItem = e.Argument as FileItem;
      ServiceProvider.Settings.SelectedBitmap.SetFileItem(fileItem);
      BitmapImage logo = ServiceProvider.Settings.SelectedBitmap.GetBitmap();
      ServiceProvider.Settings.ImageLoading = Visibility.Hidden;
      //logo.BeginInit();
      //logo.UriSource = new Uri(fileItem.FileName);
      //logo.EndInit();
      //logo.Freeze();

         //Call the UI thread using the Dispatcher to update the Image control
      Dispatcher.BeginInvoke(new ThreadStart(delegate
                                               {
                                                 image1.Source = logo;
                                               }));

    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      PropertyWnd wnd=new PropertyWnd();
      wnd.Show();
    }

  }
}
