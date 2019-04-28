using System;

namespace Compute
{
    public class RoleInstance : ICloneable
    {
        public string AssemblyFullPath { get; set; }
        public string Id => $"{this.Port}.{this.AssemblyFullPath}";
        public ushort Port { get; set; }
        public Role Role { get; set; }
        public string RoleName { get; set; }

        public object Clone()
        {
            return new RoleInstance()
            {
                Port = Port,
                AssemblyFullPath = AssemblyFullPath
            };
        }
    }
}