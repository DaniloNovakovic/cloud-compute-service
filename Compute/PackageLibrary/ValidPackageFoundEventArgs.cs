using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Compute
{

    internal class ValidPackageFoundEventArgs : EventArgs
    {
        public ValidPackageFoundEventArgs()
        {
        }

        public ValidPackageFoundEventArgs(PackageReaderResult package)
        {
            this.Package = package ?? throw new ArgumentNullException(nameof(package));
        }

        public PackageReaderResult Package { get; set; }
    }
}