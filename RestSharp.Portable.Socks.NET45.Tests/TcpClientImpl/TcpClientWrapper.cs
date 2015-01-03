using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RestSharp.Portable;

namespace RestSharp.Portable.Socks.NET45.Tests.TcpClientImpl
{
    public class TcpClientWrapper : ITcpClient
    {
        private TcpClient _tcpClient;
        private Stream _networkStream;
        private readonly SocksAddress _destinationAddress;
        private readonly bool _useSsl;
        private readonly ITcpClientFactory _factory;

        public TcpClientWrapper(SocksAddress destinationAddress, bool useSsl, ITcpClientFactory factory)
        {
            _destinationAddress = destinationAddress;
            _useSsl = useSsl;
            _factory = factory;
        }

        public void Dispose()
        {
            Close();
        }

        public int? Timeout { get; set; }

        public int? ReadWriteTimeout { get; set; }

        public async Task Open(CancellationToken cancellationToken)
        {
            _tcpClient = new TcpClient();
            if (ReadWriteTimeout.HasValue)
            {
                _tcpClient.ReceiveTimeout = ReadWriteTimeout.Value;
                _tcpClient.SendTimeout = ReadWriteTimeout.Value;
            }
            var connectTask = _tcpClient.ConnectAsync(_destinationAddress.Host, _destinationAddress.Port);
            if (Timeout.HasValue)
                connectTask = connectTask.HandleCancellation(new CancellationTokenSource(Timeout.Value).Token);
            await connectTask;
            _networkStream = _tcpClient.GetStream();
            if (_useSsl)
                _networkStream = await _factory.CreateSslStream(_networkStream, _destinationAddress.Host);
        }

        public void Close()
        {
            if (_tcpClient == null)
                return;

            _networkStream.Close();
            _networkStream = null;
            _tcpClient.Close();
            _tcpClient = null;
        }

        public Stream GetStream()
        {
            return _networkStream;
        }
    }
}