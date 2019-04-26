using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Timers;

namespace Compute
{
    internal class PackageWatcher
    {
        private readonly Dictionary<string, bool> changedFiles = new Dictionary<string, bool>();
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
        /// Attempts to read valid package (returns null if valid package is not found)
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
                this.OnPackageConfigurationError(packageConfigPath, configEx);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Exception occured while trying to read package. Reason: " + ex.Message);
            }
            return null;
        }

        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            string extension = Path.GetExtension(e.FullPath);

            if (Regex.IsMatch(extension, @"\.xml", RegexOptions.IgnoreCase))
            {
                Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
                var package = this.AttemptToReadValidPackage();
                if (package != null)
                {
                    this.OnValidPackageFound(package);
                }
            }
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

        // protection against bug with file watcher and notepad where change event occurs twice
        private void SafeOnChanged(object source, FileSystemEventArgs e)
        {
            lock (this.changedFiles)
            {
                if (this.changedFiles.ContainsKey(e.FullPath))
                {
                    return;
                }
                this.changedFiles[e.FullPath] = true;
            }

            OnChanged(source, e);

            var timer = new Timer(1000) { AutoReset = false };
            timer.Elapsed += (timerElapsedSender, timerElapsedArgs) =>
            {
                lock (this.changedFiles)
                {
                    this.changedFiles.Remove(e.FullPath);
                }
            };
            timer.Start();
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void StartWatchingPackageFolder()
        {
            using (var watcher = new FileSystemWatcher())
            {
                watcher.Path = this.configItem.PackageFullFolderPath;

                watcher.NotifyFilter = NotifyFilters.LastWrite;
                watcher.IncludeSubdirectories = false;

                watcher.Filter = "*.*";

                watcher.Changed += this.SafeOnChanged;
                watcher.Created += this.SafeOnChanged;

                watcher.EnableRaisingEvents = true;

                Console.WriteLine("Press ENTER to exit...");
                Console.ReadLine();
            }
        }
    }
}