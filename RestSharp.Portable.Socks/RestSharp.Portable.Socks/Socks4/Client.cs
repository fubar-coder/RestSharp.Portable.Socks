using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
#if !WINRT && !PCL
using System.Net.Sockets;
#endif
using System.Threading;
using System.Threading.Tasks;
using RestSharp.Portable.Socks.Socks4.Messages;
#if SUPPORTS_SSLSTREAM
using System.Net.Security;
#endif

namespace RestSharp.Portable.Socks.Socks4
{
    public class Client : ITcpClient
    {
        private readonly SocksAddress _address;
        private ITcpClient _client;

        public ICredentials Credentials { get; set; }
        public SocksAddress SocksAddress { get { return _address; } }
        public int? Timeout { get; set; }
        public int? ReadWriteTimeout { get; set; }
        private Stream _networkStream;
        private readonly ITcpClientFactory _tcpClientFactory;
        private readonly SocksAddress _destinationAddress;
        private readonly bool _useSsl;

        public Client(ITcpClientFactory tcpClientFactory, SocksAddress address, SocksAddress destinationAddress, bool useSsl)
        {
            _destinationAddress = destinationAddress;
            _useSsl = useSsl;
            _tcpClientFactory = tcpClientFactory;
            _address = address;
        }

        public async Task Open(CancellationToken ct)
        {
            if (_client != null)
                throw new InvalidOperationException();
            _client = _tcpClientFactory.Create(_address, false);
            var isConnected = false;
            try
            {
                _client.ReadWriteTimeout = ReadWriteTimeout;
                _client.Timeout = Timeout;

                // Connect
                await _client.Open(ct);
                isConnected = true;
                
                // Open connection to destination address through proxy server
                await Connect(_destinationAddress, ct);

                // Do we need SSL?
                _networkStream = _client.GetStream();
                if (_useSsl)
                    _networkStream = _tcpClientFactory.CreateSslStream(_networkStream, _destinationAddress.Host);
            }
            catch
            {
                if (isConnected)
                    _client.Close();
                _client = null;
                throw;
            }
        }

        public Stream GetStream()
        {
            return _networkStream;
        }

        private async Task Connect(SocksAddress destinationAddress, CancellationToken ct)
        {
            var response = await Execute<ConnectResponse>(new ConnectRequest(destinationAddress, Guid.NewGuid().ToString()), ct);
            if (response.Status != ConnectStatus.Granted)
                throw new Socks4ConnectException(response.Status);
        }

        private static byte[] Serialize(Request request)
        {
            using (var output = new MemoryStream())
            {
                using (var writer = new BinaryWriter(output, SocksUtilities.DefaultEncoding))
                {
                    request.WriteTo(writer);
                }
                return output.ToArray();
            }
        }

        private static TResponse Deserialize<TResponse>(Stream stream)
            where TResponse : Response, new()
        {
            using (var reader = new BinaryReader(new NonDisposableStream(stream), SocksUtilities.DefaultEncoding))
            {
                var result = new TResponse();
                result.ReadFrom(reader);
                return result;
            }
        }

        internal async Task<TResponse> Execute<TResponse>(Request request, CancellationToken ct)
            where TResponse : Response, new()
        {
            var stream = _client.GetStream();
            var output = Serialize(request);
            await stream.WriteAsync(output, 0, output.Length, ct);
            return await Task.Factory.StartNew(() => Deserialize<TResponse>(stream), ct);
        }

        public void Close()
        {
            if (_client == null)
                throw new InvalidOperationException();
#if WINRT || PCL
            _networkStream.Dispose();
#else
            _networkStream.Close();
#endif
            _networkStream = null;
            _client.Close();
            _client = null;
        }

        public void Dispose()
        {
            if (_client != null)
                Close();
        }
    }
}