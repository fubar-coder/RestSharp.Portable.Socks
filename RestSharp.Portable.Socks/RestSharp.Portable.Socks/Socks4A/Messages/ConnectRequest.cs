using System;
using System.IO;
using System.Text;

namespace RestSharp.Portable.Socks.Socks4A.Messages
{
    public class ConnectRequest : Request
    {
        public SocksAddress Address { get; private set; }
        public string UserId { get; private set; }

        public ConnectRequest(SocksAddress address, string userId)
        {
            UserId = userId;
            Address = address;
        }

        protected override void WritePayloadTo(BinaryWriter writer)
        {
            writer.Write((byte)1);
            Address.WriteToV4A(writer);
            var data = Encoding.UTF8.GetBytes(UserId);
            writer.Write(data, 0, data.Length);
            writer.Write((byte) 0);
            switch (Address.HostNameType)
            {
                case UriHostNameType.IPv4:
                case UriHostNameType.IPv6:
                    break;
                default:
                    data = Encoding.UTF8.GetBytes(Address.Host);
                    writer.Write(data, 0, data.Length);
                    writer.Write((byte)0);
                    break;
            }
        }
    }
}
