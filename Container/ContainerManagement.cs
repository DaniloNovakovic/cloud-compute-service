using System;
using Common;

namespace Container
{
    public class ContainerManagement : IContainerManagement
    {
        private readonly IAssemblyLoader assemblyLoader;
        private IWorker worker;

        public ContainerManagement()
        {
        }

        public ContainerManagement(IAssemblyLoader assemblyLoader)
        {
            this.assemblyLoader = assemblyLoader ?? throw new ArgumentNullException(nameof(assemblyLoader));
        }

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
                this.worker.Start("1"); // TODO: Find a way to send proper id
                return "[SUCCESS] Successfully loaded assembly!";
            }
            catch (Exception ex)
            {
                return $"[ERROR] Failed to load assembly! Reason: {ex.Message}";
            }
        }
    }
}