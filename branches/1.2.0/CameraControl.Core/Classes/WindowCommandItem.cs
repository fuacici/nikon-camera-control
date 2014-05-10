using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace CameraControl.Core.Classes
{
    public class WindowCommandItem
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool HaveKeyAssigned { get; set; }

        public Key Key { get; set; }

        public bool Alt { get; set; }

        public bool Ctrl { get; set; }

    }
}
