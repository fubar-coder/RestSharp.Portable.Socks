using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Schema;

namespace RestSharp.Portable.Socks.Pooling
{
    public class TcpClientPool
    {
        private readonly Dictionary<OpenConnectionKey, OpenConnection> _connectionPool = new Dictionary<OpenConnectionKey, OpenConnection>(OpenConnectionKeyComparer.Default);

        public ITcpClientFactory Factory { get; private set; }

        public TcpClientPool(ITcpClientFactory factory)
        {
            Factory = factory;
#if SUPPORTS_NLOG
            _logger.Trace("Created TcpClientPool");
#endif
        }

        public IPooledConnection Get(SocksAddress address, bool useSsl, bool forceCreate)
        {
            var conn = forceCreate
                ? Create(address, useSsl)
                : GetOrCreateClient(address, useSsl);
            return conn;
        }

        internal OpenConnection Create(SocksAddress address, bool useSsl)
        {
            var key = new OpenConnectionKey(address, useSsl);
            var client = Factory.Create(address, useSsl);
            lock (_connectionPool)
            {
                OpenConnection oldInfo;
                if (_connectionPool.TryGetValue(key, out oldInfo))
                {
                    _connectionPool.Remove(key);
                    oldInfo.Client.Dispose();
                }
                var result = new OpenConnection(address, client);
                _connectionPool.Add(key, result);
#if SUPPORTS_NLOG
                _logger.Debug("Pool: Create client (1): {0}", address);
#endif
                return result;
            }
        }

        internal OpenConnection GetOrCreateClient(SocksAddress address, bool useSsl)
        {
           return GetOrCreateClient(address, useSsl, DateTime.UtcNow);
        }

        internal OpenConnection GetOrCreateClient(SocksAddress address, bool useSsl, DateTime now)
        {
            lock (_connectionPool)
            {
                var key = new OpenConnectionKey(address, useSsl);
                OpenConnection info;
                if (_connectionPool.TryGetValue(key, out info))
                {
                    if (!info.DisposeIfInvalid(now))
                    {
#if SUPPORTS_NLOG
                        _logger.Debug("Pool: Reuse client");
#endif
                        return info;
                    }
#if SUPPORTS_NLOG
                    _logger.Debug("Dispose client");
#endif
                    _connectionPool.Remove(key);
                    info.Client.Dispose();
                }
#if SUPPORTS_NLOG
                _logger.Debug("Pool: Create client (2): {0}", address);
#endif
                info = new OpenConnection(address, Factory.Create(address, useSsl));
                _connectionPool.Add(key, info);
                return info;
            }
        }
    }
}
