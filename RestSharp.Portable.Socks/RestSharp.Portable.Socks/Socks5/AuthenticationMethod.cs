namespace RestSharp.Portable.Socks.Socks5
{
    public enum AuthenticationMethod : byte
    {
        None = 0,
        GSSAPI = 1,
        UsernamePassword = 2,
        Invalid = 255,
    }
}