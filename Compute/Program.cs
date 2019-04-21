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
        private static readonly PackageManager packageManager = new PackageManager();
        private static readonly ProcessManager processManager = ProcessManager.Instance;

        private static void Main()
        {
            var configItem = LoadComputeConfiguration();
            processManager.StartContainerProcesses(configItem);

            var containerHealthMonitor = ContainerHealthMonitor.Instance;
            containerHealthMonitor.ContainerFaulted += OnContainerHealthFaulted;
            containerHealthMonitor.Run();

            try
            {
                var validPackage = StartPeriodicCheckUntilFirstValidPackageIsFound(configItem);
                var destinationAssemblies = CopyAssemblies(configItem, validPackage);
                SendLoadAssemblySignalToContainers(destinationAssemblies);
                //StartPeriodHealthChecksInTheBackground(destinationAssemblies);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

            processManager.StopAllProcesses();
        }

        private static void OnContainerHealthFaulted(object sender, ContainerHealthMonitorEventArgs e)
        {
            Console.WriteLine($"{e.Port}: Problem occured!");
        }

        private static void OnContainerFailure(AssemblyInfo assembly, Exception exception)
        {
            Console.Error.WriteLine($"{assembly.Port}: Container failed/has closed... Exception msg: " + exception.Message);

            ushort port;
            Task sendLoadSignalTask;

            lock (processManager)
            {
                var freeContainerPorts = processManager.GetAllFreeContainerPorts();
                if (freeContainerPorts.Any()) // There is free container
                {
                    port = freeContainerPorts.First();
                    sendLoadSignalTask = ContainerController.SendLoadSignalToContainerAsync(port, assembly.AssemblyFullPath);
                    processManager.StartContainerProcess(ComputeConfiguration.Instance.ConfigurationItem);
                }
                else // There isn't any free container
                {
                    port = processManager.StartContainerProcess(ComputeConfiguration.Instance.ConfigurationItem);
                    sendLoadSignalTask = ContainerController.SendLoadSignalToContainerAsync(port, assembly.AssemblyFullPath);
                }
            }

            sendLoadSignalTask.Wait();

            Task.Factory.StartNew(() =>
            {
                ContainerController.StartPeriodicHealthCheck(new AssemblyInfo()
                {
                    Port = port,
                    AssemblyFullPath = assembly.AssemblyFullPath
                }, OnContainerFailure).Wait();
            });
        }

        private static List<AssemblyInfo> CopyAssemblies(ComputeConfigurationItem configItem, PackageReaderResult package)
        {
            Console.WriteLine($"Copying assemblies to n={package.NumberOfInstances} destinations...");

            var ports = ProcessManager.Instance.GetAllContainerPorts().Take(package.NumberOfInstances ?? 0).ToList();
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

        private static void SendLoadAssemblySignalToContainers(List<AssemblyInfo> destinationPaths)
        {
            Console.WriteLine("Sending load assembly signal to requested number of container processes...");

            ContainerController.SendLoadSignalToContainersAsync(destinationPaths).GetAwaiter().GetResult();

            Console.WriteLine("All of the processes have finished loading assemblies");
        }

        private static void StartPeriodHealthChecksInTheBackground(List<AssemblyInfo> destinationAssemblies)
        {
            Console.WriteLine("Running periodic health checks in the background... ");
            foreach (var assembly in destinationAssemblies)
            {
                Task.Factory.StartNew((object tempAssembly) =>
                {
                    ContainerController.StartPeriodicHealthCheck((AssemblyInfo)tempAssembly, OnContainerFailure).Wait();
                }, assembly.Clone());
            }
        }

        private static PackageReaderResult StartPeriodicCheckUntilFirstValidPackageIsFound(ComputeConfigurationItem configItem)
        {
            Console.WriteLine("Starting periodic check until first valid package is found...");

            var package = packageManager.PeriodicallyCheckForValidPackageAsync(configItem).GetAwaiter().GetResult();
            Debug.WriteLine(package);
            return package;
        }
    }
}