// Copyright (c) 2013-2015 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.Text;

namespace x2
{
    // Extensions.Buffer
    public static partial class Extensions
    {
        public static int ReadVariable(this Buffer self, out uint value)
        {
            return Serializer.ReadVariableInternal(self, out value);
        }
    }
}
