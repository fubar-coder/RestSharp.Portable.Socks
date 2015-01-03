using System;
using System.Runtime.Serialization;

namespace RestSharp.Portable.Socks.Socks5
{
#if !SILVERLIGHT && !WINRT && !PCL
    [Serializable]
#endif
    public class Socks5ConnectException : SocksException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public Socks5ConnectException(ConnectStatus status)
        {
            Status = status;
        }

        public Socks5ConnectException(ConnectStatus status, string message) : base(message)
        {
            Status = status;
        }

        public Socks5ConnectException(ConnectStatus status, string message, Exception inner) : base(message, inner)
        {
            Status = status;
        }

#if !SILVERLIGHT && !WINRT && !PCL
        protected Socks5ConnectException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            Status = (ConnectStatus) info.GetByte("Status");
        }
#endif

        public ConnectStatus Status { get; private set; }
    }
}
