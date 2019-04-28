using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            if (roleInstance.RoleName is null)
            {
                return;
            }

            var role = Roles.GetOrAdd(key: roleInstance.RoleName, valueFactory: roleName => new Role(roleName));
            role.Instances[roleInstance.Id] = roleInstance;
            roleInstance.Role = role;

            Debug.WriteLine($"Successfully added role instance: {roleInstance.Id}");
        }

        public static void SafeRemove(RoleInstance roleInstance)
        {
            if (roleInstance.RoleName is null)
            {
                return;
            }

            if (Roles.TryGetValue(roleInstance.RoleName, out var role) && role.Instances.TryRemove(roleInstance.Id, out var removedInstance))
            {
                Debug.WriteLine($"Successfully removed role instance: {removedInstance.Id}");
            }
            else
            {
                Debug.WriteLine($"Failed to remove role instance: {roleInstance.Id} (possible reason: it is already deleted)");
            }
        }

        #region IRoleEnvironment

        public string AcquireAddress(string myAssemblyName, string containerId)
        {
            string address = string.Empty;

            try
            {
                address = Roles[myAssemblyName].Instances.Values.First(instance => instance.ContainerId == containerId).Address;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"AcquireAddress error: {ex.Message}");
            }
            return address;
        }

        public string[] BrotherInstances(string myAssemblyName, string myAddress)
        {
            var retVal = new string[0];

            try
            {
                retVal = Roles[myAssemblyName]
                    .Instances
                    .Values
                    .Where(instance => instance.Address != myAddress)
                    .Select(instance => instance.Address)
                    .ToArray();
            }
            catch (Exception ex)
            {
                Trace.TraceError($"BrotherInstances error: {ex.Message}");
            }

            return retVal;
        }

        #endregion IRoleEnvironment
    }
}