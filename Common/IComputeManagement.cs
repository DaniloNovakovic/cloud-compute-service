using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface IComputeManagement
    {
        [OperationContract]
        string Scale(string assemblyName, int count);
    }
}