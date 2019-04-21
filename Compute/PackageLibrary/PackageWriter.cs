using System.IO;

namespace Compute
{
    internal class FileIO : IFileIO
    {
        public void CopyFile(string sourceFileName, string destFileName)
        {
            File.Copy(sourceFileName, destFileName, overwrite: true);
        }

        public void DeleteFolder(string path)
        {
            Directory.Delete(path, recursive: true);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }
    }
}