// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace xpiler
{
    interface Handler
    {
        bool Handle(string path, out Document doc);
    }
}
