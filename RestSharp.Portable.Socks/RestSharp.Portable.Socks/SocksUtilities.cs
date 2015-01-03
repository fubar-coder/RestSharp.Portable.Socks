using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
#if WINRT
using Windows.Networking;
using Windows.Networking.Sockets;
#else
using System.Net.Sockets;
#endif

namespace RestSharp.Portable.Socks
{
    internal static class SocksUtilities
    {
        internal enum IPv4SupportLevel
        {
            RequiresIPv4,
            NoPreference,
            RequiresIPv6,
        }

#if !SILVERLIGHT
        private static readonly Random _addressRng = new Random();
#endif
        private static readonly Encoding _encoding = new UTF8Encoding(false);
        public static Encoding DefaultEncoding { get { return _encoding; } }

#if WINRT
        public static EndPointType GetHostNameType(this HostName address)
        {
            switch (address.Type)
            {
                case HostNameType.Ipv4:
                    return EndPointType.IPv4;
                case HostNameType.Ipv6:
                    return EndPointType.IPv6;
                case HostNameType.DomainName:
                    return EndPointType.HostName;
            }
            throw new NotSupportedException();
        }
#else
        public static EndPointType GetHostNameType(this IPAddress address)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
                return EndPointType.IPv4;
            return EndPointType.IPv6;
        }
#endif

        public static EndPointType GetHostNameType(this Uri address)
        {
#if SILVERLIGHT
            return GetHostNameType(address.Host);
#else
            switch (address.HostNameType)
            {
                case UriHostNameType.IPv4:
                    return EndPointType.IPv4;
                case UriHostNameType.IPv6:
                    return EndPointType.IPv6;
                default:
                    return EndPointType.HostName;
            }
#endif
        }

        public static EndPointType GetHostNameType(string host)
        {
#if WINRT
            return GetHostNameType(new HostName(host));
#else
            IPAddress address;
            if (!IPAddress.TryParse(host, out address))
                return EndPointType.HostName;
            return GetHostNameType(address);
#endif
        }

        internal static bool IsLoopBack(string host)
        {
#if WINRT
            var allAddressesTask = DatagramSocket.GetEndpointPairsAsync(new HostName(host), "0").AsTask();
            allAddressesTask.Wait();
            var allAddresses = allAddressesTask.Result
                .Where(x => x != null && x.RemoteHostName != null && (x.RemoteHostName.Type == HostNameType.Ipv4 || x.RemoteHostName.Type == HostNameType.Ipv6))
                .Select(x => x.RemoteHostName)
                .ToList();
            return allAddresses.Any(x => x.IsLoopBack());
#elif SILVERLIGHT
            return false;
#else
            return Dns.GetHostAddresses(host).Any(IPAddress.IsLoopback);
#endif
        }

        internal static async Task<string> ResolveHost(string host, IPv4SupportLevel supportLevel)
        {
#if WINRT
            var allAddresses = (await DatagramSocket.GetEndpointPairsAsync(new HostName(host), "0"))
                .Where(x => x != null && x.RemoteHostName != null)
                .Select(x => x.RemoteHostName)
                .ToList();
            var addressesIPv4 = allAddresses.Where(x => x.Type == HostNameType.Ipv4)
                .ToList();
            HostName addr;
            if (supportLevel == IPv4SupportLevel.RequiresIPv4)
            {
                if (addressesIPv4.Count != 0)
                    addr = addressesIPv4[_addressRng.Next(0, addressesIPv4.Count)];
                else
                    addr = null;
            }
            else
            {
                var addressesIPv6 = allAddresses.Where(x => x.Type == HostNameType.Ipv6)
                    .ToList();
                if (supportLevel == IPv4SupportLevel.NoPreference)
                    addressesIPv6.AddRange(addressesIPv4);
                if (addressesIPv6.Count != 0)
                    addr = addressesIPv6[_addressRng.Next(0, addressesIPv6.Count)];
                else
                    addr = null;
            }
            if (addr == null)
                return null;
            return addr.CanonicalName;
#elif SILVERLIGHT
            return await Task.Factory.StartNew<string>(() =>
            {
                throw new NotSupportedException();
            });
#else
            var allAddresses = (await Task.Factory.FromAsync<string, IPAddress[]>(Dns.BeginGetHostAddresses, Dns.EndGetHostAddresses, host, null))
                .ToList();
            var addressesIPv4 = allAddresses.Where(x => x.AddressFamily == AddressFamily.InterNetwork)
                .ToList();
            IPAddress addr;
            if (supportLevel == IPv4SupportLevel.RequiresIPv4)
            {
                if (addressesIPv4.Count != 0)
                    addr = addressesIPv4[_addressRng.Next(0, addressesIPv4.Count)];
                else
                    addr = null;
            }
            else
            {
                var addressesIPv6 = allAddresses.Where(x => x.AddressFamily == AddressFamily.InterNetworkV6)
                    .ToList();
                if (supportLevel == IPv4SupportLevel.NoPreference)
                    addressesIPv6.AddRange(addressesIPv4);
                if (addressesIPv6.Count != 0)
                    addr = addressesIPv6[_addressRng.Next(0, addressesIPv6.Count)];
                else
                    addr = null;
            }
            if (addr == null)
                return null;
            return addr.ToString();
#endif
        }
    }
}
