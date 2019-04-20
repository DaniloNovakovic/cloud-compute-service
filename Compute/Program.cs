using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PackageLibrary;

namespace Compute
{
    internal static class Program
    {
        private static void Main()
        {
            var config = ComputeConfiguration.Instance;
            Console.WriteLine(config);

            var packageResult = new PackageReader().ReadPackage(Path.Combine(config.PackageFullFolderPath, config.PackageConfigFileName));
            Console.WriteLine(packageResult.NumberOfInstances);
            Console.WriteLine(packageResult.AssemblyName);

            for (int i = 0; i < config.NumberOfContainersToStart; ++i)
            {
                Process.Start(fileName: config.ContainerFullFilePath, arguments: $"{config.MinPort + 1 + i}");
            }

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }
    }
}