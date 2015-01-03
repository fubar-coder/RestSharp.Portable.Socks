using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RestSharp.Portable.Socks.Socks4A;
using RestSharp.Portable.Socks.Socks4;
using Xunit;
namespace RestSharp.Portable.Socks.NET45.Tests
{
    public class Socks4ATests
    {
        static RestClient CreateClientSocks4A(bool useSsl, bool useHost)
        {
            bool ignoreSslErrors = !useHost && useSsl;
            var client = new RestClient(string.Format("http{0}://httpbin.org/cookies", useSsl ? "s" : string.Empty))
            {
                HttpClientFactory = new Socks4AHttpClientFactory(new TcpClientImpl.TcpClientWrapperFactory(ignoreSslErrors))
                {
                    ResolveHost = !useHost,
                },
                CookieContainer = new CookieContainer(),
                Proxy = new Socks4WebProxy(new SocksAddress("localhost", 9150)),
            };
            return client;
        }

        [SocksProxyAvailableTheory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task ExecuteHttpBinTests(bool useSsl, bool useHost)
        {
            var client = CreateClientSocks4A(useSsl, useHost);
            await TestSocksRequests.Test(client);
        }
    }
}