// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;

namespace x2
{
    abstract class Definition
    {
        public string Name { get; set; }

        public abstract void Format(FormatterContext context);
    }

    class ConstsDef : Definition
    {
        public class Constant
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public string Type { get; set; }
        public string NativeType { get; set; }
        public List<Constant> Constants { get { return constants; } }

        private readonly List<Constant> constants = new List<Constant>();

        public override void Format(FormatterContext context)
        {
            context.FormatConsts(this);
        }
    }

    class CellDef : Definition
    {
        public class Property
        {
            public int Index { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Subtype { get; set; }
            public string DefaultValue { get; set; }
            public string NativeName { get; set; }
            public string NativeType { get; set; }

            public TypeSpec TypeSpec { get; set; }
        }

        public string Base { get; set; }
        public string BaseClass { get; set; }
        public virtual bool IsEvent { get { return false; } }
        public List<Property> Properties { get { return properties; } }
        public bool HasProperties { get { return (properties.Count != 0); } }

        private readonly List<Property> properties = new List<Property>();

        public override void Format(FormatterContext context)
        {
            context.FormatCell(this);
        }
    }

    class EventDef : CellDef
    {
        public string Id { get; set; }

        public override bool IsEvent { get { return true; } }
    }
}
