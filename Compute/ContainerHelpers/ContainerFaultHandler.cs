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

            RoleEnvironment.SafeRemove(oldRoleInstance);

            Console.WriteLine($"{oldRoleInstance.Port}: Container faulted. Recovering...");

            RecoverFromFailureAsync(oldRoleInstance, ProcessManager.SingletonInstance).GetAwaiter().GetResult();
        }

        private static Task AttempToSendLoadSignalAsync(RoleInstance newRoleInstance, ProcessManager processManager)
        {
            var task = Task.CompletedTask;

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

        private static Task RecoverFromFailureAsync(RoleInstance oldRoleInstance, ProcessManager processManager)
        {
            var sendLoadSignalTask = Task.CompletedTask;

            lock (processManager)
            {
                var freeContainerPorts = processManager.GetAllFreeContainerPorts();
                ushort newPort;

                if (freeContainerPorts.Any())
                {
                    newPort = freeContainerPorts.First();
                    Console.WriteLine($"[{oldRoleInstance.Port}]: Moved to existing container [{newPort}]");
                    processManager.StartContainerProcess(ComputeConfiguration.Instance.ConfigurationItem);
                }
                else
                {
                    newPort = processManager.StartContainerProcess(ComputeConfiguration.Instance.ConfigurationItem);
                    Console.WriteLine($"[{oldRoleInstance.Port}]: Replaced by new container [{newPort}]");
                }

                sendLoadSignalTask = AttempToSendLoadSignalAsync(GetNewRoleInstance(newPort, oldRoleInstance), processManager);
            }

            return sendLoadSignalTask;
        }
    }
}