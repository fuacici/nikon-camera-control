using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CameraControl.Devices.Classes;

namespace CameraControl.Devices.Example
{
  public partial class LiveViewForm : Form
  {
    public ICameraDevice CameraDevice { get; set; }
    private Timer _liveViewTimer = new Timer();

    public LiveViewForm(ICameraDevice cameraDevice)
    {
      //set live view default frame rate to 15
      _liveViewTimer.Interval = 1000/15;
      _liveViewTimer.Tick += _liveViewTimer_Tick;
      CameraDevice = cameraDevice;
      InitializeComponent();
    }

    void _liveViewTimer_Tick(object sender, EventArgs e)
    {
      LiveViewData liveViewData = null;
      try
      {
        liveViewData = CameraDevice.GetLiveViewImage();
      }
      catch (Exception)
      {
        return;
      }

      if (liveViewData == null || liveViewData.ImageData == null)
      {
        return;
      }
      try
      {
        pictureBox1.Image = new Bitmap(new MemoryStream(liveViewData.ImageData,
                                                        liveViewData.ImagePosition,
                                                        liveViewData.ImageData.Length -
                                                        liveViewData.ImagePosition));
      }
      catch (Exception)
      {

      }
    }

    private void btn_start_Click(object sender, EventArgs e)
    {
      CameraDevice.StartLiveView();
      _liveViewTimer.Start();
    }

    private void btn_stop_Click(object sender, EventArgs e)
    {
      _liveViewTimer.Start();
      CameraDevice.StopLiveView();
    }
  }
}
