using System;
using System.Linq;
using System.ServiceModel;

namespace Compute
{
    internal class WCFServer
    {
        private readonly ServiceHost host;
        private readonly Uri address;

        public WCFServer(Type serviceType)
        {
            this.host = new ServiceHost(serviceType);
            this.address = this.host.BaseAddresses.FirstOrDefault();
        }

        public WCFServer(Type serviceType, Type implementedContract, ushort port = 10100)
        {
            this.host = new ServiceHost(serviceType);
            this.address = new Uri($"net.tcp://localhost:{port}/{implementedContract.Name}");
            this.host.AddServiceEndpoint(implementedContract, new NetTcpBinding(), this.address);
        }

        public void Open()
        {
            try
            {
                this.host.Open();
                Console.WriteLine($"{address}: Opened");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{address}: Failed to open. Reason: {ex.Message}");
            }
        }

        public void Close()
        {
            try
            {
                this.host.Close();
                Console.WriteLine($"{address}: Closed");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{address}: Failed to close. Reason: {ex.Message}");
            }
        }
    }
}