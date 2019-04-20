using System;
using System.Linq;
using System.Reflection;
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
            var assembly = Assembly.LoadFrom(assemblyName);
            var typeInfo = assembly
                .DefinedTypes
                .FirstOrDefault(currType => currType.ImplementedInterfaces.Any(implementedType => implementedType.Name == typeof(IWorker).Name));
            var type = typeInfo.UnderlyingSystemType;
            return (IWorker)Activator.CreateInstance(type);
        }
    }
}