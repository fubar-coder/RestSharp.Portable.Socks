using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RestSharp.Portable.Socks
{
    public class SocksAddressComparer : IComparer<SocksAddress>, IEqualityComparer<SocksAddress>, IComparer, IEqualityComparer
    {
        public static readonly SocksAddressComparer Default = new SocksAddressComparer();

        public int Compare(SocksAddress x, SocksAddress y)
        {
            if (ReferenceEquals(x, y))
                return 0;
            if (ReferenceEquals(x, null))
                return -1;
            if (ReferenceEquals(y, null))
                return 1;
            var result = x.HostNameType.CompareTo(y.HostNameType);
            if (result != 0)
                return result;
            result = string.Compare(x.Host, y.Host, StringComparison.OrdinalIgnoreCase);
            if (result != 0)
                return result;
            return x.Port.CompareTo(y.Port);
        }

        public bool Equals(SocksAddress x, SocksAddress y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(SocksAddress obj)
        {
            var hashCode = 0;
            if (ReferenceEquals(obj, null))
                return hashCode;
            hashCode ^= obj.HostNameType.GetHashCode();
            hashCode ^= obj.Host.ToLowerInvariant().GetHashCode();
            hashCode ^= obj.Port.GetHashCode();
            return hashCode;
        }

        int IComparer.Compare(object x, object y)
        {
            return Compare((SocksAddress) x, (SocksAddress) y);
        }

        bool IEqualityComparer.Equals(object x, object y)
        {
            return Equals((SocksAddress) x, (SocksAddress) y);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            return GetHashCode((SocksAddress) obj);
        }
    }
}
