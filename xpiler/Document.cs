// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;

namespace xpiler {
  /// <summary>
  /// Represents a single definition document.
  /// </summary>
  public class Document {
    public string BaseName { get; set; }
    public string Namespace { get; set; }

    public List<Definition> Definitions {
      get { return definitions; }
    }

    private readonly List<Definition> definitions = new List<Definition>();
  }
}
