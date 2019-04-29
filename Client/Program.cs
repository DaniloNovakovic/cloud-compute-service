using System;
using System.Configuration;
using System.ServiceModel;
using Common;

namespace Client
{
    internal static class Program
    {
        private const string assemblyConfigKeyName = "assemblyName";

        private static void Main()
        {
            string assemblyName = ConfigurationManager.AppSettings.Get(assemblyConfigKeyName)
                ?? throw new ConfigurationErrorsException($"<appSettings> <add key='{assemblyConfigKeyName}' value='...' /> in configuration is missing");

            Console.WriteLine($"{nameof(assemblyName)}: {assemblyName}");

            while (true)
            {
                Console.Write("Enter scale value (integer): ");

                if (!int.TryParse(Console.ReadLine(), out int scaleCount))
                {
                    Console.Error.WriteLine("Failed to parse entered number. Please try again...");
                    continue;
                }

                SafeScale(assemblyName, scaleCount);
            }
        }

        private static void SafeScale(string assemblyName, int scaleCount)
        {
            try
            {
                var factory = new ChannelFactory<IComputeManagement>(
                    endpointConfigurationName: typeof(IComputeManagement).FullName);
                var proxy = factory.CreateChannel();

                string response = proxy.Scale(assemblyName, scaleCount);
                Console.WriteLine(response);

                factory.Close();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}