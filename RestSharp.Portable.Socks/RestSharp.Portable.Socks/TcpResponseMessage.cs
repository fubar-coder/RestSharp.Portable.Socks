using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RestSharp.Portable.Socks
{
    class TcpResponseMessage : HttpResponseMessage
    {
        private static readonly Regex _statusLineRegex = new Regex(@"^HTTP/(?<version>\d.\d)\s(?<code>\d{3})(\s(?<reason>.*)?)?$", RegexOptions.IgnoreCase);

        private readonly TcpClientMessageHandler _handler;
        private readonly ITcpClient _client;
        private bool _disposed;
        private readonly Uri _requestUri;

        public TcpResponseMessage(ITcpClient client, HttpRequestMessage request, Uri requestUri, TcpClientMessageHandler handler)
        {
            _requestUri = requestUri;
            _handler = handler;
            _client = client;
            RequestMessage = request;
        }

        private static bool IsBufferFull(ICollection<byte> buffer, int? maxLength)
        {
            if (!maxLength.HasValue)
                return false;
            return buffer.Count >= maxLength.Value;
        }

        private static async Task<string> ReadLine(Stream stream, CancellationToken cancellationToken)
        {
            var info = await ReadBuffer(stream, null, cancellationToken);
            return Encoding.UTF8.GetString(info.Item1, 0, info.Item2);
        }

        private static async Task<Tuple<byte[], int, bool>> ReadBuffer(Stream stream, int? maxLength, CancellationToken cancellationToken)
        {
            var buffer = new List<byte>(maxLength ?? 100);
            var bufferLength = 0;
            var eolFound = false;

            var tmp = new byte[1];
            while (!IsBufferFull(buffer, maxLength) && (await stream.ReadAsync(tmp, 0, 1, cancellationToken)) != 0)
            {
                var b = tmp[0];
                buffer.Add(b);
                if (b == 10)
                {
                    eolFound = true;
                    break;
                }
                if (b == 13)
                    continue;
                bufferLength = buffer.Count;
            }

            return new Tuple<byte[], int, bool>(buffer.ToArray(), bufferLength, eolFound);
        }

        public async Task Parse(CancellationToken cancellationToken)
        {
            var stream = _client.GetStream();
            var statusLineData = await ReadBuffer(stream, _handler.MaximumStatusLineLength, cancellationToken);

            Match statusLineMatch;
            if (!statusLineData.Item3)
            {
                // Simple response
                statusLineMatch = Match.Empty;
            }
            else
            {
                try
                {
                    // Is it a status line?
                    var line = Encoding.UTF8.GetString(statusLineData.Item1, 0, statusLineData.Item2);
                    statusLineMatch = _statusLineRegex.Match(line);
                }
                catch
                {
                    // Decoding failed -> Simple response
                    statusLineMatch = Match.Empty;
                }
            }

            if (!statusLineMatch.Success)
            {
                // Simple response
                var data = statusLineData.Item1;
                Content = new StreamContent(new TcpClientStream(data, stream, null));
                Version = new Version(0, 9);
            }
            else
            {
                // Full response
                StatusCode = (HttpStatusCode)int.Parse(statusLineMatch.Groups["code"].Value);
                Version = Version.Parse(statusLineMatch.Groups["version"].Value);
                var reasonGroup = statusLineMatch.Groups["reason"];
                if (reasonGroup.Success && !string.IsNullOrEmpty(reasonGroup.Value))
                    ReasonPhrase = reasonGroup.Value;

                var headers = await ReadHeaders(stream, cancellationToken);
                var contentLength = GetContentLength(headers.GetValues("Content-Length"));
                Content = new StreamContent(new TcpClientStream(new byte[0], stream, contentLength));
                SetHeaders(headers);
            }
        }

        private static long? GetContentLength(string[] values)
        {
            if (values == null || values.Length == 0)
                return null;
            long v;
            if (!long.TryParse(values[0], out v))
                return null;
            return v;
        }

        private delegate void HttpHeaderDelegate(string[] values, TcpResponseMessage response, TcpClientMessageHandler handler);

        private static readonly Dictionary<string, HttpHeaderDelegate> _httpHeaderActions =
            new Dictionary<string, HttpHeaderDelegate>(StringComparer.OrdinalIgnoreCase)
                {
                    {
                        "Content-Length", (values, response, handler) =>
                        {
                            var v = GetContentLength(values);
                            if (v == null)
                                return;
                            response.Content.Headers.ContentLength = v.Value;
                        }
                    },
                    { "Set-Cookie", SetCookies },
                    { "Set-Cookie2", SetCookies },
                };

        private static void SetCookies(IEnumerable<string> values, TcpResponseMessage response,
            TcpClientMessageHandler handler)
        {
            if (!response._handler.UseCookies)
                return;
            foreach (var value in values)
            {
                response._handler.CookieContainer.SetCookies(response._requestUri, value);
            }
        }

        private void SetHeaders(WebHeaderCollection headers)
        {
            for (int i = 0; i != headers.Count; ++i)
            {
                var key = headers.GetKey(i);
                var values = headers.GetValues(i);
                HttpHeaderDelegate httpHeaderDelegate;
                if (_httpHeaderActions.TryGetValue(key, out httpHeaderDelegate))
                {
                    httpHeaderDelegate(values, this, _handler);
                }
                else
                {
                    if (!Headers.TryAddWithoutValidation(key, values))
                        Content.Headers.TryAddWithoutValidation(key, values);
                }

            }
        }

        private static KeyValuePair<string, string> GetKeyValue(IEnumerable<string> lines)
        {
            var entry = string.Join("\r\n", lines);
            var idx = entry.IndexOf(':');
            var key = entry.Substring(0, idx).TrimEnd();
            var value = entry.Substring(idx + 1);
            return new KeyValuePair<string, string>(key, value);
        }

        private async Task<WebHeaderCollection> ReadHeaders(Stream stream, CancellationToken cancellationToken)
        {
            var result = new WebHeaderCollection();
            var header = new List<string>();
            string line;
            while (!string.IsNullOrEmpty(line = await ReadLine(stream, cancellationToken)))
            {
                if (line.StartsWith(" ") || line.StartsWith("\t"))
                {
                    header.Add(line);
                }
                else if (header.Count != 0)
                {
                    var kv = GetKeyValue(header);
                    result.Add(kv.Key, kv.Value);

                    header.Clear();
                    header.Add(line);
                }
                else
                {
                    header.Add(line);
                }
            }
            if (header.Count != 0)
            {
                var kv = GetKeyValue(header);
                result.Add(kv.Key, kv.Value);
            }
            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                _client.Close();
                _client.Dispose();
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}
