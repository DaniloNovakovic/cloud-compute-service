using System;
using Common;

namespace Container
{
    internal class AssemblyLoader : IAssemblyLoader
    {
        /// <summary>
        /// Attempts to load IWorker from assembly
        /// </summary>
        /// <param name="assemblyName">File name of assembly</param>
        /// <returns>IWorker interface that loaded assembly implemented</returns>
        public IWorker LoadAssembly(string assemblyName)
        {
            var domain = AppDomain.CreateDomain("WorkerDomain");
            var type = typeof(IWorker);
            if (!(domain.CreateInstanceFromAndUnwrap(assemblyName, type.Name) is IWorker runnable))
            {
                throw new BadImageFormatException($"Assembly specified at {assemblyName} does not implement {type.Name} interface");
            }
            return runnable;
        }
    }
}