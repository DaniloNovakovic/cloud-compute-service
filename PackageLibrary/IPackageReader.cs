namespace PackageLibrary
{
    public interface IPackageReader
    {
        PackageReaderResult ReadPackage(string packageConfigurationPath);
    }
}