using System;
using Common;

namespace Container
{
    public class ContainerManagement : IContainerManagement
    {
        private readonly IAssemblyLoader assemblyLoader;
        private IWorker worker;

        static ContainerManagement()
        {
            var randomGenerator = new Random();
            ContainerId = randomGenerator.Next(10100, ushort.MaxValue).ToString();
        }

        public ContainerManagement() : this(new AssemblyLoader())
        {
        }

        public ContainerManagement(IAssemblyLoader assemblyLoader)
        {
            this.assemblyLoader = assemblyLoader ?? throw new ArgumentNullException(nameof(assemblyLoader));
        }

        public static string ContainerId { get; internal set; }

        public string CheckHealth()
        {
            return "Healthy";
        }

        public string Load(string assemblyName)
        {
            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                return $"[ERROR] {nameof(assemblyName)} can't be null, empty or whitespace!";
            }

            try
            {
                var newWorker = this.assemblyLoader.LoadAssembly(assemblyName);
                newWorker.Start(ContainerId);
                this.worker?.Stop();
                this.worker = newWorker;
                return $"[SUCCESS] Successfully loaded assembly '{assemblyName}'!";
            }
            catch (Exception ex)
            {
                return $"[ERROR] Failed to load assembly '{assemblyName}'! Reason: {ex.Message}";
            }
        }
    }
}