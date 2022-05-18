using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_Robot_2._1
{
    internal class VirtualTrigger
    {
        public VirtualChannel Channel;
        public VirtualTrigger(VirtualChannel channel)
        {
            Channel = channel;
        }
        public void UpdateFromArray(double[] axisArray)
        {
            Channel.UpdateFromArray(axisArray);
        }
    }
}
