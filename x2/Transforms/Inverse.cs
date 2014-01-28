// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace x2.Transforms
{
    public class Inverse : IBufferTransform
    {
        public object Clone()
        {
            return new Inverse();
        }

        public byte[] InitializeHandshake()
        {
            return null;
        }

        public bool FinalizeHandshake(byte[] data)
        {
            return true;
        }

        public int Transform(Buffer buffer, int length)
        {
            for (int i = buffer.Length - length; i < buffer.Length; ++i)
            {
                buffer[i] = (byte)~buffer[i];
            }
            return length;
        }

        public int InverseTransform(Buffer buffer, int length)
        {
            for (int i = 0; i < length; ++i)
            {
                buffer[i] = (byte)~buffer[i];
            }
            return length;
        }
    }
}
