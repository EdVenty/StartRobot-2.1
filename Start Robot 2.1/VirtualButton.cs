using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_Robot_2._1
{
    internal class VirtualButton
    {
        public VirtualChannel Channel;
        public VirtualButton(VirtualChannel channel)
        {
            Channel = channel;
        }
        public void UpdateFromArray(double[] array)
        {
            Channel.UpdateFromArray(array);
        }
    }
}
