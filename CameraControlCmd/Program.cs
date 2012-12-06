using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using CameraControlCmd.Classes;
using PortableDeviceLib;

namespace CameraControlCmd
{
  class Program
  {
    //public static bool IsBusy { get; set; }

    private static InputArguments _arguments;
    static void Main(string[] args)
    {
      Console.WriteLine("digiCamControl command line utility");
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
      ExecuteArgs();
      while (CamerasAreBusy())
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
            foreach (ICameraDevice cameraDevice in ServiceProvider.DeviceManager.ConnectedDevices)
            {
              preset.Set(cameraDevice);              
            }
          }
          else
          {
            Console.WriteLine("Preset not found {0}!", _arguments["preset"]);
          }
        }
        if (_arguments.Contains("folder"))
        {
          if (string.IsNullOrEmpty(_arguments["folder"]))
          {
            Console.WriteLine("No folder !!!");
          }
          else
          {
            ServiceProvider.Settings.DefaultSession.Folder = _arguments["folder"];
          }
        }
        if (_arguments.Contains("filename"))
        {
          if (string.IsNullOrEmpty(_arguments["filename"]))
          {
            Console.WriteLine("Wrong filename !!!");
          }
          else
          {
            ServiceProvider.Settings.DefaultSession.FileNameTemplate = _arguments["filename"];
          }
        }
        if (_arguments.Contains("counter"))
        {
          int i = 0;
          if (string.IsNullOrEmpty(_arguments["counter"]) && !int.TryParse(_arguments["counter"], out i))
          {
            Console.WriteLine("Wrong counter !!!");
          }
          else
          {
            ServiceProvider.Settings.DefaultSession.Counter = i;
          }
        }
        if (_arguments.Contains("capture"))
        {
          ServiceProvider.DeviceManager.SelectedCameraDevice.CapturePhoto();
          return;
        }
        if (_arguments.Contains("capturenoaf"))
        {
          ServiceProvider.DeviceManager.SelectedCameraDevice.CapturePhoto();
          return;
        }
        if (_arguments.Contains("captureall"))
        {
          foreach (ICameraDevice cameraDevice in ServiceProvider.DeviceManager.ConnectedDevices)
          {
            ICameraDevice device = cameraDevice;
            new Thread(device.CapturePhoto).Start();
          }
        }
        if (_arguments.Contains("captureallnoaf"))
        {
          foreach (ICameraDevice cameraDevice in ServiceProvider.DeviceManager.ConnectedDevices)
          {
            ICameraDevice device = cameraDevice;
            new Thread(device.CapturePhotoNoAf).Start();
          }
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
      Console.WriteLine(" /captureall               - capture photo with all connected devices");
      Console.WriteLine(" /captureallnoaf           - capture photo without autofocus with all connected devices");
      Console.WriteLine(" /session session_name     - use session [session_name]");
      Console.WriteLine(" /preset preset_name       - use preset [session_name]");
      Console.WriteLine(" /folder path              - set the photo save folder" );
      Console.WriteLine(" /filename filename        - set the photo save file name template");
      Console.WriteLine(" /counter number           - set the photo initial counter");
      Console.WriteLine(" /wait                     - after done wait for a keypress ");
    }

    private static void InitApplication()
    {
      ServiceProvider.Configure();
      Log.Debug("Command line utility started");
      ServiceProvider.Settings = new Settings();
      ServiceProvider.Settings = ServiceProvider.Settings.Load();
      ServiceProvider.Settings.LoadSessionData();
      ServiceProvider.WindowsManager = new WindowsManager();
      //WIAManager manager = new WIAManager();
      StaticHelper.Instance.PropertyChanged += Instance_PropertyChanged;
      ServiceProvider.DeviceManager.CameraConnected += DeviceManagerCameraConnected;
      ServiceProvider.DeviceManager.ConnectToCamera();
      ServiceProvider.DeviceManager.PhotoCaptured += DeviceManager_PhotoCaptured;
      if (ServiceProvider.DeviceManager.SelectedCameraDevice.AttachedPhotoSession != null)
        ServiceProvider.Settings.DefaultSession =(PhotoSession)
          ServiceProvider.DeviceManager.SelectedCameraDevice.AttachedPhotoSession;
      foreach (ICameraDevice cameraDevice in ServiceProvider.DeviceManager.ConnectedDevices)
      {
        cameraDevice.CaptureCompleted += SelectedCameraDevice_CaptureCompleted;
      }
      //ServiceProvider.DeviceManager.SelectedCameraDevice.CaptureCompleted += SelectedCameraDevice_CaptureCompleted;
    }

    static void SelectedCameraDevice_CaptureCompleted(object sender, EventArgs e)
    {
      ICameraDevice device = sender as ICameraDevice;
      device.IsBusy = false;
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
        CameraProperty property = ServiceProvider.Settings.CameraProperties.Get(eventArgs.CameraDevice);
        if ((property.NoDownload && !eventArgs.CameraDevice.CaptureInSdRam))
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
              StaticHelper.GetUniqueFilename(
                Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_", 0,
                Path.GetExtension(fileName));
        }
        if (!Directory.Exists(Path.GetDirectoryName(fileName)))
        {
          Directory.CreateDirectory(Path.GetDirectoryName(fileName));
        }
        Console.WriteLine("Transfer started :" + fileName);
        eventArgs.CameraDevice.TransferFile(eventArgs.Handle, fileName);
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

    private static bool CamerasAreBusy()
    {
      return ServiceProvider.DeviceManager.ConnectedDevices.Aggregate(false, (current, connectedDevice) => connectedDevice.IsBusy || current);
    }

  }
}
