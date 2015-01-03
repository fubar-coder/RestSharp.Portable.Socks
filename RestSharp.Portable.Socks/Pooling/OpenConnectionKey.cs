using System;
using System.Collections.Generic;
using System.Text;

namespace RestSharp.Portable.Socks.Pooling
{
    class OpenConnectionKey : IComparable<OpenConnectionKey>, IEquatable<OpenConnectionKey>, IComparable
    {
        public SocksAddress Address { get; private set; }
        public bool UseSsl { get; private set; }

        public OpenConnectionKey(SocksAddress address, bool useSsl)
        {
            Address = address;
            UseSsl = useSsl;
        }

        public bool Equals(OpenConnectionKey other)
        {
            return CompareTo(other) == 0;
        }

        public int CompareTo(OpenConnectionKey other)
        {
            if (ReferenceEquals(this, other))
                return 0;
            if (other == null)
                return 1;
            var result = Address.CompareTo(other.Address);
            if (result != 0)
                return result;
            result = UseSsl.CompareTo(other.UseSsl);
            return result;
        }

        int IComparable.CompareTo(object obj)
        {
            return CompareTo((OpenConnectionKey) obj);
        }

        public override bool Equals(object obj)
        {
            return Equals((OpenConnectionKey)obj);
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode() ^ UseSsl.GetHashCode();
        }
    }
}
