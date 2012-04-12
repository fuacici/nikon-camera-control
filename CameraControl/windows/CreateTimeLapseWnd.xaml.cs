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

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for CreateTimeLapseWnd.xaml
  /// </summary>
  public partial class CreateTimeLapseWnd : Window
  {
    private BackgroundWorker _backgroundWorker = new BackgroundWorker();

    public CreateTimeLapseWnd()
    {
      InitializeComponent();
      _backgroundWorker.DoWork += new DoWorkEventHandler(_backgroundWorker_DoWork);
    }

    void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
    {
      foreach (FileItem fileItem in ServiceProvider.Settings.DefaultSession.Files)
      {
        FileItem item = fileItem;
        Dispatcher.BeginInvoke(new ThreadStart(delegate
                                                 {
                                                   progressBar1.Value++;
                                                   image1.Source = item.Thumbnail;
                                                 }));
        Thread.Sleep(500);
      }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      progressBar1.Maximum = ServiceProvider.Settings.DefaultSession.Files.Count;
      _backgroundWorker.RunWorkerAsync();
    }
  }
}
