namespace Common
{
    public interface IContainerManagement
    {
        string CheckHealth();

        string Load(string assemblyName);
    }
}