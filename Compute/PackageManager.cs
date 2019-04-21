using System;
using System.Configuration;
using System.IO;
using System.Threading;

namespace Compute
{
    public class PackageManager
    {
        private readonly IFileIO fileIO;
        private readonly IPackageReader reader;

        /// <summary>
        /// Constructs package reader object
        /// </summary>
        /// <param name="reader">Reader to use when reading package (will use default if null)</param>
        /// <param name="fileIO">Writer to use when reading package (will use default if null)</param>
        public PackageManager(IPackageReader reader = null, IFileIO fileIO = null)
        {
            this.reader = reader ?? new PackageReader();
            this.fileIO = fileIO ?? new FileIO();
        }

        /// <summary>
        /// Runs until valid package is found (blocking method)
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public PackageReaderResult PeriodicallyCheckForValidPackage(ComputeConfiguration config)
        {
            string packageConfigPath = Path.Combine(config.PackageFullFolderPath, config.PackageConfigFileName);
            while (true)
            {
                try
                {
                    return this.ReadPackage(packageConfigPath, maxAllowedNumberOfInstances: config.NumberOfContainersToStart);
                }
                catch (ConfigurationException configEx)
                {
                    Console.Error.WriteLine($"ConfigurationException occured while trying to read {packageConfigPath}. Reason: " + configEx.Message);
                    if (this.DeletePackage(config.PackageFullFolderPath))
                    {
                        Console.WriteLine($"Successfully deleted {config.PackageFullFolderPath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Exception occured while trying to read package. Reason: " + ex.Message);
                }

                Thread.Sleep(config.PackageAcquisitionIntervalMilliseconds);
            }
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
        /// Attempts to delete package folder recursively. Returns true upon success.
        /// </summary>
        /// <returns>True upon success, false upon failure</returns>
        public bool DeletePackage(string packageFolderFullPath)
        {
            try
            {
                this.fileIO.DeleteFolder(packageFolderFullPath);
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