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
    public static class SocksUtilities
    {
        private static readonly Random _addressRng = new Random();
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

        public static byte[] GetIpAddressBytes(this SocksAddress socksAddress)
        {
            if (socksAddress.HostNameType == EndPointType.HostName)
                throw new NotSupportedException();
#if WINRT
            switch (socksAddress.HostNameType)
            {
                case EndPointType.IPv4:
                    return GetBytesForIPv4(socksAddress.Host);
                case EndPointType.IPv6:
                    return GetBytesForIPv6(socksAddress.Host);
            }
            throw new NotSupportedException();
#else
            var address = IPAddress.Parse(socksAddress.Host);
            return address.GetAddressBytes();
#endif
        }

        private static ushort[] GetWordsForIPv6(string host)
        {
            var data = new ushort[8];
            host = host.Replace(" ", string.Empty);
            if (host.StartsWith("::ffff:"))
            {
                data[5] = 0xFFFF;
                var ipv4 = host.Substring(7);
                if (ipv4.IndexOf(':') != -1)
                {
                    var parts = ipv4.Split(':')
                        .Select(x => ushort.Parse(x, System.Globalization.NumberStyles.HexNumber))
                        .ToArray();
                    data[6] = parts[0];
                    data[7] = parts[1];
                }
                else
                {
                    var d = GetBytesForIPv4(ipv4);
                    data[6] = (ushort)((d[3] << 8) + d[2]);
                    data[7] = (ushort)((d[1] << 8) + d[0]);
                }
            }
            else
            {
                var parts = host.Split(':')
                    .Select(x => string.IsNullOrWhiteSpace(x) ? -1 : int.Parse(x, System.Globalization.NumberStyles.HexNumber))
                    .ToArray();
                var prefixSize = Array.IndexOf(parts, -1);
                if (prefixSize == -1)
                {
                    if (parts.Length != 8)
                        throw new ArgumentOutOfRangeException();
                    data = parts.Select(x => (ushort)x).ToArray();
                }
                else
                {
                    var nonEmptyIndex = prefixSize;
                    while (nonEmptyIndex < (parts.Length - 1) && parts[nonEmptyIndex + 1] != -1)
                        nonEmptyIndex += 1;
                    var suffixSize = parts.Length - prefixSize - 1;
                    for (var i = 0; i != prefixSize; ++i)
                        data[i] = (ushort)parts[i];
                    var suffixIndexSrc = parts.Length - suffixSize;
                    var suffixIndexDst = data.Length - suffixSize;
                    for (var i = 0; i != suffixSize; ++i)
                        data[suffixIndexDst++] = (ushort)parts[suffixIndexSrc++];
                }
            }
            return data;
        }

        private static byte[] GetBytesForIPv6(string host)
        {
            var result = new byte[16];
            var idxDst = 0;
            var words = GetWordsForIPv6(host);
            for (var idxSrc = 0; idxSrc != words.Length; ++idxSrc)
            {
                var v = words[idxSrc];
                result[idxDst++] = (byte) ((v >> 8) & 0xFF);
                result[idxDst++] = (byte) (v & 0xFF);
            }
            return result;
        }

        private static byte[] GetBytesForIPv4(string host)
        {
            var result = host.Split('.').Select(x => byte.Parse(host))
                .Reverse()
                .ToArray();
            return result;
        }

        internal static string GetIPv4ForBytes(byte[] data)
        {
            return string.Join(".", data.Reverse().Select(x => x.ToString()));
        }

        internal static string GetIPv6ForBytes(byte[] data)
        {
            var words = new ushort[8];
            var idxDst = 0;
            for (var idxSrc = 0; idxSrc != data.Length; idxSrc += 2)
                words[idxDst++] = (ushort)((data[idxSrc] << 8) + data[idxSrc + 1]);
            return GetIPv6ForWords(words);
        }

        internal static string GetIPv6ForWords(ushort[] data)
        {
            var zeroRanges = new List<Tuple<int, int>>();
            var startIndex = -1;
            var indexCount = 0;
            for (var i = 0; i != 8; ++i)
            {
                var v = data[i];
                if (v == 0)
                {
                    if (startIndex == -1)
                    {
                        startIndex = i;
                        indexCount = 1;
                    }
                    else
                        indexCount += 1;
                }
                else if (v != 0 && startIndex != -1)
                {
                    zeroRanges.Add(Tuple.Create(startIndex, indexCount));
                    startIndex = -1;
                }
            }
            if (startIndex != -1)
                zeroRanges.Add(Tuple.Create(startIndex, indexCount));

            if (zeroRanges.Count != 0)
            {
                var largestRange = zeroRanges.OrderByDescending(x => x.Item2).First();
                startIndex = largestRange.Item1;
                indexCount = largestRange.Item2;
            }

            ushort[] wordsPrefix, wordsSuffix;
            if (startIndex != -1)
            {
                wordsPrefix = data.Take(startIndex).ToArray();
                wordsSuffix = data.Skip(startIndex + indexCount).ToArray();
            }
            else
            {
                wordsPrefix = data;
                wordsSuffix = null;
            }

            var result = new StringBuilder();
            if (wordsPrefix.Length != 0)
                result.Append(string.Join(":", wordsPrefix.Select(x => x.ToString("x"))));
            if (wordsSuffix != null)
                result
                    .Append("::")
                    .Append(string.Join(":", wordsSuffix.Select(x => x.ToString("x"))));
            return result.ToString();
        }

        private static bool IsLoopBackIPv4(string host)
        {
            var data = GetBytesForIPv4(host);
            return data[0] == 1 && data[3] == 127;
        }

        private static bool IsLoopBackIPv6(ushort[] data)
        {
            if (data.Take(5).Any(x => x != 0))
                return false;
            if (data[5] == 0)
                return data[6] == 0 && data[7] == 1;
            if (data[5] != 0xFFFF)
                return false;
            return data[6] == 0x7F00 && data[7] == 1;
        }

        private static bool IsLoopBackIPv6(string host)
        {
            var data = GetWordsForIPv6(host);
            return IsLoopBackIPv6(data);
        }

#if WINRT
        private static bool IsLoopBack(string host, HostNameType type)
        {
            switch (type)
            {
                case HostNameType.Ipv4:
                    return IsLoopBackIPv4(host);
                case HostNameType.Ipv6:
                    return IsLoopBackIPv6(host);
            }
            throw new NotSupportedException();
        }
#endif

        internal static bool IsLoopBack(string host)
        {
#if WINRT
            var allAddressesTask = DatagramSocket.GetEndpointPairsAsync(new HostName(host), "0").AsTask();
            allAddressesTask.Wait();
            var allAddresses = allAddressesTask.Result
                .Where(x => x != null && x.RemoteHostName != null && (x.RemoteHostName.Type == HostNameType.Ipv4 || x.RemoteHostName.Type == HostNameType.Ipv6))
                .Select(x => x.RemoteHostName)
                .ToList();
            return allAddresses.Any(x => IsLoopBack(x.CanonicalName, x.Type));
#elif SILVERLIGHT
            return false;
#else
            return Dns.GetHostAddresses(host).Any(IPAddress.IsLoopback);
#endif
        }

        internal static async Task<string> ResolveHost(string host, bool preferIPv4)
        {
#if WINRT
            var allAddresses = (await DatagramSocket.GetEndpointPairsAsync(new HostName(host), "0"))
                .Where(x => x != null && x.RemoteHostName != null)
                .Select(x => x.RemoteHostName)
                .ToList();
            var addressesIPv4 = allAddresses.Where(x => x.Type == HostNameType.Ipv4)
                .ToList();
            HostName addr;
            if (preferIPv4 && addressesIPv4.Count != 0)
            {
                addr = addressesIPv4[_addressRng.Next(0, addressesIPv4.Count)];
            }
            else
            {
                var addressesIPv6 = allAddresses.Where(x => x.Type == HostNameType.Ipv6)
                    .ToList();
                if (!preferIPv4)
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
            if (preferIPv4 && addressesIPv4.Count != 0)
            {
                addr = addressesIPv4[_addressRng.Next(0, addressesIPv4.Count)];
            }
            else
            {
                var addressesIPv6 = allAddresses.Where(x => x.AddressFamily == AddressFamily.InterNetworkV6)
                    .ToList();
                if (!preferIPv4)
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
