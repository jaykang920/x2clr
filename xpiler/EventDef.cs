// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace xpiler
{
    class EventDef : CellDef
    {
        public string Id { get; set; }

        public override bool IsEvent { get { return true; } }
    }
}
