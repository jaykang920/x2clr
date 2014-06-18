﻿// Copyright (c) 2013, 2014 Jae-jun Kang
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

        private bool established;

        public int HandshakeBlockLength { get { return 8; } }
        public bool Established { get { return established; } }

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
            return new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        }

        public byte[] Handshake(byte[] challenge)
        {
            // TODO
            return new byte[] { 9, 10, 11, 12, 13, 14, 15, 16 };
        }

        public bool FinalizeHandshake(byte[] response)
        {
            // TODO
            return true;
        }

        public int Transform(Buffer buffer, int length)
        {
            Log.Trace("Cipher.Transform: input length {0}", length);

            int result;

            var ms = new MemoryStream(length + BlockSizeInBytes);
            var encryptor = algorithm.CreateEncryptor(key, iv);
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                var buffers = new List<ArraySegment<byte>>();
                buffer.ListEndingSegments(buffers, length);

                for (var i = 0; i < buffers.Count; ++i)
                {
                    var segment = buffers[i];

                    if (Log.Level <= LogLevel.Trace)
                    {
                        Log.Trace("Cipher.Transform: input block {0}",
                            BitConverter.ToString(segment.Array, segment.Offset, segment.Count));
                    }

                    cs.Write(segment.Array, segment.Offset, segment.Count);
                }
                cs.FlushFinalBlock();

                result = (int)ms.Length;
                var streamBuffer = ms.GetBuffer();

                if (Log.Level <= LogLevel.Trace)
                {
                    Log.Trace("Cipher.Transform: output {0} {1}",
                        result, BitConverter.ToString(streamBuffer, 0, result));
                }

                buffer.Rewind();
                buffer.CopyFrom(streamBuffer, 0, result);
            }

            return result;
        }

        public int InverseTransform(Buffer buffer, int length)
        {
            Log.Trace("Cipher.InverseTransform: input length {0}", length);

            int result;

            var ms = new MemoryStream(length);
            var decryptor = algorithm.CreateDecryptor(key, iv);
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
            {
                var buffers = new List<ArraySegment<byte>>();
                buffer.ListStartingSegments(buffers, length);

                for (var i = 0; i < buffers.Count; ++i)
                {
                    var segment = buffers[i];

                    if (Log.Level <= LogLevel.Trace)
                    {
                        Log.Trace("Cipher.InverseTransform: input block {0}",
                            BitConverter.ToString(segment.Array, segment.Offset, segment.Count));
                    }

                    cs.Write(segment.Array, segment.Offset, segment.Count);
                }
                cs.FlushFinalBlock();

                result = (int)ms.Length;
                var streamBuffer = ms.GetBuffer();

                if (Log.Level <= LogLevel.Trace)
                {
                    Log.Trace("Cipher.InverseTransform: output {0} {1}",
                        result, BitConverter.ToString(streamBuffer, 0, result));
                }

                buffer.Rewind();
                buffer.CopyFrom(streamBuffer, 0, result);
            }
            return result;
        }
    }
}
