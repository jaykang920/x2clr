// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.IO;

namespace xpiler {
  /// <summary>
  /// Output file formatter interface.
  /// </summary>
  public abstract class Formatter {
    public abstract class Context {
      public Document Doc { get; set; }
      public StreamWriter Out { get; set; }

      public abstract void FormatEnum(EnumDef def);
      public abstract void FormatCell(CellDef def);
    }

    public abstract string Description { get; }

    public abstract bool Format(Document doc, String outDir);

    public abstract bool IsUpToDate(string path, string outDir);
  }
}
