using System.IO;
using System.Net.Security;
using System.Threading.Tasks;
using RestSharp.Portable;

namespace RestSharp.Portable.Socks.NET45.Tests.TcpClientImpl
{
    public class TcpClientWrapperFactory : ITcpClientFactory
    {
        private readonly bool _ignoreCertificateErrors;

        public TcpClientWrapperFactory(bool ignoreCertificateErrors)
        {
            _ignoreCertificateErrors = ignoreCertificateErrors;
        }

        public ITcpClient Create(SocksAddress destinationAddress, bool useSsl)
        {
            return new TcpClientWrapper(destinationAddress, useSsl, this);
        }

        public async Task<Stream> CreateSslStream(Stream networkStream, string destinationHost)
        {
            SslStream sslStream;
            if (_ignoreCertificateErrors)
            {
                sslStream = new SslStream(networkStream, true, (sender, certificate, chain, errors) => true);
            }
            else
            {
                sslStream = new SslStream(networkStream, true);
            }
            await sslStream.AuthenticateAsClientAsync(destinationHost);
            return sslStream;
        }
    }
}