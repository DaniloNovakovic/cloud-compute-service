using System;
using System.Configuration;
using System.IO;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading;

namespace Compute
{
    internal class PackageWatcher
    {
        private readonly ComputeConfigurationItem configItem = ComputeConfiguration.Instance.ConfigurationItem;
        private readonly object mutex = new object();
        private bool fileChangeHandled = false;
        private readonly ManualResetEvent resetEvent = new ManualResetEvent(false);
        private Thread packageWatcherThread;

        public event EventHandler<ValidPackageFoundEventArgs> ValidPackageFound;

        public void Start()
        {
            if (this.packageWatcherThread?.IsAlive != true)
            {
                resetEvent.Reset();

                this.packageWatcherThread = new Thread(this.Run)
                {
                    IsBackground = true
                };
                this.packageWatcherThread.Start();
            }
        }

        public void Stop()
        {
            if (this.packageWatcherThread?.IsAlive == true)
            {
                resetEvent.Set();

                Thread.Sleep(100);

                this.packageWatcherThread.Abort();
            }
        }

        private void Run()
        {
            try
            {
                var package = this.AttemptToReadValidPackage();
                if (package != null)
                {
                    this.OnValidPackageFound(package);
                }
                this.StartWatchingPackageFolder();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Package watcher error: " + ex.Message);
            }
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

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // sleeping for 500ms because file might still be in use
            //(ex. Notepad++ performs two  writes to file on save)
            Thread.Sleep(500);

            var package = this.AttemptToReadValidPackage();

            if (package != null)
            {
                ProcessManager.Instance.ResetAllProcesses(this.configItem);
                this.OnValidPackageFound(package);
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
            if (this.ShouldIgnoreFileChange(e))
            {
                return;
            }

            this.OnChanged(source, e);

            this.StartFileChangedResetTimeout();
        }

        private bool ShouldIgnoreFileChange(FileSystemEventArgs e)
        {
            string extension = Path.GetExtension(e.FullPath);

            if (!Regex.IsMatch(extension, @"(\.xml)|(\.dll)", RegexOptions.IgnoreCase))
            {
                return true;
            }

            if (this.fileChangeHandled)
            {
                return true;
            }

            lock (this.mutex)
            {
                if (this.fileChangeHandled)
                {
                    return true;
                }
                this.fileChangeHandled = true;
            }
            return false;
        }

        private void StartFileChangedResetTimeout()
        {
            var timer = new System.Timers.Timer(1000) { AutoReset = false };
            timer.Elapsed += (timerElapsedSender, timerElapsedArgs) =>
            {
                lock (this.mutex)
                {
                    this.fileChangeHandled = false;
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

                resetEvent.WaitOne();
            }
        }
    }
}