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
        return true;
      }
      doc = new Document();
      string @namespace = rootElem.GetAttribute("namespace");
      if (@namespace != null) {
        doc.Namespaces = @namespace.Split('/');
      }

      XmlNode node = rootElem.FirstChild;
      for ( ; node != null; node = node.NextSibling) {
        if (node.NodeType != XmlNodeType.Element) {
          continue;
        }
        XmlElement elem = (XmlElement)node;
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
      if (String.IsNullOrEmpty(name)) {
        return false;
      }
      EnumDef def = new EnumDef();
      def.Name = name;

      XmlNode node = elem.FirstChild;
      for ( ; node != null; node = node.NextSibling) {
        if (node.NodeType != XmlNodeType.Element) {
          continue;
        }
        XmlElement child = (XmlElement)node;
        if (child.Name != "element") {
          continue;
        }
        name = child.GetAttribute("name");
        if (String.IsNullOrEmpty(name)) {
          return false;
        }
        EnumDef.Element element = new EnumDef.Element();
        element.Name = name;
        element.Value = child.InnerText.Trim();
        def.Elements.Add(element);
      }
      doc.Definitions.Add(def);
      return true;
    }

    private bool ParseCell(Document doc, XmlElement elem) {
      string name = elem.GetAttribute("name");
      if (String.IsNullOrEmpty(name)) {
        return false;
      }
      bool isEvent = (elem.Name == "event");
      string id = elem.GetAttribute("id");
      if (isEvent && String.IsNullOrEmpty(id)) {
        return false;
      }
      CellDef def = (isEvent ? new EventDef() : new CellDef());
      def.Name = name;
      if (isEvent) {
        ((EventDef)def).Id = id;
      }
      def.Base = elem.GetAttribute("extends");

      XmlNode node = elem.FirstChild;
      for ( ; node != null; node = node.NextSibling) {
        if (node.NodeType != XmlNodeType.Element) {
          continue;
        }
        XmlElement child = (XmlElement)node;
        if (child.Name != "property") {
          continue;
        }
        name = child.GetAttribute("name");
        string type = child.GetAttribute("type");
        if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(type)) {
          return false;
        }
        CellDef.Property property = new CellDef.Property();
        property.Name = name;
        property.Type = type;
        property.Subtype = child.GetAttribute("subtype");
        property.DefaultValue = child.InnerText.Trim();
        def.Properties.Add(property);
      }
      doc.Definitions.Add(def);
      return true;
    }
  }
}
