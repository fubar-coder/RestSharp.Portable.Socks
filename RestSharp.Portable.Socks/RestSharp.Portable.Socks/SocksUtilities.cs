using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RestSharp.Portable.Socks
{
    static class SocksUtilities
    {
        private static readonly Encoding _encoding = new UTF8Encoding(false);
        public static Encoding DefaultEncoding { get { return _encoding; } }

        public static UriHostNameType GetHostNameType(IPAddress address)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
                return UriHostNameType.IPv4;
            return UriHostNameType.IPv6;
        }

        public static UriHostNameType GetHostNameType(string host)
        {
            IPAddress address;
            if (!IPAddress.TryParse(host, out address))
                return UriHostNameType.Basic;
            return GetHostNameType(address);
        }
    }
}
