using System;

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