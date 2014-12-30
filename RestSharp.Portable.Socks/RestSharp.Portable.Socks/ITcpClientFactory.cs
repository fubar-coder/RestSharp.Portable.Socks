using System;
using System.Collections.Generic;
using System.Text;

namespace RestSharp.Portable.Socks
{
    public interface ITcpClientFactory
    {
        ITcpClient Create(SocksAddress destinationAddress, bool useSsl);
    }
}
