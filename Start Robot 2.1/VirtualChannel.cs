using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_Robot_2._1
{
   public class VirtualChannel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public VirtualChannelTag Tag { get; set; }
        public void UpdateFromArray(double[] array)
        {
            Value = array[Id];
        }
    }
}
