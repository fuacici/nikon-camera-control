﻿using System;
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

namespace CameraControl.Test
{
  /// <summary>
  /// Interaction logic for FocusControler.xaml
  /// </summary>
  public partial class FocusControler : UserControl
  {
    public FocusControler()
    {
      InitializeComponent();
    }

    public void Init()
    {
      line_x.X1 = 0;
      line_x.Y1 = canvas1.ActualHeight / 2;
      line_x.X2 = ActualWidth;
      line_x.Y2 = ActualHeight / 2;
     
    }
  }
}
