using System;
using System.Collections.Generic;
using System.Text;

namespace RestSharp.Portable.Socks
{
    [Flags]
    public enum AddressCompatibility
    {
        SupportsIPv4 = 0,
        SupportsHost = 1,
        SupportsIPv6 = 2,
    }
}
