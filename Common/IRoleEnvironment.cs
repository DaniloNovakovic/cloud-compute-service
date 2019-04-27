using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface IRoleEnvironment
    {
        [OperationContract]
        string AcquireAddress(string myAssemblyName, string containerId);

        [OperationContract]
        string[] BrotherInstances(string myAssemblyName, string myAddress);
    }
}