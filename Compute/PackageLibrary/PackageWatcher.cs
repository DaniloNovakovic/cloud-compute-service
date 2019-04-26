using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compute
{
    internal class PackageWatcher
    {
        /// <summary>
        /// Runs until valid package is found
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<PackageReaderResult> PeriodicallyCheckForValidPackageAsync(ComputeConfigurationItem configItem)
        {
            var packageManager = new PackageController();
            string packageConfigPath = Path.Combine(configItem.PackageFullFolderPath, configItem.PackageConfigFileName);
            while (true)
            {
                try
                {
                    return packageManager.ReadPackage(packageConfigPath, maxAllowedNumberOfInstances: configItem.NumberOfContainersToStart);
                }
                catch (ConfigurationException configEx)
                {
                    Console.Error.WriteLine($"ConfigurationException occured while trying to read {packageConfigPath}. Reason: " + configEx.Message);
                    if (packageManager.DeletePackage(configItem.PackageFullFolderPath))
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