// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;

namespace x2.xpiler {
  abstract class Def {
    public const int Namespace = 0;
    public const int EnumType = 1;
    public const int CellType = 2;
    public const int EventType = 3;

    public string name;

    public abstract int Type { get; }

    public Def() {
      name = null;
    }
  }

  class Namespace : Def {
    public bool opening;

    public override int Type {
      get { return Namespace; }
    }

    public Namespace() {
      opening = false;
    }
  }

  class EnumDef : Def {
    public class Element {
      public string name;
      public string value;

      public Element() {
        name = null;
        value = null;
      }
    }

    public readonly List<Element> elements;

    public override int Type {
      get { return EnumType; }
    }

    public EnumDef() {
      elements = new List<Element>();
    }
  }

  class CellDef : Def {
    public class Property {
      public string name;
      public string type;
      public string defaultValue;

      public string nativeName;
      public string nativeType;

      public Property() {
        name = null;
        type = null;
        defaultValue = null;

        nativeName = null;
        nativeType = null;
      }
    }

    public string inheritee;
    public readonly List<Property> properties;

    public override int Type {
      get { return CellType; }
    }

    public CellDef() {
      inheritee = null;
      properties = new List<Property>();
    }
  }

  class EventDef : CellDef {
    public string id;

    public override int Type {
      get { return EventType; }
    }

    public EventDef() {
      id = null;
    }
  }

  class Document {
    public readonly List<Def> defs;

    public string path;
    public string dirname;
    public string basename;
    public string rootNamespace;

    public Document(string path) {
      defs = new List<Def>();

      this.path = path;
      dirname = null;
      basename = null;
      rootNamespace = null;
    }
  }
}
