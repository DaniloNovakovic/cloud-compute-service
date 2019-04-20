using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Compute
{
    public sealed class ProcessManager
    {
        private static readonly Lazy<ProcessManager> processManager = new Lazy<ProcessManager>(() => new ProcessManager());
        private readonly Dictionary<int, ContainerProcess> ContainerProcessDict = new Dictionary<int, ContainerProcess>();
        private readonly Dictionary<ushort, bool> processManagerPortsAvailability = new Dictionary<ushort, bool>();

        private ProcessManager()
        {
        }

        public static ProcessManager Instance => processManager.Value;

        /// <summary>
        /// Starts number of containers as defined in config file
        /// </summary>
        public void StartContainerProcesses(ComputeConfiguration config)
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
            foreach (var containerProcess in this.ContainerProcessDict.Values)
            {
                this.SafelyCloseProcess(containerProcess.Process);
            }
            this.ContainerProcessDict.Clear();
            this.processManagerPortsAvailability.Clear();
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

        private ushort GetNextPort(ushort prevPort, ComputeConfiguration config)
        {
            ushort? currPort = this.FindAvailablePortInClosedInterval((ushort)(prevPort + 1), config.MaxPort);
            return currPort
                   ?? this.FindAvailablePortInClosedInterval(config.MinPort, (ushort)(prevPort - 1))
                   ?? throw new Exception($"Could not find available ports in range [{config.MinPort}, {config.MaxPort}]");
        }

        private bool IsPortAvailable(ushort port)
        {
            return this.processManagerPortsAvailability.TryGetValue(port, out bool value)
                ? value
                : (this.processManagerPortsAvailability[port] = true);
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            if (sender is Process process)
            {
                var containerProcess = this.ContainerProcessDict[process.Id];
                this.processManagerPortsAvailability[containerProcess.Port] = true;
                this.ContainerProcessDict.Remove(process.Id);
            }
        }

        private void SafelyCloseProcess(Process processToClose)
        {
            if (!processToClose.CloseMainWindow())
            {
                processToClose.Kill();
            }
            else
            {
                processToClose.Close();
            }
        }

        private ContainerProcess StartNewContainerProcess(ushort port, ComputeConfiguration config)
        {
            var newProcess = Process.Start(fileName: config.ContainerFullFilePath, arguments: $"{port}");
            newProcess.EnableRaisingEvents = true;
            newProcess.Exited += this.OnProcessExit;
            this.processManagerPortsAvailability[port] = false;
            return new ContainerProcess(newProcess, port);
        }

        private void StoreContainerProcess(ContainerProcess containerProcess)
        {
            if (this.ContainerProcessDict.TryGetValue(containerProcess.Process.Id, out var containerToClose))
            {
                this.SafelyCloseProcess(containerToClose.Process);
            }
            this.ContainerProcessDict[containerProcess.Process.Id] = containerProcess;
        }

        private class ContainerProcess
        {
            public ContainerProcess(Process process, ushort port)
            {
                this.Process = process ?? throw new ArgumentNullException(nameof(process));
                this.Port = port;
            }

            public ushort Port { get; }
            public Process Process { get; }
        }
    }
}