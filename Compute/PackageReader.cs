using System.Configuration;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Compute
{
    internal static class PackageReader
    {
        /// <summary>
        /// Attempts to extract config values from packageConfigurationPath into PackageReaderResult
        /// </summary>
        /// <param name="packageConfigurationPath">
        /// FullPath of configuration where .xml config file for package is located
        /// </param>
        /// <exception cref="ConfigurationErrorsException">Configuration is in incorrect format</exception>
        public static PackageReaderResult ReadPackage(string packageConfigurationPath)
        {
            var doc = XDocument.Load(packageConfigurationPath);

            if (!int.TryParse(doc.XPathSelectElement("//numberOfInstances").Attribute("value").Value, out int numberOfInstances))
            {
                throw new ConfigurationErrorsException($"numberOfInstances element is either missing or its value is in incorrect format in {packageConfigurationPath}");
            }

            string assemblyName = doc.XPathSelectElement("//assembly/name").Value;

            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                throw new ConfigurationErrorsException($"'<assembly><name>Value</name></assembly>' is missing in {packageConfigurationPath}");
            }

            return new PackageReaderResult()
            {
                NumberOfInstances = numberOfInstances,
                AssemblyName = assemblyName
            };
        }
    }
}