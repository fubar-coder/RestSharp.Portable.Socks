using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace RestSharp.Portable.Socks
{
    internal class OpenConnection
    {
        private static readonly TimeSpan InfiniteTimespan = TimeSpan.FromMilliseconds(System.Threading.Timeout.Infinite);

#if SUPPORTS_NLOG
        private readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
#endif

        public OpenConnection(SocksAddress address, ITcpClient client)
        {
            Address = address;
            Client = client;
            Timeout = InfiniteTimespan;
            MaxUsageCount = -1;
        }

        public static readonly TimeSpan SafeTimeoutMargin = TimeSpan.FromSeconds(0.5);

        public SocksAddress Address { get; private set; }
        public ITcpClient Client { get; private set; }
        public TimeSpan Timeout { get; private set; }
        public int MaxUsageCount { get; private set; }
        public DateTime LastUsage { get; private set; }
        public DateTime MaxValidTimestamp { get; private set; }
        public int UsageCount { get; private set; }

        public bool LimitsSpecified
        {
            get
            {
                return Timeout != InfiniteTimespan
                       || MaxUsageCount != -1;
            }
        }

        public bool DisposeIfInvalid(DateTime now)
        {
            if (IsValid(now))
                return false;
            Client.Dispose();
            return true;
        }

        public bool IsValid(DateTime now)
        {
            var timeoutExceeded = (Timeout != InfiniteTimespan) && (now >= MaxValidTimestamp);
#if SUPPORTS_NLOG
            if (timeoutExceeded)
                _logger.Debug("Timeout exceeded");
#endif
            var usageCountExceeded = (MaxUsageCount != -1) && (UsageCount >= MaxUsageCount);
#if SUPPORTS_NLOG
            if (usageCountExceeded)
                _logger.Debug("Usage count exceeded");
#endif
            return !timeoutExceeded && !usageCountExceeded;
        }

        public IDictionary<string, string> GetKeepAliveValues(IEnumerable<string> values)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var value in values)
            {
                var equalSignPos = value.IndexOf('=');
                var kaKey = ((equalSignPos == -1) ? value : value.Substring(0, equalSignPos)).Trim();
                var kaValue = (equalSignPos == -1) ? string.Empty : value.Substring(equalSignPos + 1).Trim();
                result[kaKey] = kaValue;
            }
            return result;
        }

        public void Update(HttpResponseMessage message)
        {
            Update(message, DateTime.UtcNow);
        }

        public void Update(HttpResponseMessage message, DateTime now)
        {
            var keepAlive = message.Version >= HttpVersions.Version11 ||
                            message.Headers.Connection.Any(
                                x => x.IndexOf("Keep-Alive", 0, StringComparison.OrdinalIgnoreCase) != -1);
            if (!keepAlive)
            {
                MaxUsageCount = 1;
#if SUPPORTS_NLOG
                _logger.Debug("Max usage count for {1} set to {0}", MaxUsageCount, Address);
#endif
            }
            else
            {
                IEnumerable<string> keepAliveValues;
                if (message.Headers.TryGetValues("Keep-Alive", out keepAliveValues))
                {
                    var kaValues = GetKeepAliveValues(keepAliveValues);
                    string kaValue;
                    if (kaValues.TryGetValue("timeout", out kaValue))
                    {
                        Timeout = TimeSpan.FromSeconds(int.Parse(kaValue));
#if SUPPORTS_NLOG
                        _logger.Debug("Timeout for {1} set to {0}", Timeout, Address);
#endif
                    }
                    if (kaValues.TryGetValue("max", out kaValue))
                    {
                        MaxUsageCount = int.Parse(kaValue);
#if SUPPORTS_NLOG
                        _logger.Debug("Max usage count for {1} set to {0}", MaxUsageCount, Address);
#endif
                    }
                }
                if (!LimitsSpecified)
                {
                    Timeout = TimeSpan.FromSeconds(5);
#if SUPPORTS_NLOG
                    _logger.Debug("Timeout for {1} set to {0}", Timeout, Address);
#endif
                }
            }
            UsageCount += 1;
            LastUsage = now;
            if (Timeout != InfiniteTimespan)
                MaxValidTimestamp = LastUsage + Timeout - SafeTimeoutMargin;
        }
    }
}
