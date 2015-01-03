namespace RestSharp.Portable.Socks.Socks5
{
    public enum ConnectStatus : byte
    {
        Succeeded,
        GeneralFailure,
        NotAllowed,
        NetworkUnreachable,
        HostUnreachable,
        ConnectionRefused,
        TTLExpired,
        CommandNotSupported,
        AddressTypeNotSupported,
    }
}