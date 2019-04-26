using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Common;

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