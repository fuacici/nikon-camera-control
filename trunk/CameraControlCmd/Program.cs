using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Devices;
using CameraControl.Core.Devices.Classes;
using CameraControl.Core.Interfaces;
using CameraControlCmd.Classes;

namespace CameraControlCmd
{
  class Program
  {
    public static bool IsBusy { get; set; }

    private static InputArguments _arguments;
    static void Main(string[] args)
    {
      Console.WriteLine("NCC command line utility");
      Console.WriteLine();
      _arguments = new InputArguments(args,"/");
      if (!args.Any() || _arguments.Contains("help"))
      {
        ShowHelp();
        Console.ReadLine();
        return;
      }
      InitApplication();
      if (ServiceProvider.DeviceManager.ConnectedDevices.Count == 0)
      {
        Console.WriteLine("No connected device was found ! Exiting");
        return;
      }
      IsBusy = false;
      ExecuteArgs();
      while (IsBusy)
      {
        Thread.Sleep(1);
      }
      if (_arguments.Contains("wait"))
      {
        Console.Write("Press any key ...");
        Console.ReadLine();
      }
    }

    static void ExecuteArgs()
    {
      try
      {
        if (_arguments.Contains("session"))
        {
          PhotoSession session = ServiceProvider.Settings.GetSession(_arguments["session"]);
          if(session!=null)
          {
            Console.WriteLine("Using session {0}", _arguments["session"]);
            ServiceProvider.Settings.DefaultSession = session;
          }
          else
          {
            Console.WriteLine("Session not found {0}! Using default session", _arguments["session"]);
          }
        }
        if (_arguments.Contains("preset"))
        {
          CameraPreset preset = ServiceProvider.Settings.GetPreset(_arguments["preset"]);
          if (preset != null)
          {
            Console.WriteLine("Using preset {0}", _arguments["preset"]);
            preset.Set(ServiceProvider.DeviceManager.SelectedCameraDevice);
          }
          else
          {
            Console.WriteLine("Preset not found {0}!", _arguments["preset"]);
          }
        }
        if (_arguments.Contains("capture"))
        {
          ServiceProvider.DeviceManager.SelectedCameraDevice.CapturePhoto();
          IsBusy = true;
          return;
        }
        if (_arguments.Contains("capturenoaf"))
        {
          ServiceProvider.DeviceManager.SelectedCameraDevice.CapturePhoto();
          IsBusy = true;
          return;
        }

      }
      catch (Exception exception)
      {
        Log.Error(exception);
        Console.WriteLine(exception.Message);
      }
    }

    private static void ShowHelp()
    {
      Console.WriteLine("Arguments :");
      Console.WriteLine(" /help                     - this screen");
      Console.WriteLine(" /capture                  - capture photo");
      Console.WriteLine(" /capturenoaf              - capture photo without autofocus");
      Console.WriteLine(" /session session_name     - use session [session_name]");
      Console.WriteLine(" /preset preset_name       - use preset [session_name]");
      Console.WriteLine(" /wait                     - after done wait for keypress ");
    }

    private static void InitApplication()
    {
      ServiceProvider.Configure();
      ServiceProvider.Settings = new Settings();
      ServiceProvider.Settings = ServiceProvider.Settings.Load();
      ServiceProvider.Settings.LoadSessionData();
      ServiceProvider.WindowsManager = new WindowsManager();
      WIAManager manager=new WIAManager();
      StaticHelper.Instance.PropertyChanged += Instance_PropertyChanged;
      ServiceProvider.DeviceManager.CameraConnected += DeviceManagerCameraConnected;
      manager.ConnectToCamera();
      ServiceProvider.DeviceManager.PhotoCaptured += DeviceManager_PhotoCaptured;
      if (ServiceProvider.DeviceManager.SelectedCameraDevice.AttachedPhotoSession != null)
        ServiceProvider.Settings.DefaultSession =
          ServiceProvider.DeviceManager.SelectedCameraDevice.AttachedPhotoSession;
      ServiceProvider.DeviceManager.SelectedCameraDevice.CaptureCompleted += SelectedCameraDevice_CaptureCompleted;
    }

    static void SelectedCameraDevice_CaptureCompleted(object sender, EventArgs e)
    {
      IsBusy = false;
    }

    static void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if(e.PropertyName=="SystemMessage")
      {
        Console.WriteLine(StaticHelper.Instance.SystemMessage);
      }
    }

    static void DeviceManagerCameraConnected(ICameraDevice cameraDevice)
    {
      CameraProperty property = ServiceProvider.Settings.CameraProperties.Get(cameraDevice);
      cameraDevice.DisplayName = property.DeviceName;
      cameraDevice.AttachedPhotoSession = ServiceProvider.Settings.GetSession(property.PhotoSessionName);
    }


    static void DeviceManager_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
    {
      PhotoCaptured(eventArgs);
    }


    static void PhotoCaptured(object o)
    {
      try
      {
        Console.WriteLine("Photo transfer begin.");
        PhotoCapturedEventArgs eventArgs = o as PhotoCapturedEventArgs;
        if (eventArgs == null)
          return;
        if ((ServiceProvider.Settings.DefaultSession.NoDownload && !eventArgs.CameraDevice.CaptureInSdRam))
          return;
        string fileName = "";
        if (!ServiceProvider.Settings.DefaultSession.UseOriginalFilename || eventArgs.CameraDevice.CaptureInSdRam)
        {
          fileName =
            ServiceProvider.Settings.DefaultSession.GetNextFileName(Path.GetExtension(eventArgs.FileName),
                                                                                    eventArgs.CameraDevice);
        }
        else
        {
          fileName = Path.Combine(ServiceProvider.Settings.DefaultSession.Folder, eventArgs.FileName);
          if (File.Exists(fileName))
            fileName =
              PhotoUtils.GetUniqueFilename(
                Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_", 0,
                Path.GetExtension(fileName));
        }
        if (!Directory.Exists(Path.GetDirectoryName(fileName)))
        {
          Directory.CreateDirectory(Path.GetDirectoryName(fileName));
        }
        Console.WriteLine("Transfer started :" + fileName);
        eventArgs.CameraDevice.TransferFile(eventArgs.EventArgs, fileName);
        Console.WriteLine("Transfer done :" + fileName);
        ServiceProvider.Settings.DefaultSession.AddFile(fileName);
        ServiceProvider.Settings.Save(ServiceProvider.Settings.DefaultSession);
        StaticHelper.Instance.SystemMessage = "Photo transfer done.";
        if (ServiceProvider.Settings.PlaySound)
        {
          PhotoUtils.PlayCaptureSound();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Transfer error !\nMessage :" + ex.Message);
        Log.Error("Transfer error !", ex);
      }
    }


  }
}
