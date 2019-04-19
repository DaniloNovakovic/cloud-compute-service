namespace Common
{
    public interface IRoleEnvironment
    {
        string AcquireAddress(string myAssemblyName, string containerId);

        string[] BrotherInstances(string myAssemblyName, string myAddress);
    }
}