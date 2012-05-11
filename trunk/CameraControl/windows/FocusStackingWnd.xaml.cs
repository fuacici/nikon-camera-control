using System;
using System.Collections.Generic;
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

namespace CameraControl.windows
{
  /// <summary>
  /// Interaction logic for FocusStackingWnd.xaml
  /// </summary>
  public partial class FocusStackingWnd : Window
  {
    public bool IsBusy { get; set; }
    private int photo_count = 0;
    private bool preview = false;
    public FocusStackingWnd()
    {
      InitializeComponent();
      ServiceProvider.Settings.Manager.PhotoTakenDone += new Classes.WIAManager.PhotoTakedEventHandler(Manager_PhotoTakenDone);
      IsBusy = false;
    }

    void Manager_PhotoTakenDone(WIA.Item imageFile)
    {
      if (photo_count <= int_photono.Value)
      {
        TakePhoto();
      }
      else
      {
        IsBusy = false;
      }
    }

    private void TakePhoto()
    {
      if (IsBusy)
      {
        photo_count++;
        ServiceProvider.DeviceManager.SelectedCameraDevice.Focus((int)int_step.Value);
        Thread.Sleep(200);
        if (!preview)
        {
          ServiceProvider.DeviceManager.SelectedCameraDevice.TakePictureNoAf();
        }
        else
        {
          if (photo_count <= int_photono.Value)
          {
            Thread.Sleep(500);
            TakePhoto();
          }
          else
          {
            return;
          }
        }
      }
    }

    private void btn_preview_Click(object sender, RoutedEventArgs e)
    {
      photo_count = 0;
      IsBusy = true;
      preview = true;
      TakePhoto();
    }
  }
}
