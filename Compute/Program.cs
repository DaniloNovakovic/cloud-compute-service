using System;
using System.Collections.Generic;
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

            var configItem = ComputeConfiguration.Instance.ConfigurationItem;
            Debug.WriteLine(configItem);

            Console.WriteLine("Starting up container processes...");

            var processManager = ProcessManager.Instance;
            processManager.StartContainerProcesses(configItem);

            try
            {
                Console.WriteLine("Starting periodic check until first valid package is found...");

                var packageManager = new PackageManager();
                var package = packageManager.PeriodicallyCheckForValidPackage(configItem);
                Debug.WriteLine(package);

                Console.WriteLine("Sending load assembly signal to requested number of container processes...");

                var ports = processManager.GetAllContainerPorts().Take(package.NumberOfInstances ?? 0).ToList();
                string sourceDllFullPath = Path.Combine(configItem.PackageFullFolderPath, package.AssemblyName);
                var destinationPaths = CopyAssemblies(sourceDllFullPath, configItem.PackageFullFolderPath, ports);
                var taskList = LoadAssemblies(destinationPaths);

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

        private class AssemblyInfo
        {
            public ushort Port { get; set; }
            public string AssemblyFullPath { get; set; }
        }

        private static List<AssemblyInfo> CopyAssemblies(string sourceDllFullPath, string destinationFolder, List<ushort> ports)
        {
            var packageManager = new PackageManager();
            var destinationPaths = new List<AssemblyInfo>();
            foreach (ushort port in ports)
            {
                string destinationDllFullPath = Path.Combine(destinationFolder, $"JobWorker_{port}.dll");
                if (packageManager.CopyFile(sourceDllFullPath, destinationDllFullPath))
                {
                    destinationPaths.Add(new AssemblyInfo()
                    {
                        Port = port,
                        AssemblyFullPath = destinationDllFullPath
                    });
                }
            }
            return destinationPaths;
        }

        private static List<Task> LoadAssemblies(List<AssemblyInfo> assemblies)
        {
            var taskList = new List<Task>();
            foreach (var assembly in assemblies)
            {
                taskList.Add(Task.Factory.StartNew(
                    (dynamic dobj) =>
                    {
                        SendLoadSignalToContainers(dobj.port, dobj.assemblyPath);
                    },
                    new { port = assembly.Port, assemblyPath = assembly.AssemblyFullPath }));
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
    }
}