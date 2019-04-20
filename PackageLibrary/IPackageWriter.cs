namespace PackageLibrary
{
    public interface IPackageWriter
    {
        void Copy(string fromFullPath, string toFullPath);

        void Delete(string packageFolderFullPath);
    }
}