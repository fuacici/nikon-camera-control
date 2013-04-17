using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CameraControl.Devices.Classes;
using Canon.Eos.Framework;
using Canon.Eos.Framework.Eventing;
using Canon.Eos.Framework.Internal.SDK;
using PortableDeviceLib;
using PortableDeviceLib.Model;

namespace CameraControl.Devices.Canon
{
    public class CanonSDKBase : BaseMTPCamera
    {
        
        //public const int CONST_CMD_CANON_EOS_RemoteRelease = 0x910F;
        //public const int CONST_CMD_CANON_EOS_BulbStart = 0x9125;
        //public const int CONST_CMD_CANON_EOS_BulbEnd = 0x9126;
        //public const int CONST_CMD_CANON_EOS_SetEventMode = 0x9115;
        //public const int CONST_CMD_CANON_EOS_SetRemoteMode = 0x9114;
        //public const int CONST_CMD_CANON_EOS_GetEvent = 0x9116;
        //public const int CONST_CMD_CANON_EOS_DoAf = 0x9154;
        //public const int CONST_CMD_CANON_EOS_GetViewFinderData = 0x9153;
        //public const int CONST_CMD_CANON_EOS_GetObjectInfo = 0x9103;

        //public const int CONST_CMD_CANON_EOS_SetDevicePropValueEx = 0x9110;
        //public const int CONST_CMD_CANON_EOS_RequestDevicePropValue = 0x9109;

        //public const int CONST_PROP_EOS_ShutterSpeed = 0xD102;
        //public const int CONST_PROP_EOS_LiveView = 0xD1B0;

        //public const int CONST_Event_CANON_EOS_PropValueChanged = 0xc189 ;
        //public const int CONST_Event_CANON_EOS_ObjectAddedEx = 0xc181;

        private bool _eventIsbusy = false;

        private byte[] _liveViewImageData = null;

        public EosCamera Camera = null;

        protected Dictionary<uint, string> _shutterTable = new Dictionary<uint, string>
                                                         {
                                                           {0x0C, "Bulb"},
                                                           {0x10, "30"},
                                                           {0x13, "25"},
                                                           {0x14, "20"},
                                                           {0x15, "20 (1/3)"},
                                                           {0x18, "15"},
                                                           {0x1B, "13"},
                                                           {0x1C, "10"},
                                                           {0x1D, "20  (1/3)"},
                                                           {0x20, "8"},
                                                           {0x23, "6 (1/3)"},
                                                           {0x24, "6"},
                                                           {0x25, "5"},
                                                           {0x28, "4"},
                                                           {0x2B, "3.2"},
                                                           {0x2C, "3"},
                                                           {0x2D, "2.5"},
                                                           {0x30, "2"},
                                                           {0x33, "1.6"},
                                                           {0x34, "1.5"},
                                                           {0x35, "1.3"},
                                                           {0x38, "1"},
                                                           {0x3B, "0.8"},
                                                           {0x3C, "0.7"},
                                                           {0x3D, "0.6"},
                                                           {0x40, "0.5"},
                                                           {0x43, "0.4"},
                                                           {0x44, "0.3"},
                                                           {0x45, "0.3 (1/3)"},
                                                           {0x48, "1/4"},
                                                           {0x4B, "1/5"},
                                                           {0x4C, "1/6"},
                                                           {0x4D, "1/56 (1/3)"},
                                                           {0x50, "1/8"},
                                                           {0x53, "1/10 (1/3)"},
                                                           {0x54, "1/10"},
                                                           {0x55, "1/13"},
                                                           {0x58 ,"1/15"},
                                                           {0x5B ,"1/20 (1/3)"},
                                                           {0x5C ,"1/20"},
                                                           {0x5D ,"1/25"},
                                                           {0x60 ,"1/30"},
                                                           {0x63 ,"1/40"},
                                                           {0x64 ,"1/45"},
                                                           {0x65 ,"1/50"},
                                                           {0x68 ,"1/60"},
                                                           {0x6B ,"1/80"},
                                                           {0x6C ,"1/90"},
                                                           {0x6D ,"1/100"},
                                                           {0x70 ,"1/125"},
                                                           {0x73 ,"1/160"},
                                                           {0x74 ,"1/180"},
                                                           {0x75 ,"1/200"},
                                                           {0x78 ,"1/250"},
                                                           {0x7B ,"1/320"},
                                                           {0x7C ,"1/350"},
                                                           {0x7D ,"1/400"},
                                                           {0x80 ,"1/500"},
                                                           {0x83 ,"1/640"},
                                                           {0x84 ,"1/750"},
                                                           {0x85 ,"1/800"},
                                                           {0x88 ,"1/1000"},
                                                           {0x8B ,"1/1250"},
                                                           {0x8C ,"1/1500"},
                                                           {0x8D ,"1/1600"},
                                                           {0x90 ,"1/2000"},
                                                           {0x93 ,"1/2500"},
                                                           {0x94 ,"1/3000"},
                                                           {0x95 ,"1/3200"},
                                                           {0x98 ,"1/4000"},
                                                           {0x9B ,"1/5000"},
                                                           {0x9C ,"1/6000"},
                                                           {0x9D ,"1/6400"},
                                                           {0xA0 ,"1/8000"},
                                                         };

        public CanonSDKBase()
        {

        }

        public override bool CaptureInSdRam
        {
            get { return base.CaptureInSdRam; }
            set
            {
                base.CaptureInSdRam = value;
                try
                {
                    if (base.CaptureInSdRam)
                    {
                        Camera.SavePicturesToCamera();
                    }
                    else
                    {
                        Camera.SavePicturesToHost(Path.GetTempPath());
                    }
                }
                catch (Exception exception)
                {
                    Log.Error("Error set CaptureInSdram", exception);
                }
            }
        }


        public bool Init(EosCamera camera)
        {
            try
            {
                Camera = camera;
                DeviceName = Camera.DeviceDescription;
                Manufacturer = "Canon Inc.";
                Camera.Error += _camera_Error;
                Camera.Shutdown += _camera_Shutdown;
                Camera.LiveViewPaused += Camera_LiveViewPaused;
                Camera.LiveViewUpdate += Camera_LiveViewUpdate;
                Camera.PictureTaken += Camera_PictureTaken;
                Camera.PropertyChanged += Camera_PropertyChanged;
                Capabilities.Add(CapabilityEnum.Bulb);
                Capabilities.Add(CapabilityEnum.LiveView);
                IsConnected = true;
                CaptureInSdRam = true;
                InitShutterSpeed();
                InitOther();
                return true; 
            }
            catch (Exception exception)
            {
                Log.Error("Error initialize EOS camera object ", exception);
                return false;
            }
        }


        private void InitOther()
        {
            LiveViewImageZoomRatio = new PropertyValue<int> { Name = "LiveViewImageZoomRatio" };
            LiveViewImageZoomRatio.AddValues("All", 0);
            LiveViewImageZoomRatio.AddValues("25%", 1);
            LiveViewImageZoomRatio.AddValues("33%", 2);
            LiveViewImageZoomRatio.AddValues("50%", 3);
            LiveViewImageZoomRatio.AddValues("66%", 4);
            LiveViewImageZoomRatio.AddValues("100%", 5);
            LiveViewImageZoomRatio.AddValues("200%", 6);
            LiveViewImageZoomRatio.SetValue("All");
            LiveViewImageZoomRatio.ValueChanged += LiveViewImageZoomRatio_ValueChanged;
        }

        void LiveViewImageZoomRatio_ValueChanged(object sender, string key, int val)
        {
            
        }


        void Camera_PropertyChanged(object sender, EosPropertyEventArgs e)
        {
            try
            {
                //Log.Debug("Property changed " + e.PropertyId.ToString("X"));
                switch (e.PropertyId)
                {
                    case Edsdk.PropID_Tv:
                        ShutterSpeed.SetValue(Camera.GetProperty(Edsdk.PropID_Tv));
                        break;
                }
            }
            catch (Exception exception)
            {
                Log.Error("Error set property " + e.PropertyId.ToString("X"), exception);
            }
        }

        void Camera_PictureTaken(object sender, EosImageEventArgs e)
        {
            try
            {

                Log.Debug("Picture taken event received type" + e.GetType().ToString());
                PhotoCapturedEventArgs args = new PhotoCapturedEventArgs
                                                  {
                                                      WiaImageItem = null,
                                                      //EventArgs =
                                                      //  new PortableDeviceEventArgs(new PortableDeviceEventType()
                                                      //  {
                                                      //      ObjectHandle =
                                                      //        (uint)longeventParam
                                                      //  }),
                                                      CameraDevice = this,
                                                      FileName = "IMG0000.jpg",
                                                      Handle = e
                                                  };

                EosFileImageEventArgs file = e as EosFileImageEventArgs;
                if (file != null)
                {
                    args.FileName = Path.GetFileName(file.ImageFilePath);
                }
                EosMemoryImageEventArgs memory = e as EosMemoryImageEventArgs;
                if (memory != null)
                {
                    if (!string.IsNullOrEmpty(memory.FileName))
                        args.FileName = Path.GetFileName(memory.FileName);
                }
                OnPhotoCapture(this, args);
            }
            catch (Exception exception)
            {
                Log.Error("EOS Picture taken event error", exception);
            }

        }

        void Camera_LiveViewUpdate(object sender, EosLiveImageEventArgs e)
        {
            LiveViewData viewData = new LiveViewData();
            if (Monitor.TryEnter(Locker, 1))
            {
                try
                {
                    _liveViewImageData = e.ImageData;
                    Log.Debug("live data length " + e.ImageData.Length);
                }
                catch (Exception exception)
                {
                    Log.Error("Error get live view image event", exception);
                }
                finally
                {
                    Monitor.Exit(Locker);
                }
            }

        }

        void Camera_LiveViewPaused(object sender, EventArgs e)
        {
            try
            {
                Camera.TakePicture();
                Camera.ResumeLiveview();
            }
            catch (Exception exception)
            {
                Log.Debug("Live view pause error", exception);
            }
        }

        void _camera_Shutdown(object sender, EventArgs e)
        {
            IsConnected = false;
            OnCameraDisconnected(this, new DisconnectCameraEventArgs { StillImageDevice = null });
        }

        void _camera_Error(object sender, EosExceptionEventArgs e)
        {
            try
            {
                Log.Error("Canon error", e.Exception);
            }
            catch (Exception exception)
            {
                Log.Error("Error get camera error", exception);
            }
        }

        public override bool Init(DeviceDescriptor deviceDescriptor)
        {
            //StillImageDevice = new StillImageDevice(deviceDescriptor.WpdId);
            //StillImageDevice.ConnectToDevice(AppName, AppMajorVersionNumber, AppMinorVersionNumber);
            //StillImageDevice.DeviceEvent += _stillImageDevice_DeviceEvent;
            Capabilities.Add(CapabilityEnum.Bulb);
            Capabilities.Add(CapabilityEnum.LiveView);
            
            IsConnected = true;
            return true;
        }

        private void InitShutterSpeed()
        {
            ShutterSpeed = new PropertyValue<long>();
            ShutterSpeed.Name = "ShutterSpeed";
            ShutterSpeed.ValueChanged += ShutterSpeed_ValueChanged;
            ReInitShutterSpeed();
        }

        private void ReInitShutterSpeed()
        {
            lock (Locker)
            {
                try
                {
                    byte datasize = 4;
                    ShutterSpeed.Clear();
                    foreach (KeyValuePair<uint, string> keyValuePair in _shutterTable)
                    {
                        ShutterSpeed.AddValues(keyValuePair.Value, keyValuePair.Key);
                    }
                    //byte[] result = StillImageDevice.ExecuteReadData(CONST_CMD_GetDevicePropDesc, CONST_PROP_EOS_ShutterSpeed);
                    //int type = BitConverter.ToInt16(result, 2);
                    //byte formFlag = result[(2 * datasize) + 5];
                    //UInt32 defval = BitConverter.ToUInt32(result, datasize + 5);
                    //for (int i = 0; i < result.Length - ((2 * datasize) + 6 + 2); i += datasize)
                    //{
                    //    UInt32 val = BitConverter.ToUInt32(result, ((2 * datasize) + 6 + 2) + i);
                    //    ShutterSpeed.AddValues(_shutterTable.ContainsKey(val) ? _shutterTable[val] : val.ToString(), val);
                    //}
                    ShutterSpeed.SetValue(Camera.GetProperty(Edsdk.PropID_Tv));
                }
                catch (Exception ex)
                {
                    Log.Debug("EOS Shutter speed init", ex);
                }
            }
        }

        void ShutterSpeed_ValueChanged(object sender, string key, long val)
        {
            try
            {
                Camera.SetProperty(Edsdk.PropID_Tv, val);
            }
            catch (Exception exception)
            {
                Log.Error("Error set property sP", exception);
            }
        }

        public override void CapturePhoto()
        {
            Log.Debug("EOS capture start");
            Monitor.Enter(Locker);
            try
            {
                IsBusy = true;
                if (Camera.IsInHostLiveViewMode)
                {
                    Camera.TakePictureInLiveview();
                }
                else
                {
                    Camera.TakePicture();                    
                }

            }
            catch (COMException comException)
            {
                IsBusy = false;
                ErrorCodes.GetException(comException);
            }
            catch
            {
                IsBusy = false;
                throw;
            }
            finally
            {
                Monitor.Exit(Locker);
            }
            Log.Debug("EOS capture end");
        }

        public override void CapturePhotoNoAf()
        {
            CapturePhoto();
        }

        public override void StartBulbMode()
        {
            Camera.BulbStart();
        }

        public override void EndBulbMode()
        {
            Camera.BulbStart();
        }

        public override LiveViewData GetLiveViewImage()
        {
            LiveViewData viewData = new LiveViewData();
            if (Monitor.TryEnter(Locker, 1))
            {
                try
                {
                    //DeviceReady();
                    viewData.HaveFocusData = false;
                    viewData.ImagePosition = 0;
                    viewData.ImageData = _liveViewImageData;
                    viewData.ImageHeight = 100;
                    viewData.ImageWidth = 100;
                    viewData.LiveViewImageHeight = 100;
                    viewData.LiveViewImageWidth = 100;
                }
                catch (Exception e)
                {
                    Log.Error("Error get live view image ", e);
                }
                finally
                {
                    Monitor.Exit(Locker);
                }
            }
            return viewData;
        }

        public override void StartLiveView()
        {
            //if (!Camera.IsInLiveViewMode) 
                Camera.StartLiveView();
        }

        public override void StopLiveView()
        {
            //if (Camera.IsInHostLiveViewMode)
                Camera.StopLiveView();
        }

        public override void TransferFile(object o, string filename)
        {
            EosFileImageEventArgs file = o as EosFileImageEventArgs;
            if (file != null)
            {
                Log.Debug("File transfer started");
                try
                {
                    if(File.Exists(file.ImageFilePath))
                    {
                        File.Copy(file.ImageFilePath, filename, true);
                        File.Delete(file.ImageFilePath);
                    }
                    else
                    {
                        Log.Error("Base file not found " + file.ImageFilePath);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error("Transfer error ", exception);
                }
            }
            EosMemoryImageEventArgs memory = o as EosMemoryImageEventArgs;
            if(memory!=null)
            {
                Log.Debug("Memory file transfer started");
                try
                {
                    using (FileStream fileStream = File.Create(filename, (int)memory.ImageData.Length))
                    {
                        fileStream.Write(memory.ImageData, 0, memory.ImageData.Length);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error("Error transfer memory file", exception);
                }
            }
        }

        public override string GetProhibitionCondition(OperationEnum operationEnum)
        {
            return "";
        }



    }
}
