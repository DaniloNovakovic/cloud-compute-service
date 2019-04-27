namespace Compute
{
    internal class RoleInstance
    {
        public ushort ContainerId { get; set; }
        public string AssemblyFullPath { get; set; }
        public Role Role { get; set; }
    }
}