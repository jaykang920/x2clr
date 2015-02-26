using System;
using System.Collections.Generic;
using System.IO;

namespace x2
{
    /// <summary>
    /// Creates a stream whose backing store is rather a set of fixed-length
    /// memory chunks than a single large memory block.
    /// </summary>
    public sealed class ChunkedMemoryStream : Stream
    {
        private const int chunkSizeExponent = 17;  // 128K (to reside in LOH)
        private const int chunkSize = 1 << chunkSizeExponent;
        private const int remainderMask = ~(~0 << chunkSizeExponent);

        private readonly List<byte[]> chunks;  // list of chunks

        private byte[] chunk;  // shortcut to current chunk
        private int chunkIndex;  // current chunk index
        private long position;
        private long back;
        private long front;

        private long readMarker;

        private volatile bool disposed;

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
        /// Gets or sets the number of bytes allocated for this stream.
        /// </summary>
        public long Capacity
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return ((long)chunkSize * chunks.Count);
            }
            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                while (value >= Capacity)
                {
                    chunks.Add(BufferPool.Acquire(chunkSizeExponent));
                }
            }
        }

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
        /// Gets the length of the stream in bytes.
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
        /// Gets or sets the current position within the stream.
        /// </summary>
        public override long Position
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                return (position - front);
            }
            set
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
                long adjusted = value + front;
                if (adjusted < front)
                {
                    adjusted = front;
                }
                if (adjusted > back)
                {
                    adjusted = back;
                }
                position = adjusted;
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

        /// <summary>
        /// Initializes a new instance of the ChunkedMemoryStream class.
        /// </summary>
        public ChunkedMemoryStream()
        {
            chunks = new List<byte[]>();
            chunks.Add(BufferPool.Acquire(chunkSizeExponent));

            chunkIndex = 0;
            chunk = chunks[chunkIndex];
            position = back = front = 0;

            readMarker = -1;
        }

        ~ChunkedMemoryStream()
        {
            Dispose(false);
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to
        /// be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            return;
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the
        /// position within the stream by the number of bytes read.
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }
            if (offset < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            if ((offset + count) > buffer.Length)
            {
                throw new ArgumentException();
            }
            if (!CheckLengthToRead(count))
            {
                if (position == back)
                {
                    return 0;
                }
                count = (int)(back - position);
            }
            CopyTo(buffer, offset, count, position);
            Position += count;
            return count;
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the
        /// stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        public override int ReadByte()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (!CheckLengthToRead(1))
            {
                return -1;
            }
            ChunkFeed();
            return chunk[position++ & remainderMask];
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            // SeekOrigin.Begin
            long result = 0;
            if (origin != SeekOrigin.Begin)
            {
                if (origin == SeekOrigin.End)
                {
                    result = Length;
                }
                else
                {
                    // SeekOrigin.Current
                    result = Position;
                }
            }
            result += offset;
            if (result < 0)
            {
                result = 0;
            }
            else if (result > Length)
            {
                // Seeking to any location beyond the length of the stream is supported.
                SetLength(result);
            }
            Position = result;
            return result;
        }

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        public override void SetLength(long value)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            if (value == Length)
            {
                return;
            }
            if (value > Length)
            {
                Capacity = front + value;
            }
            back = front + value;
        }

        /// <summary>
        /// Writes the stream contents to a byte array, regardless of the
        /// Position property.
        /// </summary>
        public byte[] ToArray()
        {
            byte[] array = new byte[Length];
            CopyTo(array, 0, (int)Length, front);
            return array;
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the
        /// current position within this stream by the number of bytes written.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            if (buffer == null)
            {
                throw new ArgumentNullException();
            }
            if (offset < 0 || count < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            if ((offset + count) > buffer.Length)
            {
                throw new ArgumentException();
            }
            EnsureCapacityToWrite(count);
            int index = (int)(position >> chunkSizeExponent);
            int dstOffset = (int)(position & remainderMask);
            int bytesToCopy, bytesCopied = 0;
            while (bytesCopied < count)
            {
                bytesToCopy = Math.Min(chunkSize - dstOffset, count - bytesCopied);
                System.Buffer.BlockCopy(buffer, offset + bytesCopied,
                    chunks[index++], dstOffset, bytesToCopy);
                dstOffset = 0;
                bytesCopied += bytesToCopy;
            }
            Position += count;
        }

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the
        /// position within the stream by one byte.
        /// </summary>
        public override void WriteByte(byte value)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
            EnsureCapacityToWrite(1);
            ChunkFeed();
            chunk[position++ & remainderMask] = value;
        }

        /// <summary>
        /// Releases the resources used by the Stream.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposed) { return; }

            base.Dispose(disposing);

            for (int i = 0, count = chunks.Count; i < count; ++i)
            {
                BufferPool.Release(chunkSizeExponent, chunks[i]);
            }
            chunks.Clear();
            chunk = null;
            chunkIndex = -1;

            disposed = true;
        }

        #region ArraySegment methods

        public void ListOccupiedSegments(IList<ArraySegment<byte>> blocks)
        {
            ListSegments(blocks, front, back);
        }

        public void ListStartingSegments(IList<ArraySegment<byte>> blocks, int length)
        {
            ListSegments(blocks, front, front + length);
        }

        public void ListEndingSegments(IList<ArraySegment<byte>> blocks, int length)
        {
            ListSegments(blocks, back - length, back);
        }

        private void ListSegments(IList<ArraySegment<byte>> blocks, long begin, long end)
        {
            int beginIndex = (int)(begin >> chunkSizeExponent);
            int beginOffset = (int)(begin & remainderMask);
            int endIndex = (int)(end >> chunkSizeExponent);
            int endOffset = (int)(end & remainderMask);
            if (beginIndex == endIndex)
            {
                blocks.Add(new ArraySegment<byte>(chunks[beginIndex], beginOffset,
                                                  endOffset - beginOffset));
                return;
            }
            blocks.Add(new ArraySegment<byte>(chunks[beginIndex], beginOffset,
                                              chunkSize - beginOffset));
            for (int i = beginIndex + 1; i < endIndex; ++i)
            {
                blocks.Add(new ArraySegment<byte>(chunks[i]));
            }
            if (endOffset != 0)
            {
                blocks.Add(new ArraySegment<byte>(chunks[endIndex], 0, endOffset));
            }
        }

        public void ListAvailableSegments(IList<ArraySegment<byte>> blocks)
        {
            if ((Capacity - back) < chunkSize)
            {
                chunks.Add(BufferPool.Acquire(chunkSizeExponent));
            }
            int backIndex = (int)(back >> chunkSizeExponent);
            int backOffset = (int)(back & remainderMask);
            blocks.Add(new ArraySegment<byte>(chunks[backIndex], backOffset,
                                                 chunkSize - backOffset));
            for (int i = backIndex + 1; i < chunks.Count; ++i)
            {
                blocks.Add(new ArraySegment<byte>(chunks[i]));
            }
        }

        #endregion

        #region Advanced buffer manipulation methods

        public void MarkToRead(int lengthToRead)
        {
            if ((front + lengthToRead) > back)
            {
                throw new IndexOutOfRangeException();
            }
            readMarker = front + lengthToRead;
        }

        public void Shrink(int numBytes)
        {
            if ((front + numBytes) > back)
            {
                throw new IndexOutOfRangeException();
            }
            front += numBytes;
            if (position < front)
            {
                Position = 0;
            }
        }

        public void Stretch(int numBytes)
        {
            if ((back + numBytes) > Capacity)
            {
                throw new IndexOutOfRangeException();
            }
            back += numBytes;
        }

        public void Trim()
        {
            int index, count;
            if (readMarker >= 0)
            {
                if (position < readMarker)
                {
                    Position = (readMarker - front);
                }
                readMarker = -1;
            }
            if (position == back)
            {
                index = 1;
                count = chunks.Count - 1;
                front = back = 0;
            }
            else
            {
                index = 0;
                count = (int)(position >> chunkSizeExponent);
                if (count >= chunks.Count)
                {
                    count = chunks.Count - 1;
                }
                back -= chunkSize * count;
                front = position & remainderMask;
            }
            if (count > 0)
            {
                List<byte[]> blocksToRemove = chunks.GetRange(index, count);
                chunks.RemoveRange(index, count);
                for (int i = 0; i < blocksToRemove.Count; ++i)
                {
                    BufferPool.Release(chunkSizeExponent, blocksToRemove[i]);
                }
            }
            Position = 0;
        }

        #endregion

        private bool CheckLengthToRead(int numBytes)
        {
            return ((position + numBytes) <= back);
        }

        private void ChunkFeed()
        {
            if (((position & remainderMask) == 0) &&
                ((position & ~remainderMask) != 0))
            {
                chunk = chunks[++chunkIndex];
            }
        }

        private void CopyTo(byte[] buffer, int offset, int count, long position)
        {
            int index = (int)(position >> chunkSizeExponent);
            int srcOffset = (int)(position & remainderMask);
            int bytesToCopy, bytesCopied = 0;
            while (bytesCopied < count)
            {
                bytesToCopy = Math.Min(chunkSize - srcOffset, count - bytesCopied);
                System.Buffer.BlockCopy(chunks[index++], srcOffset,
                  buffer, offset + bytesCopied, bytesToCopy);
                srcOffset = 0;
                bytesCopied += bytesToCopy;
            }
        }

        private void EnsureCapacityToWrite(int numBytes)
        {
            long required = position + numBytes;
            if (required >= Capacity)
            {
                Capacity = required;
            }
            if (required > back)
            {
                back = required;
            }
        }
    }
}
