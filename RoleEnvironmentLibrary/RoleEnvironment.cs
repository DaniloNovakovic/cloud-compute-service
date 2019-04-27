using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace RoleEnvironmentLibrary
{
    public static class RoleEnvironment
    {
        public static string CurrentRoleInstance(string myAssembly, string containerId)
        {
            var factory = new ChannelFactory<IRoleEnvironment>(new NetTcpBinding(), "net.tcp://localhost:10100/IRoleEnvironment");
            var proxy = factory.CreateChannel();
            string myAddress = proxy.AcquireAddress(myAssembly, containerId);
            BrotherInstances = proxy.BrotherInstances(myAssembly, myAddress);
            return myAddress;
        }

        public static string[] BrotherInstances { get; private set; }
    }
}