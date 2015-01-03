using System.IO;

namespace RestSharp.Portable.Socks.Socks5.Messages.Authentication
{
    public class PlainAuthenticationResponse : Response
    {
        public int StatusCode { get; private set; }

        public bool IsSuccess
        {
            get { return StatusCode == 0; }
        }

        protected override void ReadPayloadFrom(BinaryReader reader)
        {
            StatusCode = reader.ReadByte();
        }
    }
}
