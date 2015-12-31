// Copyright (c) 2013-2016 Jae-jun Kang
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
        /// <summary>
        /// Default hash seed value.
        /// </summary>
        public const int Seed = 17;

        /// <summary>
        /// The hash code value in this instance.
        /// </summary>
        public int Code;

        /// <summary>
        /// Initializes a new instance of the Hash structure with the specified
        /// seed value.
        /// </summary>
        public Hash(int seed)
        {
            Code = seed;
        }
    }
}
