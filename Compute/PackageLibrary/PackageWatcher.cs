using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Compute
{
    internal class PackageWatcher
    {
        public void Start(ComputeConfigurationItem configItem)
        {
            var package = this.PeriodicallyCheckForValidPackageAsync(configItem).GetAwaiter().GetResult();
            OnValidPackageFound(configItem, package);
        }

        private static void OnValidPackageFound(ComputeConfigurationItem configItem, PackageReaderResult package)
        {
            var ports = ProcessManager.Instance.GetAllContainerPorts().Take(package.NumberOfInstances ?? 0).ToList();
            var destinationAssemblies = CopyAssemblies(configItem, package, ports);
            ContainerController.SendLoadSignalToContainersAsync(destinationAssemblies).GetAwaiter().GetResult();
        }

        private static IEnumerable<PackageAssemblyInfo> CopyAssemblies(ComputeConfigurationItem configItem, PackageReaderResult package, List<ushort> ports)
        {
            string sourceDllFullPath = Path.Combine(configItem.PackageFullFolderPath, package.AssemblyName);
            return new PackageController().CopyAssemblies(sourceDllFullPath, configItem.PackageFullFolderPath, ports);
        }

        /// <summary>
        /// Runs until valid package is found
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<PackageReaderResult> PeriodicallyCheckForValidPackageAsync(ComputeConfigurationItem configItem)
        {
            var packageController = new PackageController();
            string packageConfigPath = Path.Combine(configItem.PackageFullFolderPath, configItem.PackageConfigFileName);
            while (true)
            {
                try
                {
                    return packageController.ReadPackage(packageConfigPath, maxAllowedNumberOfInstances: configItem.NumberOfContainersToStart);
                }
                catch (ConfigurationException configEx)
                {
                    Console.Error.WriteLine($"ConfigurationException occured while trying to read {packageConfigPath}. Reason: " + configEx.Message);
                    if (packageController.DeletePackage(configItem.PackageFullFolderPath))
                    {
                        Console.WriteLine($"Successfully deleted {configItem.PackageFullFolderPath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Exception occured while trying to read package. Reason: " + ex.Message);
                }

                await Task.Delay(configItem.PackageAcquisitionIntervalMilliseconds).ConfigureAwait(false);
            }
        }
    }
}