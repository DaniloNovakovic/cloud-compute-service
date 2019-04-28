using System;
using System.Collections.Concurrent;
using Common;

namespace Compute
{
    internal class RoleEnvironment : IRoleEnvironment
    {
        static RoleEnvironment()
        {
            Roles = new ConcurrentDictionary<string, Role>();
        }

        public static ConcurrentDictionary<string, Role> Roles { get; }

        public static void SafeAddOrUpdate(RoleInstance roleInstance)
        {
            var role = Roles.GetOrAdd(key: roleInstance.RoleName, valueFactory: roleName => new Role(roleName));
            role.Instances[roleInstance.Id] = roleInstance;
            roleInstance.Role = role;
        }

        #region IRoleEnvironment

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

        #endregion IRoleEnvironment
    }
}