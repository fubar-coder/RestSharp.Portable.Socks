using System;
using System.Net.Http;
using System.Threading;

namespace RestSharp.Portable.Socks.Socks4
{
    public class HttpSocks4MessageHandler : TcpClientMessageHandler
    { 
        private readonly TcpClientPool _pool;
        private OpenConnection _connection;

        public HttpSocks4MessageHandler(ITcpClientFactory tcpClientFactory, ISocksWebProxy proxy)
        {
            _pool = new TcpClientPool(tcpClientFactory);
            Proxy = proxy;
        }

        internal HttpSocks4MessageHandler(TcpClientPool pool, ISocksWebProxy proxy)
        {
            _pool = pool;
            Proxy = proxy;
        }

        public ISocksWebProxy Proxy { get; set; }

        protected override ITcpClient CreateClient(HttpRequestMessage request, SocksAddress destinationAddress, bool useSsl, CancellationToken cancellationToken, bool forceRecreate)
        {
            if (Proxy == null)
                throw new InvalidOperationException("Proxy property cannot be null.");

            var proxyUri = Proxy.GetProxy(request.RequestUri);

            _connection = forceRecreate 
                ? _pool.Create(destinationAddress, useSsl) 
                : _pool.GetOrCreateClient(destinationAddress, useSsl);

            var client = new Client(new TcpClientPoolFactory(_pool), new SocksAddress(proxyUri), ResolveHost, destinationAddress, useSsl)
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