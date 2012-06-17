using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using FreeImageAPI;

namespace CameraControl.Classes
{
  public class PhotoUtils
  {
    public  static bool  RunAndWait(string exe, string param)
    {
      try
      {
        ProcessStartInfo startInfo = new ProcessStartInfo(exe);
        startInfo.WindowStyle = ProcessWindowStyle.Minimized;
        startInfo.Arguments = param;
        Process process = Process.Start(startInfo);
        process.WaitForExit();
      }
      catch (Exception)
      {
        return false;
      }
      return true;
    }

    public static bool Run(string exe, string param)
    {
      try
      {
        ProcessStartInfo startInfo = new ProcessStartInfo(exe);
        startInfo.WindowStyle = ProcessWindowStyle.Minimized;
        startInfo.Arguments = param;
        Process process = Process.Start(startInfo);
        //process.WaitForExit();
      }
      catch (Exception)
      {
        return false;
      }
      return true;
    }

    public static void CopyPhotoScale(string sourse, string dest, double scale)
    {
      if (scale == 1 && Path.GetExtension(sourse).ToUpper() == Path.GetExtension(dest).ToUpper())
      {
        File.Copy(sourse, dest, true);
        return;
      }

      string newfile = dest;
      FIBITMAP dib = FreeImage.LoadEx(sourse);

      uint dw = FreeImage.GetWidth(dib);
      uint dh = FreeImage.GetHeight(dib);
      int tw = (int) (dw*scale);
      int th = (int) (dh*scale);
      double zw = (tw/(double) dw);
      double zh = (th/(double) dh);
      double z = 0;
      z = ((zw <= zh) ? zw : zh);
      dw = (uint) (dw*z);
      dh = (uint) (dh*z);
      int difw = (int) (tw - dw);
      int difh = (int) (th - dh);
      if (FreeImage.GetFileType(sourse, 0) == FREE_IMAGE_FORMAT.FIF_RAW)
      {
        FIBITMAP bmp = FreeImage.ToneMapping(dib, FREE_IMAGE_TMO.FITMO_REINHARD05, 0, 0);
        // ConvertToType(dib, FREE_IMAGE_TYPE.FIT_BITMAP, false);
        FIBITMAP resized = FreeImage.Rescale(bmp, (int) dw, (int) dh, FREE_IMAGE_FILTER.FILTER_CATMULLROM);
        FIBITMAP final = FreeImage.EnlargeCanvas<RGBQUAD>(resized, difw/2, difh/2, difw - (difw/2), difh - (difh/2),
                                                          new RGBQUAD(System.Drawing.Color.Black),
                                                          FREE_IMAGE_COLOR_OPTIONS.FICO_RGB);
        FreeImage.SaveEx(final, newfile);
        FreeImage.UnloadEx(ref final);
        FreeImage.UnloadEx(ref resized);
        FreeImage.UnloadEx(ref dib);
        FreeImage.UnloadEx(ref bmp);
      }
      else
      {
        {
          FIBITMAP resized = FreeImage.Rescale(dib, (int) dw, (int) dh, FREE_IMAGE_FILTER.FILTER_CATMULLROM);
          FIBITMAP final = FreeImage.EnlargeCanvas<RGBQUAD>(resized, difw/2, difh/2, difw - (difw/2), difh - (difh/2),
                                                            new RGBQUAD(System.Drawing.Color.Black),
                                                            FREE_IMAGE_COLOR_OPTIONS.FICO_RGB);
          FreeImage.SaveEx(final, newfile);
          FreeImage.UnloadEx(ref final);
          FreeImage.UnloadEx(ref resized);
          FreeImage.UnloadEx(ref dib);
        }
      }
    }

    public static string GetUniqueFilename(string prefix, int counter, string sufix)
    {
      string file = prefix  + counter + sufix;
      if (File.Exists(file))
      {
        return GetUniqueFilename(prefix, counter + 1, sufix);
      }
      return file;
    }

    public static bool CheckForUpdate()
    {
      try
      {
        string tempfile = Path.GetTempFileName();
        using (WebClient client = new WebClient())
        {
          client.DownloadFile("http://nikon-camera-control.googlecode.com/svn/trunk/versioninfo.xml", tempfile);
        }

        XmlDocument document = new XmlDocument();
        document.Load(tempfile);
        string ver = document.SelectSingleNode("application/version").InnerText;
        string url = "http://code.google.com/p/nikon-camera-control/downloads/list";
        var selectSingleNode = document.SelectSingleNode("application/url");
        if (selectSingleNode != null)
          url = selectSingleNode.InnerText;
        Version v_ver = new Version(ver);
        Version app_ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        if (v_ver > System.Reflection.Assembly.GetExecutingAssembly().GetName().Version)
        {
          if (
            MessageBox.Show("New version of application released\nDo you want to download?", "Update",
                            MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
          {
            System.Diagnostics.Process.Start(url);
            return true;
          }
        }
        File.Delete(tempfile);
      }
      catch (Exception exception)
      {
        ServiceProvider.Log.Error("Error download update information", exception);
      }
      return false;
    }

    /// <summary>
    /// Return serial number component from a pnp id string
    /// </summary>
    /// <param name="pnpstring"></param>
    /// <returns></returns>
    public static string GetSerial(string pnpstring)
    {
      if (pnpstring == null)
        return "";
      string ret = "";
      if(pnpstring.Contains("#"))
      {
        string[] s = pnpstring.Split('#');
        if(s.Length>2)
        {
          ret = s[2];
        }
      }
      return ret;
    }


  }
}
