using System;
using System.Linq;
using System.Net;
using System.Threading;

namespace RestSharp.Portable.Socks.Socks5
{
    public class Socks5WebProxy : ISocksWebProxy
    {
        private readonly SocksAddress _socksAddress;

        public Socks5WebProxy(SocksAddress socksAddress)
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
            return SocksUtilities.IsLoopBack(host.Host);
        }

        public ICredentials Credentials { get; set; }
    }
}