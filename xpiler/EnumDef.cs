// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;

namespace xpiler
{
    class EnumDef : Definition
    {
        public class Element
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public List<Element> Elements { get { return elements; } }

        private readonly List<Element> elements = new List<Element>();

        public override void Format(FormatterContext context)
        {
            context.FormatEnum(this);
        }
    }
}
