namespace RestSharp.Portable.Socks.Socks4
{
    public enum ConnectStatus : byte
    {
        Granted = 90,
        RejectedOrFailed,
        FailedToConnectToItentD,
        FaildDueToDifferentUserIds,
    }
}