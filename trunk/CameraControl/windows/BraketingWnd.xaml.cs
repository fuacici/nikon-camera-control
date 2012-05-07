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
using CameraControl.Devices;

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for BraketingWnd.xaml
  /// </summary>
  public partial class BraketingWnd : Window
  {
    private ICameraDevice _device;
    private PhotoSession _photoSession;
    AsyncObservableCollection<CheckedListItem> collection = new AsyncObservableCollection<CheckedListItem>();
    BackgroundWorker backgroundWorker = new BackgroundWorker();

    public BraketingWnd(ICameraDevice device, PhotoSession session)
    {
      InitializeComponent();
      _device = device;
      _photoSession = session;
      backgroundWorker.DoWork += delegate { _photoSession.Braketing.TakePhoto(); };
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      if (_photoSession.Braketing.ExposureValues.Count == 0)
        _photoSession.Braketing.ExposureValues = new AsyncObservableCollection<string> {"-1", "0", "+1"};
      foreach (string value in _device.ExposureCompensation.Values)
      {
        CheckedListItem item = new CheckedListItem()
                                 {Name = value, IsChecked = _photoSession.Braketing.ExposureValues.Contains(value)};
        item.PropertyChanged += item_PropertyChanged;
        collection.Add(item);
      }
      listBox1.ItemsSource = collection;
    }

    void item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      _photoSession.Braketing.ExposureValues.Clear();
      foreach (CheckedListItem listItem in collection)
      {
        if (listItem.IsChecked && !_photoSession.Braketing.ExposureValues.Contains(listItem.Name))
          _photoSession.Braketing.ExposureValues.Add(listItem.Name);
      }
    }

    private void btn_shot_Click(object sender, RoutedEventArgs e)
    {
      backgroundWorker.RunWorkerAsync();
    }

    private void btn_close_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

  }
}
