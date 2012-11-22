using System.Runtime.InteropServices;

namespace CameraControl.Devices.Classes
{
  public class Conts
  {
    [MarshalAs(UnmanagedType.LPStr)]
    public const string wiaEventDeviceConnected = "{A28BBADE-64B6-11D2-A231-00C04FA31809}";
    [MarshalAs(UnmanagedType.LPStr)]
    public const string wiaEventDeviceDisconnected = "{143E4E83-6497-11D2-A231-00C04FA31809}";
    [MarshalAs(UnmanagedType.LPStr)]
    public const string wiaEventItemCreated = "{4C8F4EF5-E14F-11D2-B326-00C04F68CE61}";
    [MarshalAs(UnmanagedType.LPStr)]
    public const string wiaEventItemDeleted = "{1D22A559-E14F-11D2-B326-00C04F68CE61}";
    [MarshalAs(UnmanagedType.LPStr)]
    public const string wiaCommandTakePicture = "{AF933CAC-ACAD-11D2-A093-00C04F72DC3C}";
  }
}
