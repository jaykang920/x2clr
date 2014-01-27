// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

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

    // TODO: buffer transform stack
}
