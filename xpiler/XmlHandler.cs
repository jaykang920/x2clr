// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Xml;

namespace xpiler {
  class XmlHandler : IHandler {
    public bool Handle(string path, out Document doc) {
      doc = null;
      XmlDocument xml = new XmlDocument();
      try {
        xml.Load(path);
      } catch (Exception e) {
        Console.Error.WriteLine(e.Message);
        return false;
      }

      XmlElement rootElem = xml.DocumentElement;
      if (rootElem.Name != "x2") {
        return false;
      }
      doc = new Document();
      string @namespace = rootElem.GetAttribute("namespace");
      if (@namespace != null) {
        doc.Namespaces = @namespace.Split('/');
      }

      XmlElement elem = (XmlElement)rootElem.FirstChild;
      for ( ; elem != null; elem = (XmlElement)elem.NextSibling) {
        if (elem.IsEmpty) {
          continue;
        }
        switch (elem.Name) {
          case "enum":
            if (ParseEnum(doc, elem) == false) {
              return false;
            }
            break;
          case "cell":
          case "event":
            if (ParseCell(doc, elem) == false) {
              return false;
            }
            break;
        }
      }
      return true;
    }

    private bool ParseEnum(Document doc, XmlElement elem) {
      string name = elem.GetAttribute("name");
      if (name == null) {
        return false;
      }
      EnumDef def = new EnumDef();
      def.Name = name;

      XmlElement node = (XmlElement)elem.FirstChild;
      for ( ; node != null; node = (XmlElement)node.NextSibling) {
        if (node.Name != "element") {
          continue;
        }
        name = node.GetAttribute("name");
        if (name == null) {
          return false;
        }
        EnumDef.Element element = new EnumDef.Element();
        element.Name = name;
        element.Value = node.InnerText.Trim();
        def.Elements.Add(element);
      }
      doc.Definitions.Add(def);
      return true;
    }

    private bool ParseCell(Document doc, XmlElement elem) {
      string name = elem.GetAttribute("name");
      if (name == null) {
        return false;
      }
      bool isEvent = (elem.Name == "event");
      string id = elem.GetAttribute("id");
      if (isEvent && id == null) {
        return false;
      }
      CellDef def = (isEvent ? new EventDef() : new CellDef());
      def.Name = name;
      if (isEvent) {
        ((EventDef)def).Id = id;
      }
      def.Inheritee = elem.GetAttribute("extends");

      XmlElement node = (XmlElement)elem.FirstChild;
      for ( ; node != null; node = (XmlElement)node.NextSibling) {
        if (node.Name != "property") {
          continue;
        }
        name = node.GetAttribute("name");
        string type = node.GetAttribute("type");
        if (name == null || type == null) {
          return false;
        }
        CellDef.Property property = new CellDef.Property();
        property.Name = name;
        property.Type = type;
        property.DefaultValue = node.InnerText.Trim();
        def.Properties.Add(property);
      }
      doc.Definitions.Add(def);
      return true;
    }
  }
}
