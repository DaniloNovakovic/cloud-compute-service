using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Compute
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine(ComputeConfiguration.Instance);

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }
    }
}