// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace x2
{
    /// <summary>
    /// Utility struct for hash code generation.
    /// </summary>
    /// Be aware that this struct is mutable.
    public partial struct Hash
    {
        public const int Seed = 17;

        public int Code;

        public Hash(int seed)
        {
            Code = seed;
        }
    }
}
