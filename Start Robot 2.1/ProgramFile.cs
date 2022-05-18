using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Start_Robot_2._1
{
    public class ProgramFile
    {
        public string FileName { get; set; }
        public bool IsRunning { get; set; }
        public Dictionary<FilePropertyType, object> FileProperties { get; set; } = new Dictionary<FilePropertyType, object>();
        public bool RunOnBoot
        {
            get
            {
                var ret = false;
                try
                {
                    ret = FileProperties.TryGetValue(FilePropertyType.RunOnBoot, out object value) ? Convert.ToBoolean(value.ToString()) : false;
                }
                catch(Exception err)
                {
                    Debug.WriteLine(err.Message);
                }
                return ret;
            }
            set
            {
                FileProperties[FilePropertyType.RunOnBoot] = value;
            }
        }
    }
}
