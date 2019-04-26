using System;

namespace Compute
{
    internal class ContainerHealthMonitorEventArgs : EventArgs
    {
        public ContainerHealthMonitorEventArgs()
        {
        }

        public ContainerHealthMonitorEventArgs(ushort port, string assemblyFullPath = "", Exception exception = null)
        {
            this.Port = port;
            this.AssemblyFullPath = assemblyFullPath;
            this.Exception = exception;
        }

        public string AssemblyFullPath { get; set; }
        public Exception Exception { get; set; }
        public ushort Port { get; set; }
    }
}