using System;
using System.ServiceModel;

namespace Container
{
    internal class Server
    {
        public Server(ushort port, Type serviceType, Type implementedContract)
        {
            this.Address = new Uri($"net.tcp://localhost:{port}/{implementedContract.Name}");
            this.Host = new ServiceHost(serviceType);
            this.Host.AddServiceEndpoint(implementedContract, new NetTcpBinding(), this.Address);
        }

        public Uri Address { get; }
        public ServiceHost Host { get; }

        public void Close()
        {
            try
            {
                this.Host.Close();
                Console.WriteLine($"{this.Address} : Closed");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{this.Address} : Failed to close! Reason: {ex.Message}");
            }
        }

        public void Open()
        {
            try
            {
                this.Host.Open();
                Console.WriteLine($"{this.Address} : Opened");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{this.Address} : Failed to open! Reason: {ex.Message}");
            }
        }
    }
}