using System;
using System.Collections.Generic;
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

        public ITcpClient Create(SocksAddress destinationAddress, bool useSsl)
        {
            var conn = Pool.GetOrCreateClient(destinationAddress, useSsl);
            return conn.Client;
        }
    }
}
