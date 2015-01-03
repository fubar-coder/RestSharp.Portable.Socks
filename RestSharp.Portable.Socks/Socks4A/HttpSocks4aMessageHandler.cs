using System;
using System.Net.Http;
using System.Threading;

namespace RestSharp.Portable.Socks.Socks4A
{
    public class HttpSocks4AMessageHandler : TcpClientMessageHandler
    {
        private readonly Pooling.TcpClientPool _pool;
        private Pooling.OpenConnection _connection;

        public HttpSocks4AMessageHandler(ITcpClientFactory tcpClientFactory, ISocksWebProxy proxy)
        {
            _pool = new Pooling.TcpClientPool(tcpClientFactory);
            Proxy = proxy;
        }

        internal HttpSocks4AMessageHandler(Pooling.TcpClientPool pool, ISocksWebProxy proxy)
        {
            _pool = pool;
            Proxy = proxy;
        }

        public ISocksWebProxy Proxy { get; set; }

        protected override AddressCompatibility AddressCompatibility { get { return AddressCompatibility.SupportsIPv4 | Socks.AddressCompatibility.SupportsHost; } }

        protected override ITcpClient CreateClient(HttpRequestMessage request, SocksAddress destinationAddress, bool useSsl, CancellationToken cancellationToken, bool forceRecreate)
        {
            if (Proxy == null)
                throw new InvalidOperationException("Proxy property cannot be null.");

            var proxyUri = Proxy.GetProxy(request.RequestUri);

            _connection = forceRecreate 
                ? _pool.Create(destinationAddress, useSsl) 
                : _pool.GetOrCreateClient(destinationAddress, useSsl);

            var client = new Client(new Pooling.TcpClientPoolFactory(_pool), new SocksAddress(proxyUri), destinationAddress, useSsl)
            {
                Credentials = Proxy.Credentials,
            };
            return client;
        }

        protected override void OnResponseReceived(HttpResponseMessage message)
       {
            base.OnResponseReceived(message);
            _connection.Update(message);
        }
    }
}