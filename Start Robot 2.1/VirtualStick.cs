using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_Robot_2._1
{
    internal class VirtualStick
    {
        public VirtualChannel XChannel;
        public VirtualChannel YChannel;
        public VirtualStick(VirtualChannel xChannel, VirtualChannel yChannel)
        {
            XChannel = xChannel;
            YChannel = yChannel;
        }
        public void UpdateFromArray(double[] axisArray)
        {
            XChannel.UpdateFromArray(axisArray);
            YChannel.UpdateFromArray(axisArray);
        }
    }
}
