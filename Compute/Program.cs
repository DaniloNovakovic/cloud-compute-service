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
        private static readonly PackageManager packageManager = new PackageManager();

        private static void Main()
        {
            var configItem = LoadComputeConfiguration();
            var processManager = StartUpContainerProcesses(configItem);

            try
            {
                var package = StartPeriodicCheckUntilFirstValidPackageIsFound(configItem);
                var destinationPaths = CopyAssemblies(configItem, processManager, package);

                Console.WriteLine("Sending load assembly signal to requested number of container processes...");
                LoadAssemblies(destinationPaths).GetAwaiter().GetResult();

                Console.WriteLine("All of the processes have finished loading assemblies");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();

            processManager.StopAllProcesses();
        }

        private static List<AssemblyInfo> CopyAssemblies(ComputeConfigurationItem configItem, ProcessManager processManager, PackageReaderResult package)
        {
            Console.WriteLine($"Copying assemblies to n={package.NumberOfInstances} destinations...");

            var ports = processManager.GetAllContainerPorts().Take(package.NumberOfInstances ?? 0).ToList();
            string sourceDllFullPath = Path.Combine(configItem.PackageFullFolderPath, package.AssemblyName);
            return packageManager.CopyAssemblies(sourceDllFullPath, configItem.PackageFullFolderPath, ports);
        }

        private static PackageReaderResult StartPeriodicCheckUntilFirstValidPackageIsFound(ComputeConfigurationItem configItem)
        {
            Console.WriteLine("Starting periodic check until first valid package is found...");

            var package = packageManager.PeriodicallyCheckForValidPackage(configItem);
            Debug.WriteLine(package);
            return package;
        }

        private static ProcessManager StartUpContainerProcesses(ComputeConfigurationItem configItem)
        {
            Console.WriteLine("Starting up container processes...");

            var processManager = ProcessManager.Instance;
            processManager.StartContainerProcesses(configItem);
            return processManager;
        }

        private static ComputeConfigurationItem LoadComputeConfiguration()
        {
            Console.WriteLine("Loading Compute configuration...");

            var configItem = ComputeConfiguration.Instance.ConfigurationItem;
            Debug.WriteLine(configItem);
            return configItem;
        }

        private static Task LoadAssemblies(List<AssemblyInfo> assemblies)
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

            return Task.WhenAll(taskList);
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