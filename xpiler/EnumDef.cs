// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;

namespace xpiler {
  /// <summary>
  /// Represents a single enumeration set definition.
  /// </summary>
  public class EnumDef : Definition {
    /// <summary>
    /// Represents a single enumeration element.
    /// </summary>
    public class Element {
      public string Name { get; set; }
      public string Value { get; set; }
    }

    public List<Element> Elements {
      get { return elements; }
    }

    private readonly List<Element> elements = new List<Element>();

    public override void Format(Formatter.Context context) {
      context.FormatEnum(this);
    }
  }
}
