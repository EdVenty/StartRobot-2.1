using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_Robot_2._1
{
    public class ControllersEventArgs : EventArgs
    {
        public ObservableCollection<VirtualGamepad> Gamepads { get; set; }
    }
}
