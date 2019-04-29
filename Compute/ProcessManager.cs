using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Compute
{
    internal sealed class ProcessManager
    {
        private static readonly Lazy<ProcessManager> processManager = new Lazy<ProcessManager>(() => new ProcessManager());
        private readonly ConcurrentDictionary<ushort, ContainerProcess> containerProcessDictByPort = new ConcurrentDictionary<ushort, ContainerProcess>();

        private ProcessManager()
        {
            ContainerHealthMonitor.SingletonInstance.ContainerFaulted += this.OnContainerFaulted;
        }

        /// <summary>
        /// Returns singleton instance of ProcessManager class
        /// </summary>
        public static ProcessManager SingletonInstance => processManager.Value;

        public IEnumerable<ushort> GetAllContainerPorts()
        {
            return this.containerProcessDictByPort.Keys.ToArray();
        }

        public IEnumerable<ushort> GetAllFreeContainerPorts()
        {
            return this.containerProcessDictByPort
                .Values
                .Where(container => container.IsContainerFree)
                .Select(container => container.Port)
                .ToArray();
        }

        public RoleInstance GetRoleInstance(ushort port)
        {
            return this.containerProcessDictByPort.TryGetValue(port, out var containerProcess) ? containerProcess.RoleInstance : null;
        }

        public void ResetAllProcesses(ComputeConfigurationItem config)
        {
            this.StopAllProcesses();
            this.StartContainerProcesses(config);
        }

        public ushort StartContainerProcess(ComputeConfigurationItem config)
        {
            ushort port = this.GetNextPort(config.MinPort, config);
            var newContainerProcess = this.StartNewContainerProcess(port, config);
            return newContainerProcess.Port;
        }

        /// <summary>
        /// Starts number of containers as defined in config file
        /// </summary>
        public void StartContainerProcesses(ComputeConfigurationItem config)
        {
            ushort currPort = this.GetNextPort(prevPort: config.MinPort, config);

            for (int i = 0; i < config.NumberOfContainersToStart; ++i)
            {
                var newProcess = this.StartNewContainerProcess(currPort, config);
                currPort = this.GetNextPort(prevPort: newProcess.Port, config);
            }
        }

        /// <summary>
        /// Stops / Closes all of the running processes that have been started by ProcessManager
        /// </summary>
        public void StopAllProcesses()
        {
            lock (this.containerProcessDictByPort)
            {
                foreach (var containerProcess in this.containerProcessDictByPort.Values)
                {
                    this.SafelyCloseProcess(containerProcess);
                }
                this.containerProcessDictByPort.Clear();
            }
        }

        /// <summary>
        /// Attempts to take container with port specified in roleInstance. Returns true upon
        /// success, false upon failure
        /// </summary>
        public bool TakeContainer(RoleInstance roleInstance)
        {
            if (string.IsNullOrWhiteSpace(roleInstance.RoleName))
                return false;

            if (this.containerProcessDictByPort.TryGetValue(roleInstance.Port, out var containerProcess) && containerProcess.IsContainerFree)
            {
                RoleEnvironment.SafeAddOrUpdate(roleInstance);
                containerProcess.RoleInstance = roleInstance;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to free container on given port
        /// </summary>
        /// <param name="port">Port on which the container is running</param>
        public bool FreeContainer(ushort port)
        {
            if (this.containerProcessDictByPort.TryGetValue(port, out var containerProcess)
                && !containerProcess.IsContainerFree)
            {
                SafelyCloseProcess(containerProcess);
                StartNewContainerProcess(port, ComputeConfiguration.Instance.ConfigurationItem);
                return true;
            }
            return false;
        }

        private ushort? FindAvailablePortInClosedInterval(ushort minValue, ushort maxValue)
        {
            for (ushort currPort = minValue; currPort <= maxValue; ++currPort)
            {
                if (this.IsPortAvailable(currPort))
                {
                    return currPort;
                }
            }
            return null;
        }

        private ushort GetNextPort(ushort prevPort, ComputeConfigurationItem config)
        {
            ushort? currPort = this.FindAvailablePortInClosedInterval((ushort)(prevPort + 1), config.MaxPort);
            return currPort
                   ?? this.FindAvailablePortInClosedInterval(config.MinPort, (ushort)(prevPort - 1))
                   ?? throw new Exception($"Could not find available ports in range [{config.MinPort}, {config.MaxPort}]");
        }

        private bool IsPortAvailable(ushort port)
        {
            return !this.containerProcessDictByPort.ContainsKey(port);
        }

        private void OnContainerFaulted(object sender, ContainerHealthMonitorEventArgs e)
        {
            if (this.containerProcessDictByPort.TryRemove(e.Port, out var removedContainer))
            {
                Debug.WriteLine($"{typeof(ProcessManager).Name}: removed container[{removedContainer.Port}] ({DateTime.Now})");
            }
        }

        private void SafelyCloseProcess(ContainerProcess containerProcess)
        {
            var processToClose = containerProcess.Process;

            if (!processToClose.HasExited && !processToClose.CloseMainWindow())
            {
                processToClose.Kill();
            }
            else
            {
                processToClose.Close();
            }

            if (this.containerProcessDictByPort.TryRemove(containerProcess.Port, out var container))
            {
                Debug.WriteLine($"{typeof(ProcessManager).Name}: removed container[{container.Port}] ({DateTime.Now})");

                if (container.RoleInstance != null)
                {
                    RoleEnvironment.SafeRemove(container.RoleInstance);
                }
            }
        }

        /// <summary>
        /// Starts up new container process with given port as it's argument
        /// </summary>
        /// <returns>New container process</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="System.ComponentModel.Win32Exception"></exception>
        private ContainerProcess StartNewContainerProcess(ushort port, ComputeConfigurationItem config)
        {
            if (!this.IsPortAvailable(port))
            {
                throw new InvalidOperationException($"port={port} is already taken by another process!");
            }

            var newProcess = Process.Start(fileName: config.ContainerFullFilePath, arguments: $"{port}");
            this.containerProcessDictByPort[port] = new ContainerProcess(newProcess, port);

            Debug.WriteLine($"{typeof(ProcessManager).Name}: added container[{port}] ({DateTime.Now})");

            return this.containerProcessDictByPort[port];
        }

        private class ContainerProcess
        {
            public ContainerProcess(Process process, ushort port)
            {
                this.Process = process ?? throw new ArgumentNullException(nameof(process));
                this.Port = port;
            }

            public bool IsContainerFree => this.RoleInstance is null;
            public ushort Port { get; }
            public Process Process { get; }
            public RoleInstance RoleInstance { get; set; }
        }
    }
}