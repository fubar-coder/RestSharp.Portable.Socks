using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RestSharp.Portable.Socks
{
    public interface ITcpClientFactory
    {
        ITcpClient Create(SocksAddress destinationAddress, bool useSsl);
        Task<Stream> CreateSslStream(Stream networkStream, string destinationHost);
    }
}
