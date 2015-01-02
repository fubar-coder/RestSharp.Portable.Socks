using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
#if SUPPORTS_NLOG
using NLog.Targets.Wrappers;
#endif

namespace RestSharp.Portable.Socks
{
    public abstract class TcpClientMessageHandler : HttpMessageHandler
    {
        protected TcpClientMessageHandler()
        {
            Timeout = 100000;
            ReadWriteTimeout = 300000;
            MaximumStatusLineLength = 100;
        }

        public int MaximumStatusLineLength { get; set; }

        public CookieContainer CookieContainer { get; set; }
        public bool UseCookies { get; set; }
        public int Timeout { get; set; }
        public int ReadWriteTimeout { get; set; }
        public bool ResolveHost { get; set; }

        protected abstract bool PreferIPv4 { get; }
        protected abstract ITcpClient CreateClient(HttpRequestMessage request, SocksAddress destinationAddress, bool useSsl, CancellationToken cancellationToken, bool forceRecreate);

        protected async Task<HttpResponseMessage> InternalSendAsync(HttpRequestMessage request, HttpMethod requestMethod, Uri requestUri, CancellationToken cancellationToken, bool forceRecreate)
        {
            var useSsl = string.Equals(requestUri.Scheme, "https", StringComparison.OrdinalIgnoreCase);
            var destinationAddress = new SocksAddress(requestUri);
            if (ResolveHost &&
                destinationAddress.HostNameType != EndPointType.IPv4 &&
                destinationAddress.HostNameType != EndPointType.IPv6)
            {
                var resolvedHost = await SocksUtilities.ResolveHost(destinationAddress.Host, PreferIPv4);
                if (!string.IsNullOrEmpty(resolvedHost))
                    destinationAddress = new SocksAddress(resolvedHost, destinationAddress.Port);
            }

            var client = CreateClient(request, destinationAddress, useSsl, cancellationToken, forceRecreate);
            try
            {
                client.Timeout = Timeout;
                client.ReadWriteTimeout = ReadWriteTimeout;

                await client.Open(cancellationToken);
                var stream = client.GetStream();

                await ValidateHeader(request);

                // Send request
                await WriteRequestHeader(stream, request, requestMethod, requestUri, cancellationToken);
                await WriteContent(stream, request, cancellationToken);
                await stream.FlushAsync(cancellationToken);

                if (UseCookies && CookieContainer == null)
                    CookieContainer = new CookieContainer();

                // Parse response
                var response = new TcpResponseMessage(client, request, requestUri, this);
                await response.Parse(cancellationToken);
                OnResponseReceived(response);
                return response;
            }
            catch
            {
                client.Dispose();
                throw;
            }
        }

        private async Task ValidateHeader(HttpRequestMessage request)
        {
            if (request.Method != HttpMethod.Put && request.Method != HttpMethod.Post)
                return;
            if (request.Content.Headers.ContentLength.HasValue)
                return;
            await request.Content.LoadIntoBufferAsync();
            if (request.Content.Headers.ContentLength.HasValue)
                return;
            throw new NotSupportedException("You must specify a content length when Keep-Alive is used.");
        }

        protected virtual void OnResponseReceived(HttpResponseMessage message)
        {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool secondTry;

            HttpResponseMessage response;
            try
            {
                response = await InternalSendAsync(request, request.Method, request.RequestUri, cancellationToken, false);
                secondTry = false;
            }
            catch
            {
                secondTry = true;
                response = null;
            }
            if (secondTry)
                response = await InternalSendAsync(request, request.Method, request.RequestUri, cancellationToken, true);

            switch (response.StatusCode)
            {
                case HttpStatusCode.MovedPermanently:
                    // 301
                case HttpStatusCode.Found:
                    // 302

                case HttpStatusCode.TemporaryRedirect:
                    // 307
                {
                    response.Dispose();
                    var location = new Uri(request.RequestUri, response.Headers.Location);
                    response = await InternalSendAsync(request, request.Method, location, cancellationToken, false);
                    break;
                }
                case HttpStatusCode.SeeOther:
                    // 303
                {
                    response.Dispose();
                    var location = new Uri(request.RequestUri, response.Headers.Location);
                    response = await InternalSendAsync(request, HttpMethod.Get, location, cancellationToken, false);
                    break;
                }
            }
            return response;
        }

        private async Task WriteContent(Stream stream, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content == null)
                return;
            await stream.FlushAsync(cancellationToken);
            using (var input = await request.Content.ReadAsStreamAsync())
                await input.CopyToAsync(stream, 4096, cancellationToken);
        }

        private async Task WriteRequestHeader(Stream stream, HttpRequestMessage request, HttpMethod requestMethod, Uri requestUri, CancellationToken cancellationToken)
        {
            using (var writer = new StringWriter
            {
                NewLine = "\r\n"
            })
            {
                var pathAndQuery = requestUri.GetComponents(UriComponents.PathAndQuery, UriFormat.UriEscaped);
                writer.WriteLine("{1} {2} HTTP/{0}", request.Version ?? new Version(1, 1), requestMethod.Method, pathAndQuery);
                writer.WriteLine("Host: {0}", requestUri.Host);
                foreach (var header in request.Headers.Where(x => !string.Equals(x.Key, "Host", StringComparison.OrdinalIgnoreCase)))
                    writer.WriteLine("{0}: {1}", header.Key, string.Join(",", header.Value));
                    //foreach (var headerValue in header.Value)
                    //    writer.WriteLine("{0}: {1}", header.Key, headerValue);
                if (request.Content != null)
                {
                    foreach (var header in request.Content.Headers)
                        writer.WriteLine("{0}: {1}", header.Key, string.Join(",", header.Value));
                }
                if (UseCookies && CookieContainer != null)
                {
                    var cookieHeader = CookieContainer.GetCookieHeader(request.RequestUri);
                    if (!string.IsNullOrEmpty(cookieHeader))
                        writer.WriteLine("Cookie: {0}", cookieHeader);
                }
                writer.WriteLine();

                var encoding = new UTF8Encoding(false);
                var data = encoding.GetBytes(writer.ToString());
                await stream.WriteAsync(data, 0, data.Length, cancellationToken);
            }
        }
   }
}