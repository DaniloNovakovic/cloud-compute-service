using System;
using Common;

namespace JobWorker
{
    public class Worker : IWorker
    {
        private string containerId;

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

            Console.WriteLine($"{this.containerId}: Worker stopped");
        }
    }
}