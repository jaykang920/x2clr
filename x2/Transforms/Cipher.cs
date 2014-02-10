// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace x2.Transforms
{
    public class Cipher : IBufferTransform
    {
        SymmetricAlgorithm algorithm;

        private byte[] key;
        private byte[] iv;

        private int KeySizeInBytes { get { return (algorithm.KeySize / 8); } }
        private int BlockSizeInBytes { get { return (algorithm.BlockSize / 8); } }

        public Cipher()
        {
            algorithm = AesCryptoServiceProvider.Create();
            algorithm.Mode = CipherMode.CBC;

            key = new byte[KeySizeInBytes];
            iv = new byte[BlockSizeInBytes];

            // TODO: handshaking and dynamic generation
            for (int i = 0; i < KeySizeInBytes; ++i)
            {
                key[i] = (byte)i;
            }
            for (int i = 0; i < BlockSizeInBytes; ++i)
            {
                iv[i] = (byte)i;
            }
        }

        public object Clone()
        {
            return new Cipher();
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

        public int Transform(Buffer buffer, int length)
        {
            int result;
            using (var ms = new MemoryStream(length + BlockSizeInBytes))
            {
                var encryptor = algorithm.CreateEncryptor(key, iv);
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    var buffers = new List<ArraySegment<byte>>();
                    buffer.ListEndingSegments(buffers, length);

                    for (var i = 0; i < buffers.Count; ++i)
                    {
                        var segment = buffers[i];

                        Log.Trace("Cipher.Transform: input {0} {1}",
                            length, BitConverter.ToString(segment.Array, segment.Offset, segment.Count));

                        cs.Write(segment.Array, segment.Offset, segment.Count);
                    }
                    cs.FlushFinalBlock();

                    result = (int)ms.Length;
                }

                buffer.Rewind();
                buffer.CopyFrom(ms.GetBuffer(), 0, result);

                Log.Trace("Cipher.Transform: output {0} {1}",
                    result, BitConverter.ToString(ms.GetBuffer(), 0, result));
            }
            return result;
        }

        public int InverseTransform(Buffer buffer, int length)
        {
            int result;
            using (var ms = new MemoryStream(length))
            {
                var decryptor = algorithm.CreateDecryptor(key, iv);
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                {
                    var buffers = new List<ArraySegment<byte>>();
                    buffer.ListStartingSegments(buffers, length);

                    for (var i = 0; i < buffers.Count; ++i)
                    {
                        var segment = buffers[i];

                        Log.Trace("Cipher.InverseTransform: input {0} {1}",
                            length, BitConverter.ToString(segment.Array, segment.Offset, segment.Count));

                        cs.Write(segment.Array, segment.Offset, segment.Count);
                    }
                    cs.FlushFinalBlock();

                    result = (int)ms.Length;
                }

                buffer.Rewind();
                buffer.CopyFrom(ms.GetBuffer(), 0, result);

                Log.Trace("Cipher.InverseTransform: output {0} {1}",
                    result, BitConverter.ToString(ms.GetBuffer(), 0, result));
            }
            return result;
        }
    }
}
