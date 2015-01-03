using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RestSharp.Portable.Socks.Socks5;
using Xunit;
namespace RestSharp.Portable.Socks.NET45.Tests
{
    public class Socks5Tests
    {
        static RestClient CreateClientSocks5(bool useSsl, bool useHost)
        {
            bool ignoreSslErrors = !useHost && useSsl;
            var client = new RestClient(string.Format("http{0}://httpbin.org/cookies", useSsl ? "s" : string.Empty))
            {
                HttpClientFactory = new Socks5HttpClientFactory(new TcpClientImpl.TcpClientWrapperFactory(ignoreSslErrors))
                {
                    ResolveHost = !useHost,
                },
                CookieContainer = new CookieContainer(),
                Proxy = new Socks5WebProxy(new SocksAddress("localhost", 9150)),
            };
            return client;
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task ExecuteHttpBinTests(bool useSsl, bool useHost)
        {
            var client = CreateClientSocks5(useSsl, useHost);
            await TestSocksRequests.Test(client);
        }
    }
}