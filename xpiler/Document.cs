// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;

namespace x2
{
    class Document
    {
        public string BaseName { get; set; }
        public string Namespace { get; set; }

        public IList<Definition> Definitions { get { return definitions; } }

        private readonly IList<Definition> definitions = new List<Definition>();
    }
}
