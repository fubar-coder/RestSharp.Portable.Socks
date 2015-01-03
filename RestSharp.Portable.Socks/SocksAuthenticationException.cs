using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RestSharp.Portable.Socks
{
#if !SILVERLIGHT && !WINRT && !PCL
    [Serializable]
#endif
    public class SocksAuthenticationException : SocksException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public SocksAuthenticationException()
        {
        }

        public SocksAuthenticationException(string message) : base(message)
        {
        }

        public SocksAuthenticationException(string message, Exception inner) : base(message, inner)
        {
        }

#if !SILVERLIGHT && !WINRT && !PCL
        protected SocksAuthenticationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}
