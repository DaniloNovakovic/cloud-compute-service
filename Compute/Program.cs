using System;
using System.ServiceModel;
using Common;

namespace Compute
{
    internal static class Program
    {
        private static readonly ContainerHealthMonitor containerHealthMonitor = ContainerHealthMonitor.SingletonInstance;
        private static readonly PackageWatcher packageWatcher = new PackageWatcher();
        private static readonly ProcessManager processManager = ProcessManager.SingletonInstance;
        private static readonly WCFServer roleEnvironmentHost = new WCFServer(typeof(RoleEnvironmentService));

        private static void Main()
        {
            var configItem = Start();

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

            Stop(configItem);
        }

        private static ComputeConfigurationItem Start()
        {
            var configItem = ComputeConfiguration.Instance.ConfigurationItem;
            processManager.StartContainerProcesses(configItem);

            containerHealthMonitor.ContainerFaulted += ContainerFaultHandler.OnContainerHealthFaulted;
            containerHealthMonitor.Start();

            roleEnvironmentHost.Open();

            packageWatcher.ValidPackageFound += PackageFoundHandler.OnValidPackageFound;
            packageWatcher.Start();
            return configItem;
        }

        private static void Stop(ComputeConfigurationItem configItem)
        {
            packageWatcher.Stop();
            roleEnvironmentHost.Close();
            containerHealthMonitor.Stop();

            processManager.StopAllProcesses();

            new PackageController().DeletePackage(configItem.PackageTempFullFolderPath);
        }
    }
}