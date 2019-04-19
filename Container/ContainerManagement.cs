using Common;

namespace Container
{
    public class ContainerManagement : IContainerManagement
    {
        public string CheckHealth()
        {
            return "Healthy";
        }

        public string Load(string assemblyName)
        {
            throw new System.NotImplementedException();
        }
    }
}