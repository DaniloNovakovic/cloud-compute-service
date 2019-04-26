using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace Compute
{
    public class PackageController
    {
        private readonly IFileIO fileIO;
        private readonly IPackageReader reader;

        /// <summary>
        /// Constructs package reader object
        /// </summary>
        /// <param name="reader">Reader to use when reading package (will use default if null)</param>
        /// <param name="fileIO">Writer to use when reading package (will use default if null)</param>
        public PackageController(IPackageReader reader = null, IFileIO fileIO = null)
        {
            this.reader = reader ?? new PackageReader();
            this.fileIO = fileIO ?? new FileIO();
        }

        /// <summary>
        /// Copies sourceDll to new file for each provided port.
        /// </summary>
        /// <param name="sourceDllFullPath"></param>
        /// <param name="destinationFolder"></param>
        /// <param name="ports"></param>
        /// <returns>List of successfully copied assemblies</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public IEnumerable<PackageAssemblyInfo> CopyAssemblies(string sourceDllFullPath, string destinationFolder, IEnumerable<ushort> ports)
        {
            var destinationPaths = new List<PackageAssemblyInfo>();
            string assemblyName = Path.GetFileNameWithoutExtension(sourceDllFullPath);
            int instanceCount = 0;

            foreach (ushort port in ports)
            {
                string destinationDllFullPath = Path.Combine(destinationFolder, $"{assemblyName}_{instanceCount++}.dll");
                if (this.CopyFile(sourceDllFullPath, destinationDllFullPath))
                {
                    destinationPaths.Add(new PackageAssemblyInfo()
                    {
                        Port = port,
                        AssemblyFullPath = destinationDllFullPath
                    });
                }
            }
            return destinationPaths;
        }

        /// <summary>
        /// Attempts to copy file. Returns true upon success.
        /// </summary>
        /// <returns>True upon success, false upon failure</returns>
        public bool CopyFile(string fromFullPath, string toFullPath)
        {
            try
            {
                this.fileIO.CopyFile(fromFullPath, toFullPath);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Attempts to delete all files and folders inside folder with specified path. Returns true
        /// upon success.
        /// </summary>
        /// <returns>True upon success, false upon failure</returns>
        public bool DeletePackage(string packageFolderFullPath)
        {
            try
            {
                this.fileIO.ClearFolder(packageFolderFullPath);
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Reads package from packageConfigurationPath
        /// </summary>
        /// <exception cref="ConfigurationErrorsException">configuration at given path is invalid</exception>
        /// <exception cref="FileNotFoundException">
        /// AssemblyName specified in configuration is invalid
        /// </exception>
        /// <exception cref="ArgumentException">one of the arguments is invalid</exception>
        /// <exception cref="ArgumentNullException">one of the arguments is null</exception>
        /// <exception cref="PathTooLongException">
        /// either packageConfigurationPath or packageConfigurationPath\assemblyName path is too long
        /// </exception>
        public PackageReaderResult ReadPackage(string packageConfigurationPath, int maxAllowedNumberOfInstances = 4)
        {
            var packageResult = this.reader.ReadPackage(packageConfigurationPath);

            this.ValidatePackageResult(packageResult, packageConfigurationPath, maxAllowedNumberOfInstances);

            return packageResult;
        }

        /// <summary>
        /// Throws exception if packageResult is invalid
        /// </summary>
        /// <exception cref="ConfigurationErrorsException">packageResult is invalid</exception>
        /// <exception cref="FileNotFoundException">assemblyName is invalid</exception>
        /// <exception cref="ArgumentException">one of the arguments is invalid</exception>
        /// <exception cref="ArgumentNullException">one of the arguments is null</exception>
        /// <exception cref="PathTooLongException">
        /// either packageConfigurationPath or packageConfigurationPath\assemblyName path is too long
        /// </exception>
        private void ValidatePackageResult(PackageReaderResult packageResult, string packageConfigurationPath, int maxAllowedNumberOfInstances = 4)
        {
            if (packageResult == null)
            {
                throw new ArgumentNullException(nameof(packageResult));
            }

            if (string.IsNullOrWhiteSpace(packageConfigurationPath))
            {
                throw new ArgumentException($"{nameof(packageConfigurationPath)} can't be null, empty or white space!");
            }

            if (packageResult.NumberOfInstances < 0 || packageResult.NumberOfInstances > maxAllowedNumberOfInstances)
            {
                throw new ConfigurationErrorsException(nameof(packageResult.NumberOfInstances));
            }

            if (string.IsNullOrWhiteSpace(packageResult.AssemblyName))
            {
                throw new ConfigurationErrorsException(nameof(packageResult.AssemblyName));
            }

            string assemblyFullPath = Path.GetFullPath(Path.Combine(packageConfigurationPath, $"..\\{packageResult.AssemblyName}"));
            if (!this.fileIO.FileExists(assemblyFullPath))
            {
                throw new FileNotFoundException(assemblyFullPath);
            }
        }
    }
}