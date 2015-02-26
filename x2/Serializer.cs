// Copyright (c) 2013-2015 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace x2
{
    /// <summary>
    /// Binary wire foramt serializer/deserializer.
    /// </summary>
    public sealed partial class Serializer
    {
        private Buffer stream;
        //private Stream stream;
        private long marker;

        /// <summary>
        /// Gets or sets the read boundary marker for deserialization.
        /// </summary>
        public long Marker
        {
            get { return marker; }
            set { marker = value; }
        }

        /// <summary>
        /// Initializes a new Serializer object that works on the specified
        /// stream.
        /// </summary>
        public Serializer(Buffer stream)
        //public Serializer(Stream stream)
        {
            this.stream = stream;
            marker = -1L;
        }

        private void CheckLengthToRead(int length)
        {
            long limit = marker < 0 ? stream.Length : marker;
            if ((stream.Position + length) > limit)
            {
                throw new EndOfStreamException();
            }
        }
    }
}
