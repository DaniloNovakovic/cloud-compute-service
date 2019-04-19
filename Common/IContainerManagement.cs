using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface IContainerManagement
    {
        [OperationContract]
        string CheckHealth();

        [OperationContract]
        string Load(string assemblyName);
    }
}