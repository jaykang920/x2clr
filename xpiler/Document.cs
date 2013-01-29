// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Collections.Generic;
using System.IO;

namespace xpiler {
  abstract class Definition {
    public const int EnumType = 0;
    public const int CellType = 1;
    public const int EventType = 2;

    private string name;

    public string Name {
      get { return name; }
      set { name = value; }
    }

    public abstract int Type { get; }

    public Definition() {
      name = null;
    }
  }

  class EnumDef : Definition {
    public class Element {
      private string name;
      private string value;

      public string Name {
        get { return name; }
        set { name = value; }
      }

      public string Value {
        get { return value; }
        set { this.value = value; }
      }

      public Element() {
        name = null;
        value = null;
      }
    }

    private readonly List<Element> elements;

    public List<Element> Elements {
      get { return elements; }
    }

    public override int Type {
      get { return EnumType; }
    }

    public EnumDef() {
      elements = new List<Element>();
    }
  }

  class CellDef : Definition {
    public class Property {
      private int index;
      private string name;
      private string type;
      private string defaultValue;

      private string nativeName;
      private string nativeType;

      public int Index {
        get { return index; }
        set { index = value; }
      }

      public string Name {
        get { return name; }
        set { name = value; }
      }

      public string Type {
        get { return type; }
        set { type = value; }
      }

      public string DefaultValue {
        get { return defaultValue; }
        set { defaultValue = value; }
      }

      public string NativeName {
        get { return nativeName; }
        set { nativeName = value; }
      }

      public string NativeType {
        get { return nativeType; }
        set { nativeType = value; }
      }

      public Property() {
        name = null;
        type = null;
        defaultValue = null;

        nativeName = null;
        nativeType = null;
      }
    }

    private readonly List<Property> properties;
    private string inheritee;

    public List<Property> Properties {
      get { return properties; }
    }

    public string Inheritee {
      get { return inheritee; }
      set { inheritee = value; }
    }

    public override int Type {
      get { return CellType; }
    }

    public CellDef() {
      properties = new List<Property>();
      inheritee = null;
    }
  }

  class EventDef : CellDef {
    private string id;

    public string Id {
      get { return id; }
      set { id = value; }
    }

    public override int Type {
      get { return EventType; }
    }

    public EventDef() {
      id = null;
    }
  }

  class Document {
    private readonly List<Definition> definitions;
    private string[] namespaces;

    private string path;
    private string dirName;
    private string baseName;

    public List<Definition> Definitions {
      get { return definitions; }
    }

    public string[] Namespaces {
      get { return namespaces; }
      set { namespaces = value; }
    }

    public string Path {
      get { return path; }
      set {
        path = value;
        dirName = System.IO.Path.GetDirectoryName(path);
        baseName = System.IO.Path.GetFileNameWithoutExtension(path);
      }
    }

    public string DirName {
      get { return dirName; }
    }

    public string BaseName {
      get { return baseName; }
    }

    public Document() {
      definitions = new List<Definition>();
      namespaces = null;

      path = null;
      dirName = null;
      baseName = null;
    }
  }
}
