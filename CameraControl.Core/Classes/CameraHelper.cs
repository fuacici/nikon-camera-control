using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CameraControl.Devices;

namespace CameraControl.Core.Classes
{
    public class CameraHelper
    {
        /// <summary>
        /// Captures the specified camera device.
        /// </summary>
        /// <param name="o">ICameraDevice</param>
        public static void Capture(object o)
        {
            if (o != null)
            {
                var camera = o as ICameraDevice;
                if (camera != null)
                {
                    CameraProperty property = ServiceProvider.Settings.CameraProperties.Get(camera);
                    if (property.UseExternalShutter && property.SelectedConfig!=null)
                    {
                        ServiceProvider.ExternalDeviceManager.OpenShutter(property.SelectedConfig);
                        Thread.Sleep(2000);
                        ServiceProvider.ExternalDeviceManager.CloseShutter(property.SelectedConfig);
                        return;
                    }
                    camera.CapturePhoto();
                }
            }
        }

        public static void CaptureNoAf(object o)
        {
            if (o != null)
            {
                var camera = o as ICameraDevice;
                if (camera != null)
                {
                    CameraProperty property = ServiceProvider.Settings.CameraProperties.Get(camera);
                    if (property.UseExternalShutter && property.SelectedConfig != null)
                    {
                        ServiceProvider.ExternalDeviceManager.OpenShutter(property.SelectedConfig);
                        Thread.Sleep(200);
                        ServiceProvider.ExternalDeviceManager.CloseShutter(property.SelectedConfig);
                        return;
                    }
                    camera.CapturePhotoNoAf();
                }
            }
        }


    }
}
