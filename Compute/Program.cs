using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Common;

namespace Compute
{
    internal static class Program
    {
        private static readonly ContainerHealthMonitor containerHealthMonitor = ContainerHealthMonitor.SingletonInstance;
        private static readonly PackageWatcher packageWatcher = new PackageWatcher();
        private static readonly ProcessManager processManager = ProcessManager.SingletonInstance;
        private static readonly WCFServer roleEnvironmentHost = new WCFServer(typeof(RoleEnvironment));
        private static readonly WCFServer computeManagementHost = new WCFServer(typeof(ComputeManagement));

        private static void Main()
        {
            var configItem = Start();

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
            Console.WriteLine("Closing resources, please wait...");

            Stop(configItem);
        }

        private static ComputeConfigurationItem Start()
        {
            var configItem = ComputeConfiguration.Instance.ConfigurationItem;
            processManager.StartContainerProcesses(configItem);

            containerHealthMonitor.ContainerFaulted += ContainerFaultHandler.OnContainerHealthFaulted;
            containerHealthMonitor.Start();

            roleEnvironmentHost.Open();
            computeManagementHost.Open();

            packageWatcher.ValidPackageFound += PackageFoundHandler.OnValidPackageFound;
            packageWatcher.Start();
            return configItem;
        }

        private static void Stop(ComputeConfigurationItem configItem)
        {
            var closeServerTask = Task.Run(() => roleEnvironmentHost.Close());
            var closeComputeManagementTask = Task.Run(() => computeManagementHost.Close());
            packageWatcher.Stop();
            containerHealthMonitor.Stop();

            processManager.StopAllProcesses();

            new PackageController().DeletePackage(configItem.PackageTempFullFolderPath);

            closeServerTask.GetAwaiter().GetResult();
            closeComputeManagementTask.GetAwaiter().GetResult();
        }
    }
}