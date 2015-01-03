using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable.Socks.Pooling
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

        public virtual async Task<Stream> CreateSslStream(Stream networkStream, string destinationHost)
        {
            return await Pool.Factory.CreateSslStream(networkStream, destinationHost);
        }
    }
}
