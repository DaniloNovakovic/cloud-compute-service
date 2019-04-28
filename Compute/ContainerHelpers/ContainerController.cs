using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common;

namespace Compute
{
    internal static class ContainerController
    {
        /// <summary>
        /// Attempts to send load signal to container until signal has been sent or attempts have
        /// exceeded numberOfAttempts.
        /// </summary>
        /// <param name="port">port of remote container's wcf server</param>
        /// <param name="assemblyPath">full path to .dll assembly</param>
        /// <param name="numberOfAttempts">defines number of attempts to establish connection</param>
        /// <param name="millisecondsDelay">delay/time in milliseconds between each attempt</param>
        /// <returns>true if connection has been established and Load signal has been sent.</returns>
        public static async Task<bool> SendLoadSignalToContainerAsync(RoleInstance roleInstance, int numberOfAttempts = 2, int millisecondsDelay = 500)
        {
            var port = roleInstance.Port;
            var assemblyPath = roleInstance.AssemblyFullPath;
            string remoteAddress = $"net.tcp://localhost:{port}/{typeof(IContainerManagement).Name}";
            while (true)
            {
                try
                {
                    var channelFactory = new ChannelFactory<IContainerManagement>(new NetTcpBinding(), remoteAddress);
                    var proxy = channelFactory.CreateChannel();
                    string result = proxy.Load(assemblyPath);

                    if (Regex.IsMatch(result, @"^\s*?\[SUCCESS\].*", RegexOptions.IgnoreCase))
                    {
                        ProcessManager.SingletonInstance.TakeContainer(roleInstance);
                    }

                    Console.WriteLine($"{port}: {result}");

                    channelFactory.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);

                    if (--numberOfAttempts <= 0)
                    {
                        return false;
                    }
                }

                await Task.Delay(millisecondsDelay).ConfigureAwait(false);
            }
        }

        public static Task SendLoadSignalToContainersAsync(IEnumerable<RoleInstance> instances)
        {
            var taskList = new List<Task>();
            foreach (var instance in instances)
            {
                taskList.Add(SendLoadSignalToContainerAsync(instance));
            }

            return Task.WhenAll(taskList);
        }
    }
}