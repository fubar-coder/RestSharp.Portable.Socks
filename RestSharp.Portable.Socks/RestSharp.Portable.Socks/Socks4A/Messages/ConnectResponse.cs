using System.IO;

namespace RestSharp.Portable.Socks.Socks4A.Messages
{
    public class ConnectResponse : Response
    {
        public Socks4.ConnectStatus Status { get; private set; }
        public SocksAddress IgnoredAddress { get; private set; }

        protected override void ReadPayloadFrom(BinaryReader reader)
        {
            var reply = reader.ReadByte();
            if (reply < 90 || reply > 93)
                throw new InvalidDataException();
            Status = (Socks4.ConnectStatus)reply;
            IgnoredAddress = new SocksAddress(reader, SocksVersion.SocksV4a);
        }
    }
}