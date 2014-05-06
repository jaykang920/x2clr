// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;

namespace x2
{
    public interface IBufferTransform : ICloneable
    {
        byte[] InitializeHandshake();
        bool FinalizeHandshake(byte[] data);

        /// <summary>
        /// Transform the specified ending byte(s) of the buffer.
        /// </summary>
        int Transform(Buffer buffer, int length);
        /// <summary>
        /// Inverse transform the specified starting byte(s) of the buffer.
        /// </summary>
        int InverseTransform(Buffer buffer, int length);
    }

    public class BufferTransformStack : IBufferTransform
    {
        private readonly IList<IBufferTransform> transforms;

        public BufferTransformStack()
        {
            transforms = new List<IBufferTransform>();
        }

        private BufferTransformStack(IList<IBufferTransform> transforms)
        {
            this.transforms = new List<IBufferTransform>(transforms);
        }

        public object Clone()
        {
            return new BufferTransformStack(transforms);
        }

        public byte[] InitializeHandshake()
        {
            // TODO
            return null;
        }

        public bool FinalizeHandshake(byte[] data)
        {
            // TODO
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
            if (count > 0)
            {
                for (var i = 0; i < count; ++i)
                {
                    length = transforms[i].Transform(buffer, length);
                }
            }
            return length;
        }

        public int InverseTransform(Buffer buffer, int length)
        {
            var count = transforms.Count;
            if (count > 0)
            {
                for (var i = count - 1; i >= 0; --i)
                {
                    length = transforms[i].InverseTransform(buffer, length);
                }
            }
            return length;
        }
    }
}
