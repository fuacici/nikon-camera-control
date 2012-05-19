using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FreeImageAPI;

namespace CameraControl.Classes
{
  public class PhotoUtils
  {
    public static void CopyPhotoScale(string sourse, string dest, double scale)
    {
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
        FIBITMAP resized = FreeImage.Rescale(bmp, (int) dw, (int) dh, FREE_IMAGE_FILTER.FILTER_LANCZOS3);
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
        if (scale == 1 && Path.GetExtension(sourse).ToUpper() == Path.GetExtension(dest).ToUpper())
        {
          File.Copy(sourse, dest, true);
        }
        else
        {
          FIBITMAP resized = FreeImage.Rescale(dib, (int) dw, (int) dh, FREE_IMAGE_FILTER.FILTER_BILINEAR);
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
  }
}
