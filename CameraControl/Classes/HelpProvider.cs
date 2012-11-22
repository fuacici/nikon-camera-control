using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Core.Classes;

namespace CameraControl.Classes
{
  public enum HelpSections
  {
    MainMenu,
    FocusStacking,
    Bracketig,
    Settings,
    TimeLapse,
    LiveView,
    Session,
    Bulb,
    MultipleCamera,
    DownloadPhotos
  }

  public class HelpProvider
  {
    private static Dictionary<HelpSections, string> _helpData;

    private static void Init()
    {
      _helpData = new Dictionary<HelpSections, string>
                    {
                      {HelpSections.MainMenu, "http://www.digicamcontrol.com/wiki/index.php/User_guide"},
                      {HelpSections.Bracketig, "http://www.digicamcontrol.com/wiki/index.php/Interface#Bracketing"},
                      {HelpSections.FocusStacking, "http://nccsoftware.blogspot.ro/2012/06/how-to-focus-stacking.html"},
                      {HelpSections.Settings, "http://www.digicamcontrol.com/wiki/index.php/Configuration"},
                      {HelpSections.TimeLapse, "http://www.digicamcontrol.com/wiki/index.php/Interface#Time_lapse"},
                      {HelpSections.LiveView, "http://www.digicamcontrol.com/wiki/index.php/Interface#Live_view"},
                      {HelpSections.Session, "http://www.digicamcontrol.com/wiki/index.php/Configuration#Session"},
                      {HelpSections.Bulb, "http://www.digicamcontrol.com/wiki/index.php/Interface#Bulb_mode"},
                      {HelpSections.MultipleCamera, "http://www.digicamcontrol.com/wiki/index.php/Interface#Multiple_camera_support"},
                      {HelpSections.DownloadPhotos, "http://www.digicamcontrol.com/wiki/index.php/Interface#Download_photos"},
                    };
    }


    public static void Run(HelpSections sections)
    {
      if(_helpData==null)
        Init();
      PhotoUtils.Run(_helpData[sections], "");
    }
  }
}
