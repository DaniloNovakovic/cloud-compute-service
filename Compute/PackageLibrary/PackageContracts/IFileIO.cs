namespace Compute
{
    public interface IFileIO
    {
        void CopyFile(string sourceFileName, string destFileName);

        void ClearFolder(string path);

        bool FileExists(string path);
    }
}