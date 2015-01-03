using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RestSharp.Portable.Socks
{
    public interface ITcpClient : IDisposable
    {
        int? Timeout { get; set; }
        int? ReadWriteTimeout { get; set; }

        Task Open(CancellationToken cancellationToken);
        void Close();

        Stream GetStream();
    }
}