using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Compute
{
    internal sealed class ProcessManager
    {
        private static readonly Lazy<ProcessManager> processManager = new Lazy<ProcessManager>(() => new ProcessManager());
        private readonly Dictionary<int, ContainerProcess> ContainerProcessDictById = new Dictionary<int, ContainerProcess>();
        private readonly Dictionary<ushort, ContainerProcess> ContainerProcessDictByPort = new Dictionary<ushort, ContainerProcess>();

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
            return this.ContainerProcessDictByPort.Keys.ToList();
        }

        public IEnumerable<ushort> GetAllFreeContainerPorts()
        {
            return this.ContainerProcessDictByPort
                .Values
                .Where(container => container.IsContainerFree)
                .Select(container => container.Port)
                .ToList();
        }

        public AssemblyInfo GetAssemblyInfo(ushort port)
        {
            var retVal = new AssemblyInfo();
            if (this.ContainerProcessDictByPort.TryGetValue(port, out var containerProcess))
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
            this.StoreContainerProcess(newContainerProcess);
            return port;
        }

        /// <summary>
        /// Starts number of containers as defined in config file
        /// </summary>
        public void StartContainerProcesses(ComputeConfigurationItem config)
        {
            ushort currPort = this.GetNextPort(config.MinPort, config);

            for (int i = 0; i < config.NumberOfContainersToStart; ++i)
            {
                var newContainerProcess = this.StartNewContainerProcess(currPort, config);
                this.StoreContainerProcess(newContainerProcess);
                currPort = this.GetNextPort(currPort, config);
            }
        }

        /// <summary>
        /// Stops / Closes all of the running processes that have been started by ProcessManager
        /// </summary>
        public void StopAllProcesses()
        {
            foreach (var containerProcess in this.ContainerProcessDictById.Values)
            {
                this.SafelyCloseProcess(containerProcess.Process);
            }
            this.ContainerProcessDictById.Clear();
            this.ContainerProcessDictByPort.Clear();
        }

        /// <summary>
        /// Attempts to take container with given port. Returns true upon success, false upon failure
        /// </summary>
        public bool TakeContainer(ushort port, string assemblyFullPath = null)
        {
            if (this.ContainerProcessDictByPort.TryGetValue(port, out var containerProcess) && containerProcess.IsContainerFree)
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
            return !this.ContainerProcessDictByPort.ContainsKey(port);
        }

        private void OnContainerFaulted(object sender, ContainerHealthMonitorEventArgs e)
        {
            if (this.ContainerProcessDictByPort.TryGetValue(e.Port, out var containerProcess))
            {
                this.ContainerProcessDictByPort.Remove(containerProcess.Port);
                this.ContainerProcessDictById.Remove(containerProcess.Process.Id);
            }
        }

        private void SafelyCloseProcess(Process processToClose)
        {
            if (this.ContainerProcessDictById.TryGetValue(processToClose.Id, out var containerProcess))
            {
                this.ContainerProcessDictByPort.Remove(containerProcess.Port);
            }

            if (!processToClose.HasExited && !processToClose.CloseMainWindow())
            {
                processToClose.Kill();
            }
            else
            {
                processToClose.Close();
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
            this.ContainerProcessDictByPort[port] = new ContainerProcess(newProcess, port);
            return this.ContainerProcessDictByPort[port];
        }

        private void StoreContainerProcess(ContainerProcess containerProcess)
        {
            if (this.ContainerProcessDictById.TryGetValue(containerProcess.Process.Id, out var containerToClose))
            {
                this.SafelyCloseProcess(containerToClose.Process);
            }
            this.ContainerProcessDictById[containerProcess.Process.Id] = containerProcess;
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