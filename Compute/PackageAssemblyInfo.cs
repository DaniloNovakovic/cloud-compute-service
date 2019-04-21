using System;

namespace Compute
{
    public class PackageAssemblyInfo : ICloneable
    {
        public ushort Port { get; set; }
        public string AssemblyFullPath { get; set; }

        public object Clone()
        {
            return new PackageAssemblyInfo()
            {
                Port = Port,
                AssemblyFullPath = AssemblyFullPath
            };
        }
    }
}