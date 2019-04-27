using System;
using System.Threading;
using Common;
using RoleEnvironmentLibrary;

namespace JobWorker
{
    public class Worker : IWorker
    {
        private string containerId;
        private Thread thread;

        /// <summary>
        /// Starts the worker
        /// </summary>
        /// <param name="containerId">Id of container that called worker</param>
        /// <exception cref="ArgumentException">ContainerId is null, empty or white space</exception>
        public void Start(string containerId)
        {
            if (string.IsNullOrWhiteSpace(containerId))
            {
                throw new ArgumentException($"{nameof(containerId)} mustn't be null, empty or white space!");
            }

            this.containerId = containerId;
            Console.WriteLine($"{containerId}: Worker started");

            thread = new Thread(DoWork)
            {
                IsBackground = true
            };
            thread.Start();
        }

        /// <summary>
        /// Stops the worker
        /// </summary>
        /// <exception cref="InvalidOperationException">Stop has been called before Start</exception>
        public void Stop()
        {
            if (this.containerId is null)
            {
                throw new InvalidOperationException($"Worker hasn't been started yet.");
            }

            if (thread.IsAlive)
            {
                thread.Abort();
            }

            Console.WriteLine($"{this.containerId}: Worker stopped");
        }

        private void DoWork()
        {
            try
            {
                var currentRoleInstance = RoleEnvironment.CurrentRoleInstance("JobWorker", containerId);
                var brotherInstances = RoleEnvironment.BrotherInstances;

                Console.WriteLine($"Current role instance: {currentRoleInstance}");
                Console.WriteLine("Brother instances: ");
                if (brotherInstances != null)
                {
                    foreach (var brother in brotherInstances)
                    {
                        Console.WriteLine(brother);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}