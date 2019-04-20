using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using PackageLibrary;

namespace Compute
{
    internal static class Program
    {
        private static void Main()
        {
            var config = ComputeConfiguration.Instance;
            Debug.WriteLine(config);

            var processManager = ProcessManager.Instance;
            processManager.StartContainerProcesses(config);

            var packageManager = new PackageManager();
            var package = PeriodicallyCheckForValidPackage(config, packageManager);
            Debug.WriteLine(package);

            string sourceDllFullPath = Path.Combine(config.PackageFullFolderPath, package.AssemblyName);
            var ports = processManager.GetAllContainerPorts();
            foreach (ushort port in ports)
            {
                Task.Factory.StartNew((dynamic dobj) =>
                {
                    SendLoadSignalToContainers(dobj.port);
                }, new { port });
            }

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

            processManager.StopAllProcesses();
        }

        private static void SendLoadSignalToContainers(int port)
        {
            // TODO: Form WCF Channel factory and call Load method on proxy
        }

        /// <summary>
        /// Runs until valid package is found
        /// </summary>
        private static PackageReaderResult PeriodicallyCheckForValidPackage(ComputeConfiguration config, PackageManager packageManager)
        {
            string packageConfigPath = Path.Combine(config.PackageFullFolderPath, config.PackageConfigFileName);
            while (true)
            {
                try
                {
                    return packageManager.ReadPackage(
                        packageConfigPath,
                        maxAllowedNumberOfInstances: config.NumberOfContainersToStart);
                }
                catch (ConfigurationException configEx)
                {
                    Console.Error.WriteLine($"ConfigurationException occured while trying to read {packageConfigPath}. Reason: " + configEx.Message);
                    DeletePackage(config, packageManager);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Exception occured while trying to read package. Reason: " + ex.Message);
                }

                Thread.Sleep(config.PackageAcquisitionIntervalMilliseconds);
            }
        }

        private static void DeletePackage(ComputeConfiguration config, PackageManager packageManager)
        {
            Console.WriteLine($"Deleting package located in {config.PackageFullFolderPath}...");
            try
            {
                packageManager.DeletePackage(config.PackageFullFolderPath);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to delete package. Reason: {ex.Message}");
            }
        }
    }
}