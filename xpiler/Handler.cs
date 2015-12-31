// Copyright (c) 2013-2016 Jae-jun Kang
// See the file LICENSE for details.

using System;

namespace xpiler
{
    /// <summary>
    /// Document file handler interface.
    /// </summary>
    interface Handler
    {
        bool Handle(string path, out Document doc);
    }
}
