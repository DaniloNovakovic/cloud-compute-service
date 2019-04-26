using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compute
{
    internal static class PackageFoundHandler
    {
        public static void OnValidPackageFound(object sender, ValidPackageFoundEventArgs eventArgs)
        {
            var package = eventArgs.Package;
            var ports = ProcessManager.Instance.GetAllContainerPorts().Take(package.NumberOfInstances ?? 0).ToList();
            var destinationAssemblies = CopyAssemblies(package, ports);
            ContainerController.SendLoadSignalToContainersAsync(destinationAssemblies).GetAwaiter().GetResult();
        }

        private static IEnumerable<PackageAssemblyInfo> CopyAssemblies(PackageReaderResult package, List<ushort> ports)
        {
            var configItem = ComputeConfiguration.Instance.ConfigurationItem;
            string sourceDllFullPath = Path.Combine(configItem.PackageFullFolderPath, package.AssemblyName);
            return new PackageController().CopyAssemblies(sourceDllFullPath, configItem.PackageFullFolderPath, ports);
        }
    }
}