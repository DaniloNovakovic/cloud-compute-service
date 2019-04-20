using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageLibrary
{
    public class PackageReaderResult
    {
        public int? NumberOfInstances { get; set; } = null;
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