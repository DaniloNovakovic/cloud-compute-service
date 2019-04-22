using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compute
{
    internal static class ContainerFaultHandler
    {
        public static void OnContainerHealthFaulted(object sender, ContainerHealthMonitorEventArgs args)
        {
            Console.WriteLine($"{args.Port}: Container faulted. Recovering...");

            ushort port;
            Task sendLoadSignalTask = null;
            var processManager = ProcessManager.Instance;

            lock (processManager)
            {
                var freeContainerPorts = processManager.GetAllFreeContainerPorts();
                if (freeContainerPorts.Any()) // There is free container
                {
                    port = freeContainerPorts.First();
                    Console.WriteLine($"[{args.Port}]: Moved to existing container [{port}]");
                    sendLoadSignalTask = AttempToSendLoadSignalAsync(port, args.AssemblyFullPath, processManager);
                    processManager.StartContainerProcess(ComputeConfiguration.Instance.ConfigurationItem);
                }
                else // There isn't any free container
                {
                    port = processManager.StartContainerProcess(ComputeConfiguration.Instance.ConfigurationItem);
                    Console.WriteLine($"[{args.Port}]: Replaced by new container [{port}]");
                    sendLoadSignalTask = AttempToSendLoadSignalAsync(port, args.AssemblyFullPath, processManager);
                }
            }

            if (sendLoadSignalTask != null)
            {
                sendLoadSignalTask.GetAwaiter().GetResult();
            }
        }

        private static Task AttempToSendLoadSignalAsync(ushort port, string assemblyFullPath, ProcessManager processManager)
        {
            Task task = null;

            if (!string.IsNullOrWhiteSpace(assemblyFullPath))
            {
                Console.WriteLine($"[{port}]: Attempting to send load assembly signal...");
                processManager.TakeContainer(port, assemblyFullPath);
                task = ContainerController.SendLoadSignalToContainerAsync(port, assemblyFullPath, numberOfAttempts: 1);
            }

            return task;
        }
    }
}