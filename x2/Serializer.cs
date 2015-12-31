﻿// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace x2
{
    /// <summary>
    /// Binary wire foramt serializer.
    /// </summary>
    public sealed partial class Serializer
    {
        private Buffer buffer;

        /// <summary>
        /// Initializes a new Serializer object that works on the specified
        /// stream.
        /// </summary>
        public Serializer(Buffer buffer)
        {
            this.buffer = buffer;
        }
    }
}
