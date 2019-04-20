using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using PackageLibrary;

namespace Compute
{
    internal static class Program
    {
        private static void Main()
        {
            var config = ComputeConfiguration.Instance;
            Debug.WriteLine(config);

            var manager = new PackageManager();

            var packageResult = manager.ReadPackage(
                Path.Combine(config.PackageFullFolderPath, config.PackageConfigFileName),
                maxAllowedNumberOfInstances: config.NumberOfContainersToStart);

            Debug.WriteLine(packageResult.NumberOfInstances);
            Debug.WriteLine(packageResult.AssemblyName);

            var processManager = ProcessManager.Instance;
            processManager.StartContainerProcesses(config);

            Thread.Sleep(2000);

            processManager.StopAllProcesses();

            Thread.Sleep(2000);

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }
    }
}