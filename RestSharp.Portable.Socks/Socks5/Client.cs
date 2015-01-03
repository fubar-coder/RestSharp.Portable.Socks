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
using RestSharp.Portable.Socks.Socks5.Messages;
using RestSharp.Portable.Socks.Socks5.Messages.Authentication;

namespace RestSharp.Portable.Socks.Socks5
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
                
                // Authenticate with proxy server
                await Authenticate(ct);

                // Open connection to destination address through proxy server
                await Connect(_destinationAddress, ct);

                // Do we need SSL?
                _networkStream = _client.GetStream();
                if (_useSsl)
                    _networkStream = await _tcpClientFactory.CreateSslStream(_networkStream, _destinationAddress.Host);
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

        private async Task<SocksAddress> Connect(SocksAddress destinationAddress, CancellationToken ct)
        {
            var response = await Execute<ConnectResponse>(new ConnectRequest(destinationAddress), ct);
            if (response.Status != ConnectStatus.Succeeded)
                throw new Socks5ConnectException(response.Status);
            return response.BoundAddress;
        }

        private async Task Authenticate(CancellationToken ct)
        {
            NetworkCredential socksCredentials;
            var supportedMethods = new List<AuthenticationMethod>();
            if (Credentials != null)
            {
                socksCredentials =
                    Credentials.GetCredential(new UriBuilder("socks5", SocksAddress.Host, SocksAddress.Port).Uri,
                        "Plain");
                if (socksCredentials != null)
                {
                    supportedMethods.Add(AuthenticationMethod.UsernamePassword);
                }
            }
            else
            {
                socksCredentials = null;
            }
            supportedMethods.Add(AuthenticationMethod.None);
            var response = await Execute<SelectMethodResponse>(new SelectMethodRequest(supportedMethods.Cast<byte>()), ct);
            var authenticationMethod = response.Method;
            if (!Enum.GetValues(typeof(AuthenticationMethod)).Cast<byte>().Contains(authenticationMethod))
                throw new NotSupportedException();
            var method = (AuthenticationMethod)authenticationMethod;
            switch (method)
            {
                case AuthenticationMethod.UsernamePassword:
                {
                    Debug.Assert(socksCredentials != null, "socksCredentials != null");
                    var authResponse = await Execute<PlainAuthenticationResponse>
                        (new PlainAuthenticationRequest(socksCredentials.UserName, socksCredentials.Password), ct);
                    if (!authResponse.IsSuccess)
                        throw new SocksAuthenticationException();
                    break;
                }
                case AuthenticationMethod.None:
                    break;
                default:
                    throw new NotSupportedException("No supported authentication method.");
            }
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