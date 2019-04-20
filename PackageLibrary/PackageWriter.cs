using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageLibrary
{
    internal class PackageWriter : IPackageWriter
    {
        public void Copy(string fromFullPath, string toFullPath)
        {
            throw new NotImplementedException();
        }

        public void Delete(string packageFolderFullPath)
        {
            throw new NotImplementedException();
        }
    }
}