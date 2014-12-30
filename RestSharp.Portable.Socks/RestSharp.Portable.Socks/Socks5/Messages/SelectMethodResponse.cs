using System.IO;

namespace RestSharp.Portable.Socks.Socks5.Messages
{
    public class SelectMethodResponse : Response
    {
        public byte Method { get; set; }

        protected override void ReadPayloadFrom(BinaryReader reader)
        {
            Method = reader.ReadByte();
        }
    }
}
