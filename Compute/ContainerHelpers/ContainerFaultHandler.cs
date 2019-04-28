using System;
using System.Linq;
using System.Threading.Tasks;

namespace Compute
{
    internal static class ContainerFaultHandler
    {
        public static void OnContainerHealthFaulted(object sender, ContainerHealthMonitorEventArgs args)
        {
            var oldRoleInstance = args.RoleInstance;
            Console.WriteLine($"{oldRoleInstance.Port}: Container faulted. Recovering...");
            Task sendLoadSignalTask = null;
            var processManager = ProcessManager.SingletonInstance;

            lock (processManager)
            {
                var freeContainerPorts = processManager.GetAllFreeContainerPorts();

                if (freeContainerPorts.Any()) // There is free container
                {
                    ushort newPort = freeContainerPorts.First();
                    Console.WriteLine($"[{oldRoleInstance.Port}]: Moved to existing container [{newPort}]");
                    sendLoadSignalTask = AttempToSendLoadSignalAsync(GetNewRoleInstance(newPort, oldRoleInstance), processManager);
                    processManager.StartContainerProcess(ComputeConfiguration.Instance.ConfigurationItem);
                }
                else // There isn't any free container
                {
                    ushort newPort = processManager.StartContainerProcess(ComputeConfiguration.Instance.ConfigurationItem);
                    Console.WriteLine($"[{oldRoleInstance.Port}]: Replaced by new container [{newPort}]");
                    sendLoadSignalTask = AttempToSendLoadSignalAsync(GetNewRoleInstance(newPort, oldRoleInstance), processManager);
                }
            }

            if (sendLoadSignalTask != null)
            {
                sendLoadSignalTask.GetAwaiter().GetResult();
            }
        }

        private static Task AttempToSendLoadSignalAsync(RoleInstance newRoleInstance, ProcessManager processManager)
        {
            Task task = null;

            if (!string.IsNullOrWhiteSpace(newRoleInstance.AssemblyFullPath))
            {
                Console.WriteLine($"[{newRoleInstance.Port}]: Attempting to send load assembly signal...");
                processManager.TakeContainer(newRoleInstance);
                task = ContainerController.SendLoadSignalToContainerAsync(newRoleInstance, numberOfAttempts: 1);
            }

            return task;
        }

        private static RoleInstance GetNewRoleInstance(ushort newPort, RoleInstance oldRoleInstance)
        {
            return new RoleInstance()
            {
                RoleName = oldRoleInstance.RoleName,
                AssemblyFullPath = oldRoleInstance.AssemblyFullPath,
                Port = newPort
            };
        }
    }
}