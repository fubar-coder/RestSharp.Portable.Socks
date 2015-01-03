using System.IO;

namespace RestSharp.Portable.Socks.Socks5.Messages
{
    public abstract class Response : Message
    {
        protected abstract void ReadPayloadFrom(BinaryReader reader);

        public void ReadFrom(BinaryReader reader)
        {
            var version = reader.ReadByte();
            if (version != Version)
                throw new InvalidDataException();
            ReadPayloadFrom(reader);
        }
    }
}