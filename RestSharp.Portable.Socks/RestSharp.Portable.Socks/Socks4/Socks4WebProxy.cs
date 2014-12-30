using System;
using System.Linq;
using System.Net;
using System.Threading;

namespace RestSharp.Portable.Socks.Socks4
{
    public class Socks4WebProxy : ISocksWebProxy
    {
        private readonly SocksAddress _socksAddress;

        public Socks4WebProxy(SocksAddress socksAddress)
        {
            _socksAddress = socksAddress;
        }

        public Uri GetProxy(Uri destination)
        {
            if (IsBypassed(destination))
                return null;
            return _socksAddress.ToUri();
        }

        public bool IsBypassed(Uri host)
        {
            return Dns.GetHostAddresses(host.Host).Any(IPAddress.IsLoopback);
        }

        public ICredentials Credentials { get; set; }
    }
}