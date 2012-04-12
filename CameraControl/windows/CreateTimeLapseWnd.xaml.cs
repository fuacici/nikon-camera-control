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
//using System.Windows.Shapes;
using CameraControl.Classes;
using FreeImageAPI;
using MessageBox = System.Windows.Forms.MessageBox;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for CreateTimeLapseWnd.xaml
  /// </summary>
  public partial class CreateTimeLapseWnd : Window
  {
    private BackgroundWorker _backgroundWorker = new BackgroundWorker();
    private string _tempFolder = "";
    private int _counter = 0;
    private bool _isbusy = false;

    public CreateTimeLapseWnd()
    {
      InitializeComponent();
      _backgroundWorker.DoWork += new DoWorkEventHandler(_backgroundWorker_DoWork);
      _tempFolder = Path.Combine(Path.GetTempPath(), "NCC_TimeLapse", ServiceProvider.Settings.DefaultSession.Name);
      if (Directory.Exists(_tempFolder))
      {
        Directory.Delete(_tempFolder, true);
      }
      Directory.CreateDirectory(_tempFolder);
    }

    void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
    {
      foreach (FileItem fileItem in ServiceProvider.Settings.DefaultSession.Files)
      {
        _counter++;
        FileItem item = fileItem;
        ResizeImage(fileItem.FileName);
        Dispatcher.BeginInvoke(new ThreadStart(delegate
                                                 {
                                                   progressBar1.Value++;
                                                   image1.Source = item.Thumbnail;
                                                   label1.Content = string.Format("{0}/{1}", _counter,
                                                                                  ServiceProvider.Settings.
                                                                                    DefaultSession.Files.Count);
                                                 }));
        if(_backgroundWorker.CancellationPending)
        {
          return;
        }
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      progressBar1.Maximum = ServiceProvider.Settings.DefaultSession.Files.Count;
      _isbusy = true;
      _backgroundWorker.RunWorkerAsync();
    }

    private void ResizeImage(string file)
    {
      string newfile = Path.Combine(_tempFolder, "img" + _counter.ToString("000000") + ".jpg");
      FIBITMAP dib = FreeImage.LoadEx(file);

      uint dw = FreeImage.GetWidth(dib);
      uint dh = FreeImage.GetHeight(dib);
      int tw = 1920;
      int th = 1080;
      double zw = (tw / (double)dw);
      double zh = (th / (double)dh);
      double z = (zw <= zh) ? zw : zh;
      dw = (uint)(dw * z);
      dh = (uint)(dh * z);
      int difw =(int) (tw - dw);
      int difh = (int)(th - dh);
      if (FreeImage.GetFileType(file, 0) == FREE_IMAGE_FORMAT.FIF_RAW)
      {
        FIBITMAP bmp = FreeImage.ToneMapping(dib, FREE_IMAGE_TMO.FITMO_REINHARD05, 0, 0); // ConvertToType(dib, FREE_IMAGE_TYPE.FIT_BITMAP, false);
        FIBITMAP resized = FreeImage.Rescale(bmp, (int)dw, (int)dh, FREE_IMAGE_FILTER.FILTER_LANCZOS3);
        FIBITMAP final = FreeImage.EnlargeCanvas<RGBQUAD>(resized, difw / 2, difh / 2, difw - (difw / 2), difh - (difh / 2),
                                                          new RGBQUAD(System.Drawing.Color.Black),
                                                          FREE_IMAGE_COLOR_OPTIONS.FICO_RGB);
        FreeImage.SaveEx(final, newfile);
        FreeImage.UnloadEx(ref final);
        FreeImage.UnloadEx(ref resized);
        FreeImage.UnloadEx(ref dib);
        FreeImage.UnloadEx(ref bmp);
      }
      else
      {
        FIBITMAP resized = FreeImage.Rescale(dib, (int) dw, (int) dh, FREE_IMAGE_FILTER.FILTER_LANCZOS3);
        FIBITMAP final = FreeImage.EnlargeCanvas<RGBQUAD>(resized, difw/2, difh/2, difw - (difw/2), difh - (difh/2),
                                                          new RGBQUAD(System.Drawing.Color.Black),
                                                          FREE_IMAGE_COLOR_OPTIONS.FICO_RGB);
        FreeImage.SaveEx(final,newfile);
        FreeImage.UnloadEx(ref final);
        FreeImage.UnloadEx(ref resized);
        FreeImage.UnloadEx(ref dib);
      }
    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      if(_backgroundWorker.IsBusy)
      {
        if(MessageBox.Show("A task is running !\n Do you want to cancel it ?","",MessageBoxButtons.YesNo)==System.Windows.Forms.DialogResult.No)
        {
          e.Cancel = true;
        }
      }
    }

  }
}
