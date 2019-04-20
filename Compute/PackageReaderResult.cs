using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compute
{
    internal class PackageReaderResult
    {
        public int? NumberOfInstances { get; set; } = null;
        public string AssemblyName { get; set; } = string.Empty;
    }
}