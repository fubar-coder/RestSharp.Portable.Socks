using System.Net;
using System.Threading.Tasks;
using RestSharp.Portable.Socks.Socks4;
using RestSharp.Portable.Socks.Socks5;
using Xunit;
namespace RestSharp.Portable.Socks.NET45.Tests
{
    public class Socks4Tests
    {
        static RestClient CreateClientSocks4(bool useSsl)
        {
            var client = new RestClient(string.Format("http{0}://httpbin.org/cookies", useSsl ? "s" : string.Empty))
            {
                HttpClientFactory = new Socks4HttpClientFactory(new TcpClientImpl.TcpClientWrapperFactory(useSsl)),
                CookieContainer = new CookieContainer(),
                Proxy = new Socks5WebProxy(new SocksAddress("localhost", 9150)),
            };
            return client;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ExecuteHttpBinTests(bool useSsl)
        {
            await TestSocksRequests.Test(CreateClientSocks4(useSsl));
        }
    }
}