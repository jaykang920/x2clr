// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace xpiler
{
    interface Handler
    {
        bool Handle(string path, out Document doc);
    }
}
