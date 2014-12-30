using System.IO;

namespace RestSharp.Portable.Socks.Socks4.Messages
{
    public abstract class Request : Message
    {
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Version);
            WritePayloadTo(writer);
        }

        protected abstract void WritePayloadTo(BinaryWriter writer);
    }
}