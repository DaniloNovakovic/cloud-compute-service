using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace Compute
{
    public class ContainerHealthMonitorEventArgs : EventArgs
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

    internal sealed class ContainerHealthMonitor
    {
        private static readonly Lazy<ContainerHealthMonitor> monitor = new Lazy<ContainerHealthMonitor>(() => new ContainerHealthMonitor());
        private Thread healthCheckerThread;

        private ContainerHealthMonitor()
        {
        }

        public event EventHandler<ContainerHealthMonitorEventArgs> ContainerFaulted;

        /// <summary>
        /// Returns singleton instance of ContainerHealthMonitor class
        /// </summary>
        public static ContainerHealthMonitor Instance => monitor.Value;

        /// <summary>
        /// Starts a detached task that will periodically check health for all container processes
        /// </summary>
        /// <exception cref="OutOfMemoryException"></exception>
        public void Start()
        {
            if (this.healthCheckerThread is null)
            {
                this.healthCheckerThread = new Thread(this.PeriodicallyCheckHealth);
                this.healthCheckerThread.IsBackground = true;
                this.healthCheckerThread.Start();
            }
        }

        public void Stop()
        {
            if (this.healthCheckerThread?.IsAlive ?? false)
            {
                this.healthCheckerThread.Abort();
            }
        }

        private void CheckHealth(ushort port)
        {
            string remoteAddress = $"net.tcp://localhost:{port}/{typeof(IContainerManagement).Name}";
            var channelFactory = new ChannelFactory<IContainerManagement>(new NetTcpBinding(), remoteAddress);
            var proxy = channelFactory.CreateChannel();

            string result = proxy.CheckHealth();
            Trace.TraceInformation($"{port}: {DateTime.Now}: {result}");

            channelFactory.Close();
        }

        private void OnContainerFaulted(ushort port, Exception ex)
        {
            var assemblyInfo = ProcessManager.Instance.GetAssemblyInfo(port);
            ContainerFaulted?.Invoke(this, new ContainerHealthMonitorEventArgs(port, assemblyInfo.AssemblyFullPath, ex));
        }

        private void PeriodicallyCheckHealth()
        {
            var processManager = ProcessManager.Instance;
            while (true)
            {
                var taskList = new List<Task>();
                foreach (ushort port in processManager.GetAllContainerPorts())
                {
                    taskList.Add(Task.Factory.StartNew((dynamic dobj) =>
                    {
                        try
                        {
                            this.CheckHealth(dobj.port);
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError($"{dobj.port} : {ex.Message}");
                            this.OnContainerFaulted(dobj.port, ex);
                        }
                    }, new { port }));
                }
                Thread.Sleep(1000);
                Task.WhenAll(taskList).GetAwaiter().GetResult();
            }
        }
    }
}