using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compute
{
    public class ContainerProcess
    {
        public Process Process { get; }
        public ushort Port { get; }

        public ContainerProcess(Process process, ushort port)
        {
            this.Process = process ?? throw new ArgumentNullException(nameof(process));
            this.Port = port;
        }
    }
}