using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace Compute
{
    internal static class ContainerController
    {
        public static Task SendLoadSignalToContainers(List<AssemblyInfo> assemblies)
        {
            var taskList = new List<Task>();
            foreach (var assembly in assemblies)
            {
                taskList.Add(Task.Factory.StartNew(
                    (dynamic dobj) =>
                    {
                        ContainerController.SendLoadSignalToContainer(dobj.port, dobj.assemblyPath);
                    },
                    new { port = assembly.Port, assemblyPath = assembly.AssemblyFullPath }));
            }

            return Task.WhenAll(taskList);
        }

        public static void SendLoadSignalToContainer(int port, string assemblyPath)
        {
            Thread.Sleep(1000);

            string remoteAddress = $"net.tcp://localhost:{port}/{typeof(IContainerManagement).Name}";
            while (true)
            {
                try
                {
                    var channelFactory = new ChannelFactory<IContainerManagement>(new NetTcpBinding(), remoteAddress);
                    var proxy = channelFactory.CreateChannel();
                    string result = proxy.Load(assemblyPath);
                    Console.WriteLine(result);
                    channelFactory.Close();
                    return;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }

                Thread.Sleep(1000);
            }
        }
    }
}