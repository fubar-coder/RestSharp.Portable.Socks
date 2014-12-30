namespace RestSharp.Portable.Socks.Socks4A.Messages
{
    public abstract class Message
    {
        protected Message()
        {
            Version = (byte)SocksVersion.SocksV4a.Major;
        }

        public byte Version { get; private set; }
    }
}
