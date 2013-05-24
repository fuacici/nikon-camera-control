using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CameraControl.Core.Classes;
using CameraControl.Core.Translation;
using CameraControl.Devices;
using CameraControl.Devices.Classes;

namespace CameraControl.Core.Scripting
{
    public class ScriptObject : BaseFieldClass
    {
        private AsyncObservableCollection<IScriptCommand> _commands;
        public AsyncObservableCollection<IScriptCommand> Commands
        {
            get { return _commands; }
            set
            {
                _commands = value;
                NotifyPropertyChanged("Commands");
            }
        }

        private bool _useExternal;
        public bool UseExternal
        {
            get { return _useExternal; }
            set
            {
                _useExternal = value;
                NotifyPropertyChanged("UseExternal");
            }
        }

        private CustomConfig _selectedConfig;
        public CustomConfig SelectedConfig
        {
            get { return _selectedConfig; }
            set
            {
                _selectedConfig = value;
                NotifyPropertyChanged("SelectedConfig");
            }
        }

        private ICameraDevice _cameraDevice;
        public ICameraDevice CameraDevice
        {
            get { return _cameraDevice; }
            set
            {
                _cameraDevice = value;
                NotifyPropertyChanged("CameraDevice");
            }
        }

        public ScriptObject()
        {
            Commands = new AsyncObservableCollection<IScriptCommand>();
            UseExternal = false;
        }

        public void StartCapture()
        {
            Thread thread = new Thread(StartCaptureThread);
            thread.Start();   
        }

        public void StopCapture()
        {
            Thread thread = new Thread(StopCaptureThread);
            thread.Start();
        }

        private void StopCaptureThread()
        {
            bool retry;
            do
            {
                retry = false;
                try
                {
                    if (UseExternal)
                    {
                        if (SelectedConfig != null)
                        {
                            ServiceProvider.ExternalDeviceManager.Stop(SelectedConfig);
                        }
                        else
                        {
                            StaticHelper.Instance.SystemMessage = TranslationStrings.LabelNoExternalDeviceSelected;
                        }
                    }
                    else
                    {
                        if (CameraDevice.GetCapability(CapabilityEnum.Bulb))
                        {
                            CameraDevice.EndBulbMode();
                        }
                        else
                        {
                            StaticHelper.Instance.SystemMessage = TranslationStrings.MsgBulbModeNotSupported;
                        }
                    }
                    StaticHelper.Instance.SystemMessage = "Capture done";
                    Log.Debug("Bulb capture done");
                }
                catch (DeviceException deviceException)
                {
                    if (deviceException.ErrorCode == ErrorCodes.ERROR_BUSY)
                        retry = true;
                    else
                    {
                        StaticHelper.Instance.SystemMessage = deviceException.Message;
                        Log.Error("Bulb done", deviceException);
                    }

                }
                catch (Exception exception)
                {
                    StaticHelper.Instance.SystemMessage = exception.Message;
                    Log.Error("Bulb done", exception);
                }
            } while (retry);
        }

        void StartCaptureThread()
        {
            bool retry;
            do
            {
                retry = false;
                try
                {
                    Log.Debug("Bulb capture started");
                    if (UseExternal)
                    {
                        if (SelectedConfig != null)
                        {
                            ServiceProvider.ExternalDeviceManager.Start(SelectedConfig);
                        }
                        else
                        {
                            StaticHelper.Instance.SystemMessage = TranslationStrings.LabelNoExternalDeviceSelected;
                        }
                    }
                    else
                    {
                        if (CameraDevice.GetCapability(CapabilityEnum.Bulb))
                        {
                            CameraDevice.LockCamera();
                            CameraDevice.StartBulbMode();
                        }
                        else
                        {
                            StaticHelper.Instance.SystemMessage = TranslationStrings.MsgBulbModeNotSupported;
                        }
                    }
                }
                catch (DeviceException deviceException)
                {
                    if (deviceException.ErrorCode == ErrorCodes.ERROR_BUSY)
                        retry = true;
                    else
                    {
                        StaticHelper.Instance.SystemMessage = deviceException.Message;
                        Log.Error("Bulb start", deviceException);
                    }
                }
                catch (Exception exception)
                {
                    StaticHelper.Instance.SystemMessage = exception.Message;
                    Log.Error("Bulb start", exception);
                }
            } while (retry);
        }

    }
}
