// Copyright (c) 2013-2015 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace x2
{
    /// <summary>
    /// Utility class for hash code generation.
    /// </summary>
    public partial class Hash
    {
        public const int Seed = 17;

        public int Code { get; private set; }

        public Hash()
            : this(Seed)
        {
        }

        public Hash(int seed)
        {
            Code = seed;
        }
    }
}
