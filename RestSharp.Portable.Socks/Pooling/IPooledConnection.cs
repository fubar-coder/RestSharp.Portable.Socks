using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace RestSharp.Portable.Socks.Pooling
{
    public interface IPooledConnection
    {
        ITcpClient Client { get; }

        void Update(HttpResponseMessage message);
        void Update(HttpResponseMessage message, DateTime now);
    }
}
