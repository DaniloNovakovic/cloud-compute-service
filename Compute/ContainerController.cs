﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace Compute
{
    internal static class ContainerController
    {
        public static Task SendLoadSignalToContainersAsync(List<AssemblyInfo> assemblies)
        {
            var taskList = new List<Task>();
            foreach (var assembly in assemblies)
            {
                taskList.Add(SendLoadSignalToContainerAsync(assembly.Port, assembly.AssemblyFullPath));
            }

            return Task.WhenAll(taskList);
        }

        /// <summary>
        /// Attempts to send load signal to container until signal is successfully sent
        /// </summary>
        /// <param name="port">port of remote container's wcf server</param>
        /// <param name="assemblyPath">full path to .dll assembly</param>
        public static async Task SendLoadSignalToContainerAsync(ushort port, string assemblyPath)
        {
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
                        ProcessManager.Instance.TakeContainer(port);
                    }

                    Console.WriteLine(result);

                    channelFactory.Close();
                    break;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                }

                await Task.Delay(1000).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Periodically checks health of remote container. Runs OnContainerError callback upon error/exception.
        /// </summary>
        /// <param name="OnContainerFailure">Callback that is invoked upon error</param>
        public static async Task StartPeriodicHealthCheck(AssemblyInfo assemblyInfo, Action<AssemblyInfo, Exception> OnContainerFailure)
        {
            string remoteAddress = $"net.tcp://localhost:{assemblyInfo.Port}/{typeof(IContainerManagement).Name}";
            while (true)
            {
                try
                {
                    var channelFactory = new ChannelFactory<IContainerManagement>(new NetTcpBinding(), remoteAddress);
                    var proxy = channelFactory.CreateChannel();
                    string result = proxy.CheckHealth();
                    Trace.TraceInformation($"{assemblyInfo.Port}: {DateTime.Now}: {result}");
                    channelFactory.Close();
                }
                catch (Exception ex)
                {
                    if (OnContainerFailure == null)
                    {
                        Console.Error.WriteLine(ex.Message);
                    }
                    else
                    {
                        OnContainerFailure(assemblyInfo, ex);
                    }
                    break;
                }

                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
    }
}