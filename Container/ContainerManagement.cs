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
                this.worker = this.assemblyLoader.LoadAssembly(assemblyName);
                this.worker.Start(ContainerId);
                return "[SUCCESS] Successfully loaded assembly!";
            }
            catch (Exception ex)
            {
                return $"[ERROR] Failed to load assembly! Reason: {ex.Message}";
            }
        }
    }
}