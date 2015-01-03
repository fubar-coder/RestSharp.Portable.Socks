using System;
using System.Collections.Generic;
using System.Text;

namespace RestSharp.Portable.Socks
{
    static class SocksVersion
    {
        public static readonly Version SocksV4 = new Version(4, 0);
        public static readonly Version SocksV4a = new Version(4, 1);
        public static readonly Version SocksV5 = new Version(5, 0);
    }
}
