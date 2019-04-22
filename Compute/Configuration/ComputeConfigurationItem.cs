using System.Linq;
using System.Text;

namespace Compute
{
    public class ComputeConfigurationItem
    {
        public string PackageFullFolderPath { get; internal set; }
        public string PackageConfigFileName { get; internal set; }
        public int NumberOfContainersToStart { get; internal set; }
        public int PackageAcquisitionIntervalMilliseconds { get; internal set; }
        public string ContainerFullFilePath { get; internal set; }
        public string ContainerRelativeFilePath { get; internal set; }
        public ushort MaxPort { get; internal set; }
        public ushort MinPort { get; internal set; }
        public string PackageRelativeFolderPath { get; internal set; }
        public string PackageTempRelativeFolderPath { get; internal set; }
        public string PackageTempFullFolderPath { get; internal set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var property in this.GetType().GetProperties().Where(prop => prop.PropertyType != this.GetType()))
            {
                builder.Append(property.Name).Append(" = ").Append(property.GetValue(this)).AppendLine();
            }

            return builder.ToString();
        }
    }
}