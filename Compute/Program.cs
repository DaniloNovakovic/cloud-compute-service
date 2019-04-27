using System;

namespace Compute
{
    internal static class Program
    {
        private static readonly ProcessManager processManager = ProcessManager.Instance;
        private static readonly ContainerHealthMonitor containerHealthMonitor = ContainerHealthMonitor.Instance;
        private static readonly PackageWatcher packageWatcher = new PackageWatcher();

        private static void Main()
        {
            var configItem = ComputeConfiguration.Instance.ConfigurationItem;
            processManager.StartContainerProcesses(configItem);

            containerHealthMonitor.ContainerFaulted += ContainerFaultHandler.OnContainerHealthFaulted;
            containerHealthMonitor.Start();

            packageWatcher.ValidPackageFound += PackageFoundHandler.OnValidPackageFound;
            packageWatcher.Start();

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

            processManager.StopAllProcesses();
            containerHealthMonitor.Stop();
            packageWatcher.Stop();

            new PackageController().DeletePackage(configItem.PackageTempFullFolderPath);
        }
    }
}