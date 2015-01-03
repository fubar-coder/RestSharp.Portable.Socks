using System.Net.Http;
using RestSharp.Portable.HttpClientImpl;

namespace RestSharp.Portable.Socks.Socks5
{
    public class Socks5HttpClientFactory : DefaultHttpClientFactory
    {
        private readonly ITcpClientFactory _tcpClientFactory;
        private readonly TcpClientPool _pool;

        public Socks5HttpClientFactory(ITcpClientFactory tcpClientFactory)
        {
            _pool = new TcpClientPool(tcpClientFactory);
            _tcpClientFactory = tcpClientFactory;
        }

        protected override HttpMessageHandler CreateMessageHandler(IRestClient client, IRestRequest request)
        {
            var proxy = GetProxy(client);
            var socksProxy = proxy as ISocksWebProxy;
            if (socksProxy == null)
                return base.CreateMessageHandler(client, request);

            var httpClientHandler = new HttpSocks5MessageHandler(_pool, socksProxy);
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
