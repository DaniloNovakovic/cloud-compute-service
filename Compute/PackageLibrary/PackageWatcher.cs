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
        private readonly ComputeConfigurationItem configItem = ComputeConfiguration.Instance.ConfigurationItem;

        public event EventHandler<ValidPackageFoundEventArgs> ValidPackageFound;

        public void Start()
        {
            var package = this.AttemptToReadValidPackage();
            if (package != null)
            {
                this.OnValidPackageFound(package);
            }
            this.StartWatchingPackageFolder();
        }

        /// <summary>
        /// Runs until valid package is found
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        private PackageReaderResult AttemptToReadValidPackage()
        {
            string packageConfigPath = Path.Combine(this.configItem.PackageFullFolderPath, this.configItem.PackageConfigFileName);

            try
            {
                return new PackageController().ReadPackage(
                    packageConfigPath,
                    maxAllowedNumberOfInstances: this.configItem.NumberOfContainersToStart);
            }
            catch (ConfigurationException configEx)
            {
                OnPackageConfigurationError(packageConfigPath, configEx);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Exception occured while trying to read package. Reason: " + ex.Message);
            }
            return null;
        }

        private void OnPackageConfigurationError(string packageConfigPath, ConfigurationException configEx)
        {
            Console.Error.WriteLine($"ConfigurationException occured while trying to read {packageConfigPath}. Reason: " + configEx.Message);
            if (new PackageController().DeletePackage(this.configItem.PackageFullFolderPath))
            {
                Console.WriteLine($"Successfully deleted {this.configItem.PackageFullFolderPath}");
            }
        }

        private void OnValidPackageFound(PackageReaderResult package)
        {
            ValidPackageFound?.Invoke(this, new ValidPackageFoundEventArgs(package));
        }

        private void StartWatchingPackageFolder()
        {
        }
    }
}