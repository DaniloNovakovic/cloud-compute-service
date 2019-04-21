using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compute
{
    public class AssemblyInfo : ICloneable
    {
        public ushort Port { get; set; }
        public string AssemblyFullPath { get; set; }

        public object Clone()
        {
            return new AssemblyInfo()
            {
                Port = Port,
                AssemblyFullPath = AssemblyFullPath
            };
        }
    }
}