namespace RestSharp.Portable.Socks.Socks4.Messages
{
    public abstract class Message
    {
        protected Message()
        {
            Version = (byte)SocksVersion.SocksV4.Major;
        }

        public byte Version { get; private set; }
    }
}
