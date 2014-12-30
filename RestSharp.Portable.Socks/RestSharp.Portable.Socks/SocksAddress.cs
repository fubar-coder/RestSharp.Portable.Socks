using System;
using System.IO;
using System.Net;

namespace RestSharp.Portable.Socks
{
    public class SocksAddress : IComparable<SocksAddress>, IComparable, IEquatable<SocksAddress>
    {
        private readonly byte[] _invalidSocksAddressV4 = new byte[] {0, 0, 0, 1};

        private IPEndPoint _endPoint;

        public UriHostNameType HostNameType { get; private set; }

        public string Host { get; private set; }

        public int Port { get; private set; }

        public IPEndPoint EndPoint
        {
            get
            {
                if (HostNameType != UriHostNameType.IPv4 && HostNameType != UriHostNameType.IPv6)
                    throw new InvalidOperationException();
                return _endPoint;
            }
        }

        public SocksAddress(BinaryReader reader, Version version)
        {
            if (version == SocksVersion.SocksV5)
            {
                ReadFromV5(reader);
            }
            else if (version == SocksVersion.SocksV4 || version == SocksVersion.SocksV4a)
            {
                ReadFromV4(reader);
            }
        }

        public SocksAddress(Uri uri)
            : this(uri.HostNameType, uri.Host, uri.Port)
        {
        }

        public SocksAddress(string host, int port)
            : this(SocksUtilities.GetHostNameType(host), host, port)
        {
            
        }

        public SocksAddress(IPAddress address, int port)
        {
            Port = port;
            Host = address.ToString();
            _endPoint = new IPEndPoint(address, port);
            HostNameType = SocksUtilities.GetHostNameType(address);
        }

        public SocksAddress(IPEndPoint endPoint)
        {
            Port = endPoint.Port;
            Host = endPoint.Address.ToString();
            _endPoint = endPoint;
            HostNameType = SocksUtilities.GetHostNameType(endPoint.Address);
        }

        private SocksAddress(UriHostNameType hostNameType, string host, int port)
        {
            Host = host;
            Port = port;
            switch (hostNameType)
            {
                case UriHostNameType.IPv4:
                case UriHostNameType.IPv6:
                    _endPoint = new IPEndPoint(IPAddress.Parse(host), port);
                    break;
                default:
                    hostNameType = UriHostNameType.Basic;
                    break;
            }
            HostNameType = hostNameType;
        }

        public void WriteToV4(BinaryWriter writer)
        {
            var port = IPAddress.HostToNetworkOrder(unchecked((short)Port));
            writer.Write(port);
            switch (HostNameType)
            {
                case UriHostNameType.IPv4:
                    writer.Write(EndPoint.Address.GetAddressBytes());
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public void WriteToV4A(BinaryWriter writer)
        {
            var port = IPAddress.HostToNetworkOrder(unchecked((short)Port));
            writer.Write(port);
            switch (HostNameType)
            {
                case UriHostNameType.IPv4:
                    writer.Write(EndPoint.Address.GetAddressBytes());
                    break;
                case UriHostNameType.IPv6:
                    throw new NotSupportedException();
                default:
                    writer.Write(_invalidSocksAddressV4);
                    break;
            }
        }

        public void WriteToV5(BinaryWriter writer)
        {
            switch (HostNameType)
            {
                case UriHostNameType.IPv4:
                    writer.Write((byte) 1);
                    writer.Write(EndPoint.Address.GetAddressBytes());
                    break;
                case UriHostNameType.IPv6:
                    writer.Write((byte) 4);
                    writer.Write(EndPoint.Address.GetAddressBytes());
                    break;
                default:
                {
                    writer.Write((byte) 3);
                    var bytes = SocksUtilities.DefaultEncoding.GetBytes(Host);
                    writer.Write((byte) bytes.Length);
                    writer.Write(bytes);
                    break;
                }
            }
            var port = IPAddress.HostToNetworkOrder(unchecked((short) Port));
            writer.Write(port);
        }

        public void ReadFromV4(BinaryReader reader)
        {
            Port = unchecked((ushort)IPAddress.NetworkToHostOrder(reader.ReadInt16()));
            // IPv4
            HostNameType = UriHostNameType.IPv4;
            var address = new IPAddress(reader.ReadBytes(4));
            Host = address.ToString();
            _endPoint = new IPEndPoint(address, Port);
        }

        public void ReadFromV5(BinaryReader reader)
        {
            IPAddress address;
            var hostNameType = reader.ReadByte();
            switch (hostNameType)
            {
                case 1:
                    // IPv4
                    HostNameType = UriHostNameType.IPv4;
                    address = new IPAddress(reader.ReadBytes(4));
                    break;
                case 3:
                    // Host name
                    HostNameType = UriHostNameType.Basic;
                    address = null;
                    Host = SocksUtilities.DefaultEncoding.GetString(reader.ReadBytes(reader.ReadByte()));
                    break;
                case 4:
                    // IPv6
                    HostNameType = UriHostNameType.IPv6;
                    address = new IPAddress(reader.ReadBytes(16));
                    break;
                default:
                    throw new NotSupportedException();
            }
            Port = unchecked ((ushort) IPAddress.NetworkToHostOrder(reader.ReadInt16()));
            if (address == null) 
                return;
            Host = address.ToString();
            _endPoint = new IPEndPoint(address, Port);
        }

        public Uri ToUri()
        {
            return new Uri(string.Format("socks5://{0}:{1}", Host, Port));
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", Host, Port);
        }

        public int CompareTo(SocksAddress other)
        {
            return SocksAddressComparer.Default.Compare(this, other);
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo((SocksAddress) obj);
        }

        public bool Equals(SocksAddress other)
        {
            return SocksAddressComparer.Default.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return SocksAddressComparer.Default.Equals(this, (SocksAddress) obj);
        }

        public override int GetHashCode()
        {
            return SocksAddressComparer.Default.GetHashCode(this);
        }
    }
}
