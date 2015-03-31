// Copyright (c) 2013-2015 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace x2.Transforms
{
    // Important!
    // Illustration purpose only. Do NOT use this as is in production.
    public class Cipher : IBufferTransform
    {
        private static byte[] sharedSecret = {
             1, 11, 12,  5,
            10,  2,  6, 13,
             9,  7,  3, 14,
             8, 16, 15,  4,

            10,  9,  8,  7,
            11,  2,  1,  6,
            12,  3,  4,  5,
            13, 14, 15, 16
        };

        SymmetricAlgorithm encryptingAlgorithm;
        SymmetricAlgorithm decryptingAlgorithm;

        private byte[] encryptingKey;
        private byte[] decryptingKey;

        private byte[] encryptingIV;
        private byte[] decryptingIV;

        private int EncryptingKeySizeInBytes { get { return (encryptingAlgorithm.KeySize / 8); } }
        private int EncryptingBlockSizeInBytes { get { return (encryptingAlgorithm.BlockSize / 8); } }
        private int DecryptingKeySizeInBytes { get { return (decryptingAlgorithm.KeySize / 8); } }
        private int DecryptingBlockSizeInBytes { get { return (decryptingAlgorithm.BlockSize / 8); } }

        public int HandshakeBlockLength { get { return 32; } }

        public Cipher()
        {
            encryptingAlgorithm = AesCryptoServiceProvider.Create();
            encryptingAlgorithm.Mode = CipherMode.CBC;
            encryptingAlgorithm.Padding = PaddingMode.PKCS7;

            decryptingAlgorithm = AesCryptoServiceProvider.Create();
            decryptingAlgorithm.Mode = CipherMode.CBC;
            decryptingAlgorithm.Padding = PaddingMode.PKCS7;
        }

        public object Clone()
        {
            return new Cipher();
        }

        public byte[] InitializeHandshake()
        {
            var challenge = new byte[HandshakeBlockLength];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(challenge);

            decryptingKey = challenge;

            return challenge;
        }

        public byte[] Handshake(byte[] challenge)
        {
            var data = new byte[HandshakeBlockLength];
            var response = new byte[HandshakeBlockLength];
            for (int i = 0; i < HandshakeBlockLength; ++i)
            {
                data[i] = (byte)(challenge[i] ^ ~sharedSecret[i]);
                response[i] = (byte)(challenge[i] ^ sharedSecret[i]);
            }

            encryptingKey = new byte[EncryptingKeySizeInBytes];
            encryptingIV = new byte[EncryptingBlockSizeInBytes];

            System.Buffer.BlockCopy(data, 0, encryptingKey, 0, EncryptingKeySizeInBytes);
            System.Buffer.BlockCopy(data, HandshakeBlockLength - EncryptingBlockSizeInBytes,
                encryptingIV, 0, EncryptingBlockSizeInBytes);

            return response;
        }

        public bool FinalizeHandshake(byte[] response)
        {
            var actual = new byte[HandshakeBlockLength];
            for (int i = 0; i < HandshakeBlockLength; ++i)
            {
                actual[i] = (byte)(response[i] ^ sharedSecret[i]);
            }

            bool result = actual.SequenceEqual(decryptingKey);

            if (result)
            {
                decryptingKey = new byte[DecryptingKeySizeInBytes];
                decryptingIV = new byte[DecryptingBlockSizeInBytes];

                for (int i = 0; i < HandshakeBlockLength; ++i)
                {
                    actual[i] = (byte)(actual[i] ^ ~sharedSecret[i]);
                }
                System.Buffer.BlockCopy(actual, 0, decryptingKey, 0, DecryptingKeySizeInBytes);
                System.Buffer.BlockCopy(actual, HandshakeBlockLength - DecryptingBlockSizeInBytes,
                    decryptingIV, 0, DecryptingBlockSizeInBytes);
            }

            return result;
        }

        public int Transform(Buffer buffer, int length)
        {
            Log.Trace("Cipher.Transform: input length {0}", length);

            int result;

            using (var ms = new MemoryStream(length + EncryptingBlockSizeInBytes))
            {
                var encryptor = encryptingAlgorithm.CreateEncryptor(encryptingKey, encryptingIV);
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
#if UNITY_WORKAROUND
                    // Workaround for ancient mono 2.0 of Unity3D
                    // Multiple Write() calls are not properly handled there.

                    byte[] plaintext = buffer.ToArray();
                    if (Log.Level <= LogLevel.Trace)
                    {
                        Log.Trace("Cipher.Transform: input {0}",
                            BitConverter.ToString(plaintext, plaintext.Length - length, length));
                    }
                    cs.Write(plaintext, plaintext.Length - length, length);
#else
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
#endif

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

                    // Store the last ciphertext block as a next encrypting IV.
                    System.Buffer.BlockCopy(streamBuffer, result - EncryptingBlockSizeInBytes,
                        encryptingIV, 0, EncryptingBlockSizeInBytes);
                }
            }

            return result;
        }

        public int InverseTransform(Buffer buffer, int length)
        {
            Log.Trace("Cipher.InverseTransform: input length {0}", length);

            int result;

            using (var ms = new MemoryStream(length))
            {
                var decryptor = decryptingAlgorithm.CreateDecryptor(decryptingKey, decryptingIV);
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                {
#if UNITY_WORKAROUND
                    // Workaround for ancient mono 2.0 of Unity3D
                    // Multiple Write() calls are not properly handled there.

                    byte[] ciphertext = buffer.ToArray();
                    if (Log.Level <= LogLevel.Trace)
                    {
                        Log.Trace("Cipher.InverseTransform: input {0}",
                            BitConverter.ToString(ciphertext, 0, length));
                    }
                    System.Buffer.BlockCopy(ciphertext, length - DecryptingBlockSizeInBytes,
                        decryptingIV, 0, DecryptingBlockSizeInBytes);
                    cs.Write(ciphertext, 0, length);

#else
                    var buffers = new List<ArraySegment<byte>>();
                    buffer.ListStartingSegments(buffers, length);

                    // Store the last ciphertext block as a next decrypting IV.
                    byte[] nextIV = new byte[DecryptingBlockSizeInBytes];
                    int bytesCopied = 0;
                    for (var i = buffers.Count - 1; bytesCopied < DecryptingBlockSizeInBytes && i >= 0; --i)
                    {
                        var segment = buffers[i];
                        int bytesToCopy = Math.Min(segment.Count, DecryptingBlockSizeInBytes);
                        System.Buffer.BlockCopy(segment.Array, segment.Offset + segment.Count - bytesToCopy,
                            decryptingIV, DecryptingBlockSizeInBytes - bytesCopied - bytesToCopy, bytesToCopy);
                        bytesCopied += bytesToCopy;
                    }

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
#endif

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
            }

            return result;
        }
    }
}
