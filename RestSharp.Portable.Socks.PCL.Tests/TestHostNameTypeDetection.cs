using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace RestSharp.Portable.Socks.PCL.Tests
{
    public class TestHostNameTypeDetection
    {
        [Theory]
        [InlineData("127.0.0.1", EndPointType.IPv4)]
        [InlineData(" 127 . 0 . 0 . 1 ", EndPointType.IPv4)]
        [InlineData("httpbin.org", EndPointType.HostName)]
        [InlineData(" 127 . 0 . 0 . 1 ", EndPointType.IPv4)]
        [InlineData("::1", EndPointType.IPv6)]
        public void TestHostNameTypeIPv4(string ipAddress, EndPointType epType)
        {
            Assert.Equal(epType, SocksUtilities.GetHostNameType(ipAddress));
        }
    }
}
