using Common;

namespace Container
{
    public interface IAssemblyLoader
    {
        IWorker LoadAssembly(string assemblyName);
    }
}