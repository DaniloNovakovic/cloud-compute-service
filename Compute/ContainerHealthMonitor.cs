using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using Common;

namespace Compute
{
    internal sealed class ContainerHealthMonitor
    {
        private readonly Lazy<ContainerHealthMonitor> monitor = new Lazy<ContainerHealthMonitor>(() => new ContainerHealthMonitor());
        private Thread healthCheckerThread;

        private ContainerHealthMonitor()
        {
        }

        public event EventHandler<ContainerHealthMonitorEventArgs> ContainerFaulted;

        /// <summary>
        /// Returns singleton instance of ContainerHealthMonitor class
        /// </summary>
        public ContainerHealthMonitor Instance => this.monitor.Value;

        /// <summary>
        /// Starts a detached task that will periodically check health for all container processes
        /// </summary>
        /// <exception cref="OutOfMemoryException"></exception>
        public void Run()
        {
            if (this.healthCheckerThread is null)
            {
                this.healthCheckerThread = new Thread(this.PeriodicallyCheckHealth);
                this.healthCheckerThread.Start();
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

        private void OnContainerFaulted(ushort port)
        {
            ContainerFaulted?.Invoke(this, new ContainerHealthMonitorEventArgs(port));
        }

        private void PeriodicallyCheckHealth()
        {
            var processManager = ProcessManager.Instance;
            while (true)
            {
                foreach (ushort port in processManager.GetAllContainerPorts())
                {
                    try
                    {
                        this.CheckHealth(port);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(port + ": " + ex.Message);
                        this.OnContainerFaulted(port);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        internal class ContainerHealthMonitorEventArgs : EventArgs
        {
            public ContainerHealthMonitorEventArgs()
            {
            }

            public ContainerHealthMonitorEventArgs(ushort port)
            {
                this.Port = port;
            }

            public ushort Port { get; set; }
        }
    }
}