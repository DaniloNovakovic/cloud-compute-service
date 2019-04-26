using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Compute
{
    internal static class Program
    {
        private static readonly PackageController packageManager = new PackageController();
        private static readonly ProcessManager processManager = ProcessManager.Instance;

        private static void Main()
        {
            var configItem = LoadComputeConfiguration();
            processManager.StartContainerProcesses(configItem);

            var containerHealthMonitor = ContainerHealthMonitor.Instance;
            containerHealthMonitor.ContainerFaulted += ContainerFaultHandler.OnContainerHealthFaulted;
            containerHealthMonitor.Start();
            var packageWatcher = new PackageWatcher();

            try
            {
                var validPackage = packageWatcher.PeriodicallyCheckForValidPackageAsync(configItem).GetAwaiter().GetResult();
                var destinationAssemblies = CopyAssemblies(configItem, validPackage);
                SendLoadAssemblySignalToContainers(destinationAssemblies);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

            processManager.StopAllProcesses();
            containerHealthMonitor.Stop();
        }

        private static IEnumerable<PackageAssemblyInfo> CopyAssemblies(ComputeConfigurationItem configItem, PackageReaderResult package)
        {
            Console.WriteLine($"Copying assemblies to n={package.NumberOfInstances} destinations...");

            var ports = ProcessManager.Instance.GetAllContainerPorts().Take(package.NumberOfInstances ?? 0).ToList();
            string sourceDllFullPath = Path.Combine(configItem.PackageFullFolderPath, package.AssemblyName);
            return packageManager.CopyAssemblies(sourceDllFullPath, configItem.PackageFullFolderPath, ports);
        }

        private static ComputeConfigurationItem LoadComputeConfiguration()
        {
            Console.WriteLine("Loading Compute configuration...");

            return ComputeConfiguration.Instance.ConfigurationItem;
        }

        private static void SendLoadAssemblySignalToContainers(IEnumerable<PackageAssemblyInfo> destinationPaths)
        {
            Console.WriteLine("Sending load assembly signal to requested number of container processes...");

            ContainerController.SendLoadSignalToContainersAsync(destinationPaths).GetAwaiter().GetResult();

            Console.WriteLine("All of the processes have finished loading assemblies");
        }
    }
}