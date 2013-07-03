// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;

namespace xpiler {
  /// <summary>
  /// Represents a single cell definition.
  /// </summary>
  public class CellDef : Definition {
    /// <summary>
    /// Represents a single cell property.
    /// </summary>
    public class Property {
      public int Index { get; set; }
      public string Name { get; set; }
      public string Type { get; set; }
      public string Subtype { get; set; } //
      public string DefaultValue { get; set; }
      public string NativeName { get; set; }
      public string NativeType { get; set; }
    }

    public string Base { get; set; }

    public bool HasProperties {
      get { return (properties.Count != 0); }
    }

    public virtual bool IsEvent {
      get { return false; }
    }

    public List<Property> Properties {
      get { return properties; }
    }

    private readonly List<Property> properties = new List<Property>();

    public override void Format(FormatterContext context) {
      context.FormatCell(this);
    }
  }
}
