namespace RestSharp.Portable.Socks.Socks5.Messages
{
    public abstract class Message
    {
        protected Message()
        {
            Version = (byte)SocksVersion.SocksV5.Major;
        }

        public byte Version { get; private set; }
    }
}
