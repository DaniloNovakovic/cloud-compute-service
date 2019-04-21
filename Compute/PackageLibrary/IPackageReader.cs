namespace Compute
{
    public interface IPackageReader
    {
        PackageReaderResult ReadPackage(string packageConfigurationPath);
    }
}