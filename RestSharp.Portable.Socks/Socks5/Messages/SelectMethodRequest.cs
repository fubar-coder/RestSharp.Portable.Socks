using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RestSharp.Portable.Socks.Socks5.Messages
{
    public class SelectMethodRequest : Request
    {
        private readonly byte[] _methods;

        public SelectMethodRequest(IEnumerable<byte> methods)
        {
            _methods = methods.ToArray();
        }

        public IEnumerable<byte> Methods { get { return _methods; } }
        protected override void WritePayloadTo(BinaryWriter writer)
        {
            writer.Write((byte)_methods.Length);
            writer.Write(_methods);
        }
    }
}
