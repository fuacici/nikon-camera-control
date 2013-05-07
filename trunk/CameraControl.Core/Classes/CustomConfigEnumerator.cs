using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CameraControl.Devices.Classes;

namespace CameraControl.Core.Classes
{
    public class CustomConfigEnumerator : BaseFieldClass
    {
        private AsyncObservableCollection<CameraPreset> _items;
        public AsyncObservableCollection<CameraPreset> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                NotifyPropertyChanged("Items");
            }
            
        }

        public CustomConfigEnumerator()
        {
            Items=new AsyncObservableCollection<CameraPreset>();
        }
    }
}
