using System.Net.Http;
using System.Threading;

namespace RestSharp.Portable.Socks.NET45.Tests.TcpClientImpl
{
    class TcpClientWrapperMessageHandler : TcpClientMessageHandler
    {
        private readonly Pooling.TcpClientPool _pool;
        private Pooling.IPooledConnection _connection;

        public TcpClientWrapperMessageHandler(ITcpClientFactory factory)
            : this(new Pooling.TcpClientPool(factory))
        {
        }

        internal TcpClientWrapperMessageHandler(Pooling.TcpClientPool pool)
        {
            _pool = pool;
        }

        protected override AddressCompatibility AddressCompatibility
        {
            get
            {
                return AddressCompatibility.SupportsHost | AddressCompatibility.SupportsIPv4 |
                       AddressCompatibility.SupportsIPv6;
            }
        }

        protected override ITcpClient CreateClient(HttpRequestMessage request, SocksAddress destinationAddress, bool useSsl, CancellationToken cancellationToken, bool forceRecreate)
        {
            _connection = _pool.Get(destinationAddress, useSsl, forceRecreate);
            var client = _connection.Client;
            return client;
        }

        protected override void OnResponseReceived(HttpResponseMessage message)
        {
            base.OnResponseReceived(message);
            _connection.Update(message);
        }
    }
}