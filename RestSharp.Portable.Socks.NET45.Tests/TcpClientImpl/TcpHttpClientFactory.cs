using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RestSharp.Portable;
using RestSharp.Portable.HttpClientImpl;

namespace RestSharp.Portable.Socks.NET45.Tests.TcpClientImpl
{
    public class TcpHttpClientFactory : DefaultHttpClientFactory
    {
        private readonly Pooling.TcpClientPool _pool;

        public bool ResolveHost { get; set; }

        public TcpHttpClientFactory(bool ignoreCertificateErrors)
        {
            _pool = new Pooling.TcpClientPool(new TcpClientWrapperFactory(ignoreCertificateErrors));
        }

        protected override HttpMessageHandler CreateMessageHandler(IRestClient client, IRestRequest request)
        {
            var httpClientHandler = new TcpClientWrapperMessageHandler(_pool);
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
