using System;
using System.Collections.Generic;
using System.IO;
#if SUPPORTS_SSLSTREAM
using System.Net.Security;
#endif
using System.Text;

namespace RestSharp.Portable.Socks
{
    class TcpClientPoolFactory : ITcpClientFactory
    {
        public TcpClientPool Pool { get; private set; }

        public TcpClientPoolFactory(TcpClientPool pool)
        {
            Pool = pool;
        }

        public virtual ITcpClient Create(SocksAddress destinationAddress, bool useSsl)
        {
            var conn = Pool.GetOrCreateClient(destinationAddress, useSsl);
            return conn.Client;
        }

        public virtual Stream CreateSslStream(Stream networkStream, string destinationHost)
        {
#if SUPPORTS_SSLSTREAM
            var sslStream = new SslStream(networkStream, true);
            sslStream.AuthenticateAsClient(destinationHost);
            return sslStream;
#else
            throw new NotSupportedException();
#endif
        }
    }
}
