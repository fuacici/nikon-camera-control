using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CameraControl.Classes;
using WIA;

namespace CameraControl
{
  /// <summary>
  /// Interaction logic for Controler.xaml
  /// </summary>
  public partial class Controler : UserControl
  {
    private WIAManager _manager;
    public WIAManager Manager
    {
      get { return _manager; }
      set
      {
        if (_manager != null)
          _manager.PropertyChanged -= _manager_PropertyChanged;
        _manager = value;
        DataContext = Manager;
        SetProperties();
        _manager.PropertyChanged += _manager_PropertyChanged;
      }
    }

    void _manager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if(e.PropertyName=="IsConected")
      {
        SetProperties();
      }
    }

    public Controler()
    {
      InitializeComponent();
      DataContext = Manager;
    }

    public void SetProperties()
    {
      if (!Manager.IsConected)
        return;
      try
      {
        cmb_iso.Items.Clear();
        Property isoProperty = Manager.Device.Properties["Exposure Index"];
        if (isoProperty != null)
        {
          foreach (var subTypeValue in isoProperty.SubTypeValues)
          {
            cmb_iso.Items.Add(subTypeValue);
          }
          cmb_iso.SelectedValue = isoProperty.get_Value();
        }
        cmb_shutter.Items.Clear();
        Property shutterProperty = Manager.Device.Properties["Exposure Time"];
        if (shutterProperty != null)
        {
          foreach (var subTypeValue in shutterProperty.SubTypeValues)
          {
            if (Manager.ShutterTable.ContainsKey((int) subTypeValue))
              cmb_shutter.Items.Add(Manager.ShutterTable[(int) subTypeValue]);
          }
          cmb_shutter.SelectedValue = Manager.ShutterTable[(int) shutterProperty.get_Value()];
        }
        cmb_aperture.Items.Clear();
        Property apertureProperty = Manager.Device.Properties[WIAManager.CONST_PROP_F_Number];
        if (apertureProperty != null)
        {
          Manager.FTable.Clear();
          foreach (var subTypeValue in apertureProperty.SubTypeValues)
          {
            double d = (int) subTypeValue;
            //if (Manager.ShutterTable.ContainsKey((int)subTypeValue))
            string s = "f/" + (d/100).ToString("0.0");
            Manager.FTable.Add(s, (int)subTypeValue);
            cmb_aperture.Items.Add(s);

            if ((int)subTypeValue == (int)apertureProperty.get_Value())
              cmb_aperture.SelectedValue = s;
          }
        }

        cmb_EComp.Items.Clear();
        Property ecProperty = Manager.Device.Properties["Exposure Compensation"];
        if (ecProperty != null)
        {
          int t=ecProperty.Type;
          Manager.ECTable.Clear();
          foreach (var subTypeValue in ecProperty.SubTypeValues)
          {
            int val = (int) subTypeValue;
            decimal dval = (decimal) val/1000;
            //if (Manager.ShutterTable.ContainsKey((int)subTypeValue))
            string sval = decimal.Round(dval, 1).ToString();
            if (val > 0)
              sval = "+" + sval;
            Manager.ECTable.Add(sval, val);
            cmb_EComp.Items.Add(sval);
            if ((int)ecProperty.get_Value() == (int)subTypeValue)
              cmb_EComp.SelectedValue = sval;
          }
        }

        //cmb_mode.Items.Clear();
        //foreach (string s in Manager.ExposureModeList)
        //{
        //  cmb_mode.Items.Add(s);
        //}
        //cmb_mode.SelectedValue = Manager.ExposureMode;
      }
      catch (Exception)
      {

      }

    }

    private void cmb_iso_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmb_iso.SelectedValue != null)
        Manager.Device.Properties["Exposure Index"].set_Value(cmb_iso.SelectedValue);
    }

    private void cmb_shutter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmb_shutter.SelectedValue != null)
      {
        Manager.Device.Properties["Exposure Time"].set_Value(Manager.GetShutterValue((string) cmb_shutter.SelectedValue));
      }
    }

    private void cmb_aperture_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmb_aperture.SelectedValue != null)
      {
        Manager.Device.Properties[WIAManager.CONST_PROP_F_Number].set_Value(Manager.FTable[(string)cmb_aperture.SelectedValue]);
      }
    }

    private void cmb_mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }

    private void cmb_EComp_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (cmb_EComp.SelectedValue != null)
      {
        Property ecProperty = Manager.Device.Properties["Exposure Compensation"];
        if (ecProperty != null)
        {
          foreach (var subTypeValue in ecProperty.SubTypeValues)
          {
            int val = (int)subTypeValue;
            decimal dval = (decimal)val / 1000;
            //if (Manager.ShutterTable.ContainsKey((int)subTypeValue))
            string sval = decimal.Round(dval, 1).ToString();
            if (val > 0)
              sval = "+" + sval;
            if ((string)cmb_EComp.SelectedValue == sval)
              Manager.Device.Properties["Exposure Compensation"].set_Value(subTypeValue);
          }
        }
        //Manager.Device.Properties["Exposure Compensation"].set_Value((int)Manager.ECTable[(string)cmb_EComp.SelectedValue]);
      }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      SetProperties();
    }


  }
}
