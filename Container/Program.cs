using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace Container
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            ushort port = GetPortFromArgs(args) ?? 10100;
            ContainerManagement.ContainerId = port.ToString();

            Console.WriteLine($"Starting server on port {port}...");

            var server = new Server(port, typeof(ContainerManagement), typeof(IContainerManagement));
            server.Open();

            Console.WriteLine("Press ENTER to close server...");
            Console.ReadLine();
            server.Close();
        }

        private static ushort? GetPortFromArgs(string[] args)
        {
            var parsable = Array.Find(args, str => Regex.IsMatch(str.Trim(), @"^\d+$"));
            return ushort.TryParse(parsable, out ushort result) ? result : (ushort?)null;
        }
    }
}