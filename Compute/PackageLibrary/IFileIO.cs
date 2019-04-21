namespace Compute
{
    public interface IFileIO
    {
        void CopyFile(string sourceFileName, string destFileName);

        void DeleteFolder(string path);

        bool FileExists(string path);
    }
}