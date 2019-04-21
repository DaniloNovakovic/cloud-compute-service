﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace Compute
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine("Loading Compute configuration...");

            var config = ComputeConfiguration.Instance;
            Debug.WriteLine(config);

            Console.WriteLine("Starting up container processes...");

            var processManager = ProcessManager.Instance;
            processManager.StartContainerProcesses(config);

            try
            {
                Console.WriteLine("Starting periodic check until first valid package is found...");

                var packageManager = new PackageManager();
                var package = PeriodicallyCheckForValidPackage(config, packageManager);
                Debug.WriteLine(package);

                Console.WriteLine("Sending load assembly signal to requested number of container processes...");

                var ports = processManager.GetAllContainerPorts().Take(package.NumberOfInstances ?? 0).ToList();
                string sourceDllFullPath = Path.Combine(config.PackageFullFolderPath, package.AssemblyName);
                var taskList = LoadAssemblies(config, packageManager, ports, sourceDllFullPath);

                Task.WhenAll(taskList).GetAwaiter().GetResult();

                Console.WriteLine("All of the processes have finished loading assemblies");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: " + ex.Message);
            }

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

            processManager.StopAllProcesses();
        }

        private static List<Task> LoadAssemblies(ComputeConfiguration config, PackageManager packageManager, List<ushort> ports, string sourceDllFullPath)
        {
            var taskList = new List<Task>();
            foreach (ushort port in ports)
            {
                string destinationDllFullPath = Path.Combine(config.PackageFullFolderPath, $"JobWorker_{port}.dll");
                packageManager.CopyFile(sourceDllFullPath, destinationDllFullPath);

                taskList.Add(Task.Factory.StartNew((dynamic dobj) =>
                {
                    SendLoadSignalToContainers(dobj.port, dobj.assemblyPath);
                }, new { port, assemblyPath = destinationDllFullPath }));
            }

            return taskList;
        }

        private static void SendLoadSignalToContainers(int port, string assemblyPath)
        {
            Thread.Sleep(1000);

            string remoteAddress = $"net.tcp://localhost:{port}/{typeof(IContainerManagement).Name}";
            while (true)
            {
                try
                {
                    var channelFactory = new ChannelFactory<IContainerManagement>(new NetTcpBinding(), remoteAddress);
                    var proxy = channelFactory.CreateChannel();
                    string result = proxy.Load(assemblyPath);
                    Console.WriteLine(result);
                    channelFactory.Close();
                    return;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }

                Thread.Sleep(1000);
            }
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