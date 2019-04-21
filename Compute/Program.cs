﻿using System;
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
            containerHealthMonitor.Start();

            try
            {
                var validPackage = StartPeriodicCheckUntilFirstValidPackageIsFound(configItem);
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

        private static void OnContainerHealthFaulted(object sender, ContainerHealthMonitorEventArgs args)
        {
            Console.WriteLine($"{args.Port}: Problem occured! Reason: {args.Exception.Message}");

            ushort port;
            Task sendLoadSignalTask = null;
            bool containerWasTaken = !string.IsNullOrWhiteSpace(args.AssemblyFullPath);

            lock (processManager)
            {
                var freeContainerPorts = processManager.GetAllFreeContainerPorts();
                if (freeContainerPorts.Any()) // There is free container
                {
                    port = freeContainerPorts.First();
                    if (containerWasTaken)
                    {
                        sendLoadSignalTask = ContainerController.SendLoadSignalToContainerAsync(port, args.AssemblyFullPath);
                    }
                    processManager.StartContainerProcess(ComputeConfiguration.Instance.ConfigurationItem);
                }
                else // There isn't any free container
                {
                    port = processManager.StartContainerProcess(ComputeConfiguration.Instance.ConfigurationItem);
                    if (containerWasTaken)
                    {
                        sendLoadSignalTask = ContainerController.SendLoadSignalToContainerAsync(port, args.AssemblyFullPath);
                    }
                }
            }

            if (sendLoadSignalTask != null)
            {
                sendLoadSignalTask.GetAwaiter().GetResult();
            }
        }

        private static List<PackageAssemblyInfo> CopyAssemblies(ComputeConfigurationItem configItem, PackageReaderResult package)
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

        private static void SendLoadAssemblySignalToContainers(List<PackageAssemblyInfo> destinationPaths)
        {
            Console.WriteLine("Sending load assembly signal to requested number of container processes...");

            ContainerController.SendLoadSignalToContainersAsync(destinationPaths).GetAwaiter().GetResult();

            Console.WriteLine("All of the processes have finished loading assemblies");
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