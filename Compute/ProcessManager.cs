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
            ContainerHealthMonitor.Instance.ContainerFaulted += this.OnContainerFaulted;
        }

        /// <summary>
        /// Returns singleton instance of ProcessManager class
        /// </summary>
        public static ProcessManager Instance => processManager.Value;

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

        public PackageAssemblyInfo GetAssemblyInfo(ushort port)
        {
            var retVal = new PackageAssemblyInfo();
            if (this.containerProcessDictByPort.TryGetValue(port, out var containerProcess))
            {
                retVal.Port = containerProcess.Port;
                retVal.AssemblyFullPath = containerProcess.AssemblyFullPath;
            }
            return retVal;
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
        /// Attempts to take container with given port. Returns true upon success, false upon failure
        /// </summary>
        public bool TakeContainer(ushort port, string assemblyFullPath = null)
        {
            if (this.containerProcessDictByPort.TryGetValue(port, out var containerProcess) && containerProcess.IsContainerFree)
            {
                containerProcess.IsContainerFree = false;
                containerProcess.AssemblyFullPath = assemblyFullPath;
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
                Debug.WriteLine($"{typeof(ProcessManager).Name}: Removed {removedContainer.Port} ({DateTime.Now})");
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
                Debug.WriteLine($"{typeof(ProcessManager).Name}: removed {container} ({DateTime.Now})");
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
            return this.containerProcessDictByPort[port];
        }

        private class ContainerProcess
        {
            public ContainerProcess(Process process, ushort port)
            {
                this.Process = process ?? throw new ArgumentNullException(nameof(process));
                this.Port = port;
            }

            public string AssemblyFullPath { get; set; } = string.Empty;
            public bool IsContainerFree { get; set; } = true;
            public ushort Port { get; }
            public Process Process { get; }
        }
    }
}