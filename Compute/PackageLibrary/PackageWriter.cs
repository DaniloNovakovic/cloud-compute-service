using System.IO;

namespace Compute
{
    internal class PackageWriter : IPackageWriter
    {
        public void Copy(string fromFullPath, string toFullPath)
        {
            File.Copy(fromFullPath, toFullPath, overwrite: true);
        }

        public void Delete(string packageFolderFullPath)
        {
            Directory.Delete(packageFolderFullPath, recursive: true);
        }
    }
}