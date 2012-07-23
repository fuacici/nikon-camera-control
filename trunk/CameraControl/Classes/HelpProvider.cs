using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CameraControl.Classes
{
  public enum HelpSections
  {
    FocusStacking,
    Bracketig
  }

  public class HelpProvider
  {
    private static Dictionary<HelpSections, string> _helpData;

    private static void Init()
    {
      _helpData = new Dictionary<HelpSections, string>
                    {
                      {HelpSections.Bracketig, "http://nccsoftware.blogspot.ro/2012/07/how-to-exposure-blending.html"},
                      {HelpSections.FocusStacking, "http://nccsoftware.blogspot.ro/2012/06/how-to-focus-stacking.html"}
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
