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
                                                           {0, "30"},
                                                           {1, "25"},
                                                           {2, "20"},
                                                           {3, "15"},
                                                           {4, "13"},
                                                           {5, "10"},
                                                           {6, "8"},
                                                           {7, "6"},
                                                           {8, "5"},
                                                           {9, "4"},
                                                           {10, "3.2"},
                                                           {11, "2.5"},
                                                           {12, "2"},
                                                           {13, "1.6"},
                                                           {14, "1.3"},
                                                           {15, "1"},
                                                           {16, "0.8"},
                                                           {17, "0.6"},
                                                           {18, "0.5"},
                                                           {19, "0.4"},
                                                           {20, "0.3"},
                                                           {21, "1/4"},
                                                           {22, "1/5"},
                                                           {23, "1/6"},
                                                           {24, "1/8"},
                                                           {25, "1/10"},
                                                           {26, "1/13"},
                                                           {27, "1/15"},
                                                           {28, "1/20"},
                                                           {29, "1/25"},
                                                           {30, "1/30"},
                                                           {31, "1/40"},
                                                           {32, "1/50"},
                                                           {33, "1/60"},
                                                           {34, "1/80"},
                                                           {35, "1/100"},
                                                           {36, "1/125"},
                                                           {37, "1/160"},
                                                           {38, "1/200"},
                                                           {39, "1/250"},
                                                           {40, "1/320"},
                                                           {41, "1/400"},
                                                           {42, "1/500"},
                                                           {43, "1/640"},
                                                           {44, "1/800"},
                                                           {45, "1/1000"},
                                                           {46, "1/1250"},
                                                           {47, "1/1600"},
                                                           {48, "1/2000"},
                                                           {49, "1/2500"},
                                                           {50, "1/3200"},
                                                           {51, "1/4000"},
                                                           {52, "1/5000"},
                                                           {53, "1/6400"},
                                                           {54, "1/8000"}
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
                return true; 
            }
            catch (Exception exception)
            {
                Log.Error("Error initialize EOS camera object ", exception);
                return false;
            }
        }

        void Camera_PropertyChanged(object sender, EosPropertyEventArgs e)
        {
            try
            {
                Log.Debug("Property changed " + e.PropertyId.ToString("X"));
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
                Handle =e
            };
            OnPhotoCapture(this, args);
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

            InitShutterSpeed();
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
            //ErrorCodes.GetException(ExecuteWithNoData(CONST_CMD_CANON_EOS_BulbStart));
        }

        public override void EndBulbMode()
        {
            //ErrorCodes.GetException(ExecuteWithNoData(CONST_CMD_CANON_EOS_BulbEnd));
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
            if (!Camera.IsInHostLiveViewMode) Camera.StartLiveView();
        }

        public override void StopLiveView()
        {
            if (Camera.IsInHostLiveViewMode) Camera.StopLiveView();
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

    }
}
