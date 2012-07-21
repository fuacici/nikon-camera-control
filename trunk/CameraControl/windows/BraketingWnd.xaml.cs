using System;
using System.ComponentModel;
using System.Windows;
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
      _photoSession.Braketing.IsBusy = false;
      backgroundWorker.DoWork += delegate { _photoSession.Braketing.TakePhoto(); };
      _photoSession.Braketing.IsBusyChanged += Braketing_IsBusyChanged;
      _photoSession.Braketing.PhotoCaptured += Braketing_PhotoCaptured;
      _photoSession.Braketing.BracketingDone += Braketing_BracketingDone;
    }

    void Braketing_BracketingDone(object sender, EventArgs e)
    {
      Dispatcher.Invoke(new Action(delegate { lbl_status.Content = "Bracketing done"; }));
    }

    void Braketing_PhotoCaptured(object sender, EventArgs e)
    {
      Dispatcher.Invoke(new Action(delegate
      {
        lbl_status.Content = "Action in progress " + _photoSession.Braketing.ExposureValues.Count + "/" +
          _photoSession.Braketing.Index;
      }));
    }

    void Braketing_IsBusyChanged(object sender, EventArgs e)
    {
      Dispatcher.Invoke(new Action(delegate { btn_shot.Content = _photoSession.Braketing.IsBusy ? "Stop" : "Start"; }));
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

    void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
      if (!_photoSession.Braketing.IsBusy)
      {
        backgroundWorker.RunWorkerAsync();
      }
      else
      {
        _photoSession.Braketing.Stop();
      }
      
    }

    private void btn_close_Click(object sender, RoutedEventArgs e)
    {
      _photoSession.Braketing.Stop();
      this.Close();
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
      _photoSession.Braketing.Stop();
    }

  }
}
