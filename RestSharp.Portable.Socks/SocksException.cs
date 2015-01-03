using System;
using System.Runtime.Serialization;

namespace RestSharp.Portable.Socks
{
#if !SILVERLIGHT && !WINRT && !PCL
    [Serializable]
#endif
    public class SocksException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public SocksException()
        {
        }

        public SocksException(string message) : base(message)
        {
        }

        public SocksException(string message, Exception inner) : base(message, inner)
        {
        }

#if !SILVERLIGHT && !WINRT && !PCL
        protected SocksException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}
