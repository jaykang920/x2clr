// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;

namespace x2
{
    public interface IBufferTransform : ICloneable
    {
        int HandshakeBlockLength { get; }

        byte[] InitializeHandshake();
        byte[] Handshake(byte[] challenge);
        bool FinalizeHandshake(byte[] response);

        /// <summary>
        /// Transform the specified trailing byte(s) of the buffer.
        /// </summary>
        int Transform(Buffer buffer, int length);
        /// <summary>
        /// Inverse transform the specified leading byte(s) of the buffer.
        /// </summary>
        int InverseTransform(Buffer buffer, int length);
    }

    public class BufferTransformStack : IBufferTransform
    {
        private readonly IList<IBufferTransform> transforms;

        public int HandshakeBlockLength
        {
            get
            {
                int result = 0;
                for (int i = 0, count = transforms.Count; i < count; ++i)
                {
                    result += transforms[i].HandshakeBlockLength;
                }
                return result;
            }
        }

        public BufferTransformStack()
        {
            transforms = new List<IBufferTransform>();
        }

        private BufferTransformStack(IList<IBufferTransform> transforms)
        {
            this.transforms = new List<IBufferTransform>();
            for (int i = 0, count = transforms.Count; i < count; ++i)
            {
                this.transforms.Add((IBufferTransform)transforms[i].Clone());
            }
        }

        public object Clone()
        {
            return new BufferTransformStack(transforms);
        }

        public byte[] InitializeHandshake()
        {
            byte[] result = null;
            for (int i = 0, count = transforms.Count; i < count; ++i)
            {
                result = Combine(result, transforms[i].InitializeHandshake());
            }
            return result;
        }

        public byte[] Handshake(byte[] challenge)
        {
            byte[] result = null;
            int offset = 0;
            for (int i = 0, count = transforms.Count; i < count; ++i)
            {
                var transform = transforms[i];
                var blockLength = transform.HandshakeBlockLength;
                if (blockLength > 0)
                {
                    var block = new byte[blockLength];
                    System.Buffer.BlockCopy(challenge, offset, block, 0, blockLength);

                    result = Combine(result, transforms[i].Handshake(block));
                }
            }
            return result;
        }

        public bool FinalizeHandshake(byte[] response)
        {
            int offset = 0;
            for (int i = 0, count = transforms.Count; i < count; ++i)
            {
                var transform = transforms[i];
                var blockLength = transform.HandshakeBlockLength;
                if (blockLength > 0)
                {
                    var block = new byte[blockLength];
                    System.Buffer.BlockCopy(response, offset, block, 0, blockLength);

                    if (!transforms[i].FinalizeHandshake(block))
                    {
                        return false;
                    }
                }
                offset += blockLength;
            }
            return true;
        }

        public BufferTransformStack Add(IBufferTransform transform)
        {
            if (!transforms.Contains(transform))
            {
                transforms.Add(transform);
            }
            return this;
        }

        public BufferTransformStack Remove(IBufferTransform transform)
        {
            transforms.Remove(transform);
            return this;
        }

        public int Transform(Buffer buffer, int length)
        {
            var count = transforms.Count;
            for (var i = 0; i < count; ++i)
            {
                length = transforms[i].Transform(buffer, length);
            }
            return length;
        }

        public int InverseTransform(Buffer buffer, int length)
        {
            var count = transforms.Count;
            for (var i = count - 1; i >= 0; --i)
            {
                length = transforms[i].InverseTransform(buffer, length);
            }
            return length;
        }

        private static byte[] Combine(byte[] first, byte[] second)
        {
            if (second == null)
            {
                return first;
            }
            if (first == null)
            {
                return second;
            }
            byte[] result = new byte[first.Length + second.Length];
            System.Buffer.BlockCopy(first, 0, result, 0, first.Length);
            System.Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
            return result;
        }
    }
}
