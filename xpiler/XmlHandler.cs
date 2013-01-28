// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Xml;

namespace x2.xpiler {
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
      doc.Namespace = rootElem.GetAttribute("namespace");

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
        Console.WriteLine(elem.Name);
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
      XmlElement element = (XmlElement)elem.FirstChild;
      for (; element != null; element = (XmlElement)element.NextSibling) {
        if (element.Name != "element") {
          continue;
        }
        name = element.GetAttribute("name");
        if (name == null) {
          continue;
        }
        EnumDef.Element e = new EnumDef.Element();
        e.Name = name;
        e.Value = element.InnerText;

        def.Elements.Add(e);
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
        //def.id = id;
      }

      XmlElement element = (XmlElement)elem.FirstChild;
      for (; element != null; element = (XmlElement)element.NextSibling) {
        if (element.Name != "element") {
          continue;
        }
        name = element.GetAttribute("name");
        if (name == null) {
          continue;
        }
        CellDef.Property e = new CellDef.Property();
        e.Name = name;
        e.DefaultValue = element.InnerText;

        def.Properties.Add(e);
      }
      doc.Definitions.Add(def);
      return true;
    }
  }
}
