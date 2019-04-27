using System;
using System.ServiceModel;
using Common;

namespace Compute
{
    internal static class Program
    {
        private static readonly ProcessManager processManager = ProcessManager.Instance;
        private static readonly ContainerHealthMonitor containerHealthMonitor = ContainerHealthMonitor.Instance;
        private static readonly PackageWatcher packageWatcher = new PackageWatcher();
        private static ServiceHost host;

        private static void Main()
        {
            var configItem = ComputeConfiguration.Instance.ConfigurationItem;
            processManager.StartContainerProcesses(configItem);

            containerHealthMonitor.ContainerFaulted += ContainerFaultHandler.OnContainerHealthFaulted;
            containerHealthMonitor.Start();

            packageWatcher.ValidPackageFound += PackageFoundHandler.OnValidPackageFound;
            packageWatcher.Start();

            host = new ServiceHost(typeof(RoleEnvironmentService));
            host.Open();
            Console.WriteLine("Server started...");

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

            host.Close();

            processManager.StopAllProcesses();
            containerHealthMonitor.Stop();
            packageWatcher.Stop();

            new PackageController().DeletePackage(configItem.PackageTempFullFolderPath);
        }
    }
}