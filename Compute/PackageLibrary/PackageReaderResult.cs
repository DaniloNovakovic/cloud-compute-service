using System.Linq;
using System.Text;

namespace Compute
{
    public class PackageReaderResult
    {
        public int? NumberOfInstances { get; set; }
        public string AssemblyName { get; set; } = string.Empty;

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