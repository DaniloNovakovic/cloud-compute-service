using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if (!Validate(assemblyName, count, out string errMsg))
                return errMsg;

            var currRole = RoleEnvironment.Roles[assemblyName];
            int currInstanceCount = currRole.Instances.Count;

            if (currInstanceCount == count)
            {
                return $"[SUCCESS] {assemblyName} already has {count} number of instances!";
            }
            if (currInstanceCount < count)
            {
                int numInstancesToStart = count - currInstanceCount;
                return $"[SUCCESS] Started {numInstancesToStart} number of instances";
            }
            else
            {
                int numInstancesToStop = currInstanceCount - count;
                return $"[SUCCESS] Stopped {numInstancesToStop} number of instances";
            }
        }

        private bool Validate(string assemblyName, int count, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (count > config.NumberOfContainersToStart)
            {
                errorMessage = $"[ERROR] {nameof(count)} ({count}) exceeds total number of containers ({config.NumberOfContainersToStart})";
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