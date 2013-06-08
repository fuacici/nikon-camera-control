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
using System.Windows.Shapes;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;

namespace CameraControl.windows
{
    /// <summary>
    /// Interaction logic for ScriptWnd.xaml
    /// </summary>
    public partial class ScriptWnd : IWindow, IToolPlugin
    {
        public ScriptWnd()
        {
            InitializeComponent();
            ServiceProvider.Settings.ApplyTheme(this);
        }

        #region Implementation of IWindow

        public void ExecuteCommand(string cmd, object param)
        {
            switch (cmd)
            {
                case WindowsCmdConsts.ScriptWnd_Show:
                    Dispatcher.Invoke(new Action(delegate
                    {
                        Show();
                        Activate();
                        //Topmost = true;
                        //Topmost = false;
                        Focus();
                    }));
                    break;
                case WindowsCmdConsts.ScriptWnd_Hide:
                    Hide();
                    break;
                case CmdConsts.All_Close:
                    Dispatcher.Invoke(new Action(delegate
                    {
                        Hide();
                        Close();
                    }));
                    break;
            }
        }

        #endregion

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsVisible)
            {
                e.Cancel = true;
                ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.ScriptWnd_Hide);
            }
        }

        #region Implementation of IToolPlugin

        public bool Execute()
        {
            ServiceProvider.WindowsManager.ExecuteCommand(WindowsCmdConsts.ScriptWnd_Show);
            return true;
        }

        #endregion
    }
}
