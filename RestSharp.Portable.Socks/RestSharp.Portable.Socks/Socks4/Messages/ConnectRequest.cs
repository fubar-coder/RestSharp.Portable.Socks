using System.IO;
using System.Text;

namespace RestSharp.Portable.Socks.Socks4.Messages
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
            Address.WriteToV4(writer);
            var data = Encoding.UTF8.GetBytes(UserId);
            writer.Write(data, 0, data.Length);
            writer.Write((byte) 0);
        }
    }
}
