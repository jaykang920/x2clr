using System;
using System.Collections.Generic;
using System.IO;

namespace x2
{
    /// <summary>
    /// Creates a stream whose backing store is rather a set of fixed-length
    /// memory chunks than a single big memory block.
    /// </summary>
    public sealed class ChunkedMemoryStream : Stream
    {
        private const int chunkSizeExponent = 17;  // 128K (to reside in LOH)
        private const int remainderMask = ~(~0 << chunkSizeExponent);

        private readonly List<byte[]> chunks = new List<byte[]>();

        private byte[] chunk;  // current chunk
        private int chunkIndex;  // current chunk index
        private long position;
        private long back;
        private long front;

        private bool disposed;

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead { get { return !disposed; } }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek { get { return !disposed; } }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite { get { return !disposed; } }

        /// <summary>
        /// Shorthand for testing (Length == 0).
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return (front == back);
            }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return (back - front);
            }
        }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return position;
            }
            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                if (value < front || back < value)
                {
                    // Consider supporting seek-beyond-the-length case.
                    throw new IndexOutOfRangeException();
                }
                position = value;
                int index = (int)(position >> chunkSizeExponent);
                if ((index != 0) && ((position & remainderMask) == 0))
                {
                    --index;
                }
                if (index != chunkIndex)
                {
                    chunkIndex = index;
                    chunk = chunks[chunkIndex];
                }
            }
        }

        private int ChunkSize
        {
            get { return (1 << chunkSizeExponent); }
        }

        private long Capacity
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return ((long)ChunkSize * chunks.Count);
            }
        }

        /// <summary>
        /// Initializes a new instance of the ChunkedMemoryStream class.
        /// </summary>
        public ChunkedMemoryStream()
        {
            chunks.Add(BufferPool.Acquire(chunkSizeExponent));

            chunkIndex = 0;
            chunk = chunks[chunkIndex];
        }

        public override void Flush()
        {
            return;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            base.Dispose(disposing);

            CleanUp();

            disposed = true;
        }

        private void CleanUp()
        {
            if (chunks.Count == 0) { return; }
            for (int i = 0, count = chunks.Count; i < count; ++i)
            {
                BufferPool.Release(chunkSizeExponent, chunks[i]);
            }
            chunks.Clear();
            chunk = null;
            chunkIndex = -1;
        }
    }
}
