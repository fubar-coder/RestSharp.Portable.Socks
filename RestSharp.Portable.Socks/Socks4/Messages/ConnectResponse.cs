using System.IO;

namespace RestSharp.Portable.Socks.Socks4.Messages
{
    public class ConnectResponse : Response
    {
        public ConnectStatus Status { get; private set; }
        public SocksAddress IgnoredAddress { get; private set; }

        protected override void ReadPayloadFrom(BinaryReader reader)
        {
            var reply = reader.ReadByte();
            if (reply < 90 || reply > 93)
                throw new InvalidDataException();
            Status = (ConnectStatus)reply;
            IgnoredAddress = new SocksAddress(reader, SocksVersion.SocksV4);
        }
    }
}