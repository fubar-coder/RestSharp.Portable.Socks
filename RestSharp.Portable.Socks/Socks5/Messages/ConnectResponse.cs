using System.IO;

namespace RestSharp.Portable.Socks.Socks5.Messages
{
    public class ConnectResponse : Response
    {
        public ConnectStatus Status { get; private set; }
        public SocksAddress BoundAddress { get; private set; }

        protected override void ReadPayloadFrom(BinaryReader reader)
        {
            var reply = reader.ReadByte();
            if (reply >= 9)
                throw new InvalidDataException();
            Status = (ConnectStatus)reply;
            reader.ReadByte();  // Reserved
            BoundAddress = new SocksAddress(reader, SocksVersion.SocksV5);
        }
    }
}