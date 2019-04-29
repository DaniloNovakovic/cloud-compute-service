using System;
using System.Configuration;
using System.ServiceModel;
using System.Text.RegularExpressions;
using Common;

namespace Client
{
    internal static class Program
    {
        private static void Main()
        {
            while (true)
            {
                Console.Write("Enter role / assembly name (case-sensitive): ");

                string assemblyName = Console.ReadLine();

                Console.Write("Enter scale value (integer): ");

                if (!int.TryParse(Console.ReadLine(), out int scaleCount))
                {
                    Console.Error.WriteLine("Failed to parse entered number. Please try again...");
                    continue;
                }

                SafeScale(assemblyName, scaleCount);

                Console.Write("Quit (y/N)? ");
                switch (Console.ReadLine())
                {
                    case "y":
                    case "Y":
                        return;
                }

                Console.WriteLine();
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

                Console.ForegroundColor = Regex.IsMatch(response, @"\[ERROR\]", RegexOptions.IgnoreCase)
                    ? ConsoleColor.Red
                    : ConsoleColor.Green;

                Console.WriteLine(response);

                Console.ResetColor();

                factory.Close();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}