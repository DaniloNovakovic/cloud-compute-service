using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageLibrary
{
    public interface IPackageWriter
    {
        void Delete(string packageFolderFullPath);

        void Copy(string fromFullPath, string toFullPath);
    }
}