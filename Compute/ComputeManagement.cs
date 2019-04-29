using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common;

namespace Compute
{
    internal class ComputeManagement : IComputeManagement
    {
        private readonly ComputeConfigurationItem config = ComputeConfiguration.Instance.ConfigurationItem;
        private readonly ProcessManager processManager = ProcessManager.SingletonInstance;

        public string Scale(string assemblyName, int count)
        {
            if (!this.Validate(assemblyName, count, out string errMsg))
            {
                return errMsg;
            }

            var currRole = RoleEnvironment.Roles[assemblyName];
            int currInstanceCount = currRole.Instances.Count;

            if (currInstanceCount == count)
            {
                return $"[SUCCESS] {assemblyName} already has {count} number of instances!";
            }
            if (currInstanceCount < count)
            {
                int numInstancesToStart = count - currInstanceCount;
                this.StartInstances(assemblyName, numInstancesToStart);
                return $"[SUCCESS] Started {numInstancesToStart} number of instances";
            }
            else
            {
                int numInstancesToStop = currInstanceCount - count;
                this.StopInstances(currRole, numInstancesToStop);
                return $"[SUCCESS] Stopped {numInstancesToStop} number of instances";
            }
        }

        private void StartInstance(string assemblyName)
        {
            int instancesCount = RoleEnvironment.Roles[assemblyName].Instances.Count;
            string sourceDllPath = Path.GetFullPath(Path.Combine(this.config.PackageFullFolderPath, assemblyName + ".dll"));
            string destinationPath = Path.GetFullPath(Path.Combine(this.config.PackageTempFullFolderPath, $"{assemblyName}_{instancesCount + 1}"));

            var packageController = new PackageController();
            packageController.CopyFile(sourceDllPath, destinationPath);

            RoleInstance newInstance;
            lock (this.processManager)
            {
                newInstance = new RoleInstance()
                {
                    RoleName = assemblyName,
                    AssemblyFullPath = destinationPath,
                    Port = this.processManager.GetAllFreeContainerPorts().First()
                };
                this.processManager.TakeContainer(newInstance);
            }
            ContainerController
                .SendLoadSignalToContainerAsync(newInstance, numberOfAttempts: 1, millisecondsDelay: 0)
                .Wait();
        }

        private void StartInstances(string assemblyName, int numInstancesToStart)
        {
            for (int i = 0; i < numInstancesToStart; ++i)
            {
                Task.Run(() => this.StartInstance(assemblyName));
            }
        }

        private void StopInstances(Role currRole, int numInstancesToStop)
        {
            currRole.Instances.Values.Take(numInstancesToStop).ToList()
                .ForEach(instance => this.processManager.FreeContainer(instance.Port));
        }

        private bool Validate(string assemblyName, int count, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (count > this.config.NumberOfContainersToStart)
            {
                errorMessage = $"[ERROR] {nameof(count)} ({count}) exceeds total number of containers ({this.config.NumberOfContainersToStart})";
                return false;
            }
            if (count < 0)
            {
                errorMessage = $"[ERROR] {nameof(count)} can't be a negative number!";
                return false;
            }

            if (!RoleEnvironment.Roles.TryGetValue(assemblyName, out var role))
            {
                errorMessage = $"[ERROR] role with name {assemblyName} does not exist!";
                return false;
            }

            return true;
        }
    }
}