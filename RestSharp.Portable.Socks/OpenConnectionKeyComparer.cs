using System.Collections;
using System.Collections.Generic;

namespace RestSharp.Portable.Socks
{
    class OpenConnectionKeyComparer : IComparer<OpenConnectionKey>, IEqualityComparer<OpenConnectionKey>, IComparer, IEqualityComparer
    {
        public readonly static OpenConnectionKeyComparer Default = new OpenConnectionKeyComparer();

        bool IEqualityComparer.Equals(object x, object y)
        {
            return Equals((OpenConnectionKey) x, (OpenConnectionKey) y);
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            return GetHashCode((OpenConnectionKey) obj);
        }

        int IComparer.Compare(object x, object y)
        {
            return Compare((OpenConnectionKey) x, (OpenConnectionKey) y);
        }

        public int Compare(OpenConnectionKey x, OpenConnectionKey y)
        {
            if (ReferenceEquals(x, y))
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;
            var result = x.Address.CompareTo(y.Address);
            if (result != 0)
                return result;
            result = x.UseSsl.CompareTo(y.UseSsl);
            return result;
        }

        public bool Equals(OpenConnectionKey x, OpenConnectionKey y)
        {
            return Compare(x, y) == 0;
        }

        public int GetHashCode(OpenConnectionKey obj)
        {
            var hashCode = 0;
            if (obj != null)
                hashCode = obj.Address.GetHashCode() ^ obj.UseSsl.GetHashCode();
            return hashCode;
        }
    }
}
