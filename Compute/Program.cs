using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Compute
{
    internal static class Program
    {
        private static readonly PackageManager packageManager = new PackageManager();

        private static void Main()
        {
            var configItem = LoadComputeConfiguration();
            var processManager = StartUpContainerProcesses(configItem);

            try
            {
                var package = StartPeriodicCheckUntilFirstValidPackageIsFound(configItem);
                var destinationPaths = CopyAssemblies(configItem, processManager, package);
                SendLoadAssemblySignalToContainers(destinationPaths);

                Console.WriteLine("All of the processes have finished loading assemblies");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

            processManager.StopAllProcesses();
        }

        private static void SendLoadAssemblySignalToContainers(List<AssemblyInfo> destinationPaths)
        {
            Console.WriteLine("Sending load assembly signal to requested number of container processes...");

            ContainerController.SendLoadSignalToContainers(destinationPaths).GetAwaiter().GetResult();
        }

        private static List<AssemblyInfo> CopyAssemblies(ComputeConfigurationItem configItem, ProcessManager processManager, PackageReaderResult package)
        {
            Console.WriteLine($"Copying assemblies to n={package.NumberOfInstances} destinations...");

            var ports = processManager.GetAllContainerPorts().Take(package.NumberOfInstances ?? 0).ToList();
            string sourceDllFullPath = Path.Combine(configItem.PackageFullFolderPath, package.AssemblyName);
            return packageManager.CopyAssemblies(sourceDllFullPath, configItem.PackageFullFolderPath, ports);
        }

        private static ComputeConfigurationItem LoadComputeConfiguration()
        {
            Console.WriteLine("Loading Compute configuration...");

            var configItem = ComputeConfiguration.Instance.ConfigurationItem;
            Debug.WriteLine(configItem);
            return configItem;
        }

        private static ProcessManager StartUpContainerProcesses(ComputeConfigurationItem configItem)
        {
            Console.WriteLine("Starting up container processes...");

            var processManager = ProcessManager.Instance;
            processManager.StartContainerProcesses(configItem);
            return processManager;
        }

        private static PackageReaderResult StartPeriodicCheckUntilFirstValidPackageIsFound(ComputeConfigurationItem configItem)
        {
            Console.WriteLine("Starting periodic check until first valid package is found...");

            var package = packageManager.PeriodicallyCheckForValidPackage(configItem);
            Debug.WriteLine(package);
            return package;
        }
    }
}