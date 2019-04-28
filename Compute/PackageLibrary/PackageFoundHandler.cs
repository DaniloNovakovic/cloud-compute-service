using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Compute
{
    internal static class PackageFoundHandler
    {
        public static void OnValidPackageFound(object sender, ValidPackageFoundEventArgs eventArgs)
        {
            var package = eventArgs.Package;

            var destinationAssemblies = CopyAssemblies(package, package.NumberOfInstances ?? 0);
            CopyDependencies(package);
            ContainerController.SendLoadSignalToContainersAsync(destinationAssemblies).GetAwaiter().GetResult();
        }

        private static IEnumerable<RoleInstance> CopyAssemblies(PackageReaderResult package, int numberOfInstances)
        {
            var configItem = ComputeConfiguration.Instance.ConfigurationItem;
            string sourceDllFullPath = Path.Combine(configItem.PackageFullFolderPath, package.AssemblyName);
            return new PackageController().CopyAssemblies(sourceDllFullPath, configItem.PackageTempFullFolderPath, numberOfInstances);
        }

        private static void CopyDependencies(PackageReaderResult package)
        {
            var configItem = ComputeConfiguration.Instance.ConfigurationItem;
            var packageController = new PackageController();
            var root = new DirectoryInfo(configItem.PackageFullFolderPath);

            var fileEnumerator = root
                .EnumerateFiles()
                .Where(fi => fi.Extension == ".dll" && !fi.Name.Contains(package.AssemblyName));

            foreach (var file in fileEnumerator)
            {
                var destPath = Path.Combine(configItem.PackageTempFullFolderPath, file.Name);
                if (!packageController.CopyFile(file.FullName, destPath))
                {
                    Console.Error.WriteLine($"Failed to copy file from {file.FullName} to {destPath}");
                }
            }
        }
    }
}