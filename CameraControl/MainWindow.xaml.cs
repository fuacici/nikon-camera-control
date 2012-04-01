using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
using WIA;
using WIAVIDEOLib;

namespace CameraControl
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private WiaVideo wiaVideo;
    public WIAManager WiaManager { get; set; }
    private int imageCounter = 0;

    public MainWindow()
    {
      ServiceProvider.Settings = new Settings();
      ServiceProvider.Settings = ServiceProvider.Settings.Load();
      WiaManager = new WIAManager();
      ServiceProvider.Settings.Manager = WiaManager;
      WiaManager.PhotoTaked += WiaManager_PhotoTaked;
      InitializeComponent();
      DataContext = WiaManager;
      controler1.Manager = WiaManager;
      SessionPanel.DataContext = ServiceProvider.Settings;
    }

    void WiaManager_PhotoTaked(Item item)
    {
      string s = item.ItemID;
      ImageFile imageFile = (ImageFile)item.Transfer("{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}");
      foreach (IProperty property in imageFile.Properties)
      {
        string n = property.Name;
        string v = property.get_Value().ToString();
      }
      imageCounter++;
      string fileName = string.Format("d:\\valami{0}.jpg", imageCounter);
      //file exist : : 0x80070050
      File.Delete(fileName);
      imageFile.SaveFile(fileName);
      BitmapImage logo = new BitmapImage();
      logo.BeginInit();
      logo.UriSource = new Uri(fileName);
      logo.EndInit();
      image1.Source = logo;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      WiaManager.ConnectToCamera();
      //WiaManager = new WIAManager();
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
      WiaManager.TakePicture();
    }

    private void btn_edit_Sesion_Click(object sender, RoutedEventArgs e)
    {
      EditSession editSession = new EditSession(ServiceProvider.Settings.DefaultSession);
      editSession.ShowDialog();
    }

  }
}
