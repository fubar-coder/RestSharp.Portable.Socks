using System;
using System.IO;
using System.Net;
#if WINRT
using Windows.Networking;
#endif

namespace RestSharp.Portable.Socks
{
    public class SocksAddress : IComparable<SocksAddress>, IComparable, IEquatable<SocksAddress>
    {
        private readonly byte[] _invalidSocksAddressV4 = new byte[] {0, 0, 0, 1};

        public EndPointType HostNameType { get; private set; }

        public string Host { get; private set; }

        public int Port { get; private set; }

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
            : this(uri.GetHostNameType(), uri.Host, uri.Port)
        {
        }

        public SocksAddress(string host, int port)
            : this(SocksUtilities.GetHostNameType(host), host, port)
        {
            
        }

#if WINRT
        public SocksAddress(HostName address, int port)
        {
            Port = port;
            Host = address.ToString();
            HostNameType = address.GetHostNameType();
        }
#else
        public SocksAddress(IPAddress address, int port)
        {
            Port = port;
            Host = address.ToString();
            HostNameType = SocksUtilities.GetHostNameType(address);
        }
#endif

#if !WINRT
        public SocksAddress(IPEndPoint endPoint)
        {
            Port = endPoint.Port;
            Host = endPoint.Address.ToString();
            HostNameType = SocksUtilities.GetHostNameType(endPoint.Address);
        }
#endif

        private SocksAddress(EndPointType hostNameType, string host, int port)
        {
            Host = host;
            Port = port;
            HostNameType = hostNameType;
        }

        public void WriteToV4(BinaryWriter writer)
        {
            writer.Write(this.GetPortBytes());
            switch (HostNameType)
            {
                case EndPointType.IPv4:
                    writer.Write(this.GetAddressBytes());
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public void WriteToV4A(BinaryWriter writer)
        {
            writer.Write(this.GetPortBytes());
            switch (HostNameType)
            {
                case EndPointType.IPv4:
                    writer.Write(this.GetAddressBytes());
                    break;
                case EndPointType.IPv6:
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
                case EndPointType.IPv4:
                    writer.Write((byte) 1);
                    writer.Write(this.GetAddressBytes());
                    break;
                case EndPointType.IPv6:
                    writer.Write((byte) 4);
                    writer.Write(this.GetAddressBytes());
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
            writer.Write(this.GetPortBytes());
        }

        public void ReadFromV4(BinaryReader reader)
        {
            Port = NetworkConverter.ToPort(reader.ReadBytes(2));
            // IPv4
            HostNameType = EndPointType.IPv4;
            Host = NetworkConverter.ToIPv4(reader.ReadBytes(4));
        }

        public void ReadFromV5(BinaryReader reader)
        {
            var hostNameType = reader.ReadByte();
            switch (hostNameType)
            {
                case 1:
                    // IPv4
                    HostNameType = EndPointType.IPv4;
                    Host = NetworkConverter.ToIPv4(reader.ReadBytes(4));
                    break;
                case 3:
                    // Host name
                    HostNameType = EndPointType.HostName;
                    {
                        var data = reader.ReadBytes(reader.ReadByte());
                        Host = SocksUtilities.DefaultEncoding.GetString(data, 0, data.Length);
                    }
                    break;
                case 4:
                    // IPv6
                    HostNameType = EndPointType.IPv6;
                    Host = NetworkConverter.ToIPv6(reader.ReadBytes(16));
                    break;
                default:
                    throw new NotSupportedException();
            }
            Port = NetworkConverter.ToPort(reader.ReadBytes(2));
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
