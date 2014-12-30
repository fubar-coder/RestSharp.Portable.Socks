using System.IO;

namespace RestSharp.Portable.Socks.Socks4.Messages
{
    public abstract class Response : Message
    {
        protected abstract void ReadPayloadFrom(BinaryReader reader);

        public void ReadFrom(BinaryReader reader)
        {
            var version = reader.ReadByte();
            if (version != 0)
                throw new InvalidDataException();
            ReadPayloadFrom(reader);
        }
    }
}