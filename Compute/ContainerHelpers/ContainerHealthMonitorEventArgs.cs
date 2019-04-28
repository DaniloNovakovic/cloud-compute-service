using System;

namespace Compute
{
    internal class ContainerHealthMonitorEventArgs : EventArgs
    {
        public ContainerHealthMonitorEventArgs()
        {
        }

        public ContainerHealthMonitorEventArgs(RoleInstance roleInstance, Exception exception = null)
        {
            this.RoleInstance = roleInstance ?? throw new ArgumentNullException(nameof(roleInstance));
            this.Exception = exception;
        }

        public ContainerHealthMonitorEventArgs(ushort port, string assemblyFullPath = "", Exception exception = null)
        {
            this.RoleInstance = new RoleInstance
            {
                Port = port,
                AssemblyFullPath = assemblyFullPath
            };
            this.Exception = exception;
        }

        public RoleInstance RoleInstance { get; set; }
        public string AssemblyFullPath => RoleInstance?.AssemblyFullPath ?? string.Empty;
        public Exception Exception { get; set; }
        public ushort Port => RoleInstance.Port;
    }
}