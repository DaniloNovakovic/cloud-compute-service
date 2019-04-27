using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Compute
{
    internal class RoleEnvironmentService : IRoleEnvironment
    {
        public string AcquireAddress(string myAssemblyName, string containerId)
        {
            Console.WriteLine($"{myAssemblyName}: containerId - {containerId}");
            return containerId;
        }

        public string[] BrotherInstances(string myAssemblyName, string myAddress)
        {
            Console.WriteLine($"{myAssemblyName}: myAddress - {myAddress}");
            return null;
        }
    }
}