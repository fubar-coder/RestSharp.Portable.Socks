using System.Net.Http;
using RestSharp.Portable.HttpClientImpl;

namespace RestSharp.Portable.Socks.Socks4A
{
    public class Socks4AHttpClientFactory : DefaultHttpClientFactory
    {
        private readonly ITcpClientFactory _tcpClientFactory;
        private readonly Pooling.TcpClientPool _pool;

        public Socks4AHttpClientFactory(ITcpClientFactory tcpClientFactory)
        {
            _pool = new Pooling.TcpClientPool(tcpClientFactory);
            _tcpClientFactory = tcpClientFactory;
        }

        protected override HttpMessageHandler CreateMessageHandler(IRestClient client, IRestRequest request)
        {
            var proxy = GetProxy(client);
            var socksProxy = proxy as ISocksWebProxy;
            if (socksProxy == null)
                return base.CreateMessageHandler(client, request);

            var httpClientHandler = new HttpSocks4AMessageHandler(_pool, socksProxy);
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
