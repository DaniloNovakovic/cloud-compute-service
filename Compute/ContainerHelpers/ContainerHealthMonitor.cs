using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace Compute
{
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
        public static ContainerHealthMonitor SingletonInstance => monitor.Value;

        /// <summary>
        /// Starts a detached task that will periodically check health for all container processes
        /// </summary>
        /// <exception cref="OutOfMemoryException"></exception>
        public void Start()
        {
            if (this.healthCheckerThread is null)
            {
                this.healthCheckerThread = new Thread(this.PeriodicallyCheckHealth)
                {
                    IsBackground = true
                };
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
            var roleInstance = ProcessManager.SingletonInstance.GetRoleInstance(port);
            ContainerFaulted?.Invoke(this, new ContainerHealthMonitorEventArgs(port, roleInstance?.AssemblyFullPath ?? "", ex));
        }

        private void PeriodicallyCheckHealth()
        {
            var processManager = ProcessManager.SingletonInstance;
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