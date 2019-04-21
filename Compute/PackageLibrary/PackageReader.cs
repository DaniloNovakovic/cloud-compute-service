using System.Configuration;
using System.Text.RegularExpressions;
using System.Xml.XPath;

namespace Compute
{
    public class PackageReader : IPackageReader
    {
        private readonly IXDocumentLoader xDocLoader;

        /// <summary>
        /// Constructs package reader object
        /// </summary>
        /// <param name="loader">Loader to use when reading package (will use default if null)</param>
        public PackageReader(IXDocumentLoader loader = null)
        {
            this.xDocLoader = loader ?? new XDocumentLoader();
        }

        /// <summary>
        /// Attempts to extract config values from packageConfigurationPath into PackageReaderResult
        /// </summary>
        /// <param name="packageConfigurationPath">
        /// FullPath of configuration where .xml config file for package is located
        /// </param>
        /// <exception cref="ConfigurationErrorsException">Configuration is in incorrect format</exception>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException"></exception>
        public PackageReaderResult ReadPackage(string packageConfigurationPath)
        {
            var xDoc = this.xDocLoader.Load(packageConfigurationPath);

            if (!int.TryParse(xDoc.XPathSelectElement("//numberOfInstances").Attribute("value").Value, out int numberOfInstances))
            {
                throw new ConfigurationErrorsException($"numberOfInstances element is either missing or its value is in incorrect format in {packageConfigurationPath}");
            }

            string assemblyName = xDoc.XPathSelectElement("//assembly/name").Value;

            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                throw new ConfigurationErrorsException($"'<assembly><name>Value</name></assembly>' is missing in {packageConfigurationPath}");
            }

            if (!Regex.IsMatch(assemblyName, @".*\.dll$"))
                assemblyName += ".dll";

            return new PackageReaderResult()
            {
                NumberOfInstances = numberOfInstances,
                AssemblyName = assemblyName
            };
        }
    }
}