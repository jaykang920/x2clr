// Copyright (c) 2013-2015 Jae-jun Kang
// See the file LICENSE for details.

using System;
using System.IO;

namespace xpiler
{
    abstract class Formatter
    {
        public abstract string Description { get; }

        public abstract bool Format(Document doc, String outDir);

        public abstract bool IsUpToDate(string path, string outDir);
    }

    abstract class FormatterContext
    {
        public Document Doc { get; set; }
        public StreamWriter Out { get; set; }

        public abstract void FormatReference(Reference reference);
        public abstract void FormatConsts(ConstsDef def);
        public abstract void FormatCell(CellDef def);
    }
}
