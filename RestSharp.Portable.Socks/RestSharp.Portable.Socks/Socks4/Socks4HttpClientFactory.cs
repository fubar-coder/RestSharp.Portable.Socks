using System.Net.Http;
using RestSharp.Portable.HttpClientImpl;

namespace RestSharp.Portable.Socks.Socks4
{
    public class Socks4HttpClientFactory : DefaultHttpClientFactory
    {
        private readonly ITcpClientFactory _tcpClientFactory;
        private readonly TcpClientPool _pool;

        public Socks4HttpClientFactory(ITcpClientFactory tcpClientFactory)
        {
            _pool = new TcpClientPool(tcpClientFactory);
            _tcpClientFactory = tcpClientFactory;
        }

        public bool ResolveHost { get; set; }

        protected override HttpMessageHandler CreateMessageHandler(IRestClient client, IRestRequest request)
        {
            var proxy = GetProxy(client);
            var socksProxy = proxy as ISocksWebProxy;
            if (socksProxy == null)
                return base.CreateMessageHandler(client, request);

            var httpClientHandler = new HttpSocks4MessageHandler(_pool, socksProxy)
            {
                ResolveHost = ResolveHost,
            };
            var cookies = GetCookies(client, request);
            if (cookies != null)
            {
                httpClientHandler.UseCookies = true;
                httpClientHandler.CookieContainer = cookies;
            }
            return httpClientHandler;
        }
    }
}
