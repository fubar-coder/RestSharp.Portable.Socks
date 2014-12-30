using System.IO;

namespace RestSharp.Portable.Socks.Socks5.Messages
{
    public class ConnectRequest : Request
    {
        public SocksAddress Address { get; private set; }

        public ConnectRequest(SocksAddress address)
        {
            Address = address;
        }

        protected override void WritePayloadTo(BinaryWriter writer)
        {
            writer.Write((byte)1);
            writer.Write((byte)0);
            Address.WriteToV5(writer);
        }
    }
}
