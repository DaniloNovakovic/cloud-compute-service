using System.Configuration;
using System.IO;

namespace Compute
{
    public class PackageManager
    {
        private readonly IPackageReader reader;
        private readonly IFileIO writer;

        /// <summary>
        /// Constructs package reader object
        /// </summary>
        /// <param name="reader">Reader to use when reading package (will use default if null)</param>
        /// <param name="writer">Writer to use when reading package (will use default if null)</param>
        public PackageManager(IPackageReader reader = null, IFileIO writer = null)
        {
            this.reader = reader ?? new PackageReader();
            this.writer = writer ?? new FileIO();
        }

        public PackageReaderResult ReadPackage(string packageConfigurationPath, int maxAllowedNumberOfInstances = 4)
        {
            var packageResult = this.reader.ReadPackage(packageConfigurationPath);

            if (packageResult.NumberOfInstances < 0 || packageResult.NumberOfInstances > maxAllowedNumberOfInstances)
            {
                throw new ConfigurationErrorsException(nameof(packageResult.NumberOfInstances));
            }

            if (string.IsNullOrWhiteSpace(packageResult.AssemblyName))
            {
                throw new ConfigurationErrorsException(nameof(packageResult.AssemblyName));
            }

            string assemblyFullPath = Path.GetFullPath(Path.Combine(packageConfigurationPath, $"..\\{packageResult.AssemblyName}"));
            if (!this.writer.FileExists(assemblyFullPath))
            {
                throw new FileNotFoundException(assemblyFullPath);
            }

            return packageResult;
        }

        public void DeletePackage(string packageFolderFullPath)
        {
            this.writer.DeleteFolder(packageFolderFullPath);
        }

        public void CopyFile(string fromFullPath, string toFullPath)
        {
            this.writer.CopyFile(fromFullPath, toFullPath);
        }
    }
}