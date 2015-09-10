// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;

using x2;

namespace x2.Links
{
    public class SendBuffer : IDisposable
    {
        private byte[] headerBytes;
        private int headerLength;
        private Buffer buffer;

        public byte[] HeaderBytes { get { return headerBytes; } }
        public int HeaderLength
        {
            get { return headerLength; }
            set { headerLength = value; }
        }
        public Buffer Buffer { get { return buffer; } }
        public int Length { get { return (headerLength + (int)buffer.Length); } }

        public SendBuffer()
        {
            headerBytes = new byte[5];
            buffer = new Buffer(12);
        }

        ~SendBuffer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            buffer.Dispose();
        }

        public void ListOccupiedSegments(IList<ArraySegment<byte>> blockList)
        {
            blockList.Add(new ArraySegment<byte>(headerBytes, 0, headerLength));
            buffer.ListOccupiedSegments(blockList);
        }
    }

}
