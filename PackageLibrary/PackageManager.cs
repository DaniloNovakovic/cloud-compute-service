using System.Configuration;

namespace PackageLibrary
{
    public class PackageManager
    {
        private readonly IPackageReader reader;
        private readonly IPackageWriter writer;

        /// <summary>
        /// Constructs package reader object
        /// </summary>
        /// <param name="reader">Reader to use when reading package (will use default if null)</param>
        /// <param name="writer">Writer to use when reading package (will use default if null)</param>
        public PackageManager(IPackageReader reader = null, IPackageWriter writer = null)
        {
            this.reader = reader ?? new PackageReader();
            this.writer = writer ?? new PackageWriter();
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

            return packageResult;
        }

        public void DeletePackage(string packageFolderFullPath)
        {
            this.writer.Delete(packageFolderFullPath);
        }

        public void CopyFile(string fromFullPath, string toFullPath)
        {
            this.writer.Copy(fromFullPath, toFullPath);
        }
    }
}