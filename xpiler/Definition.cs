// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;

namespace xpiler {
  /// <summary>
  /// Represents a single abstract definition.
  /// </summary>
  public abstract class Definition {
    public string Name { get; set; }

    public abstract void Format(FormatterContext context);
  }
}
