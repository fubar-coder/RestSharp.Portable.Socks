﻿using System;
using System.IO;

namespace RestSharp.Portable.Socks
{
    class TcpClientStream : Stream
    {
        private readonly MemoryStream _preBaseStream;
        private readonly Stream _baseStream;
        private readonly long? _contentLength;
        private readonly long _preDataSize;
        private long _position = 0;

        public TcpClientStream(byte[] preData, Stream baseStream, long? contentLength)
        {
            _contentLength = contentLength;
            _preDataSize = preData.Length;
            _preBaseStream = new MemoryStream(preData);
            _baseStream = baseStream;
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_contentLength != null && _position >= _contentLength)
                return 0;
            var stream = (_position < _preDataSize) ? _preBaseStream : _baseStream;
            var readCount = stream.Read(buffer, offset, count);
            _position += readCount;
            return readCount;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { return _position; }
            set { throw new NotSupportedException(); }
        }
    }
}
