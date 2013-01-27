// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Xml;

namespace x2.xpiler {
  class XmlHandler : IHandler {
    public Document Handle(string path) {
      XmlDocument xml = new XmlDocument();
      try {
        xml.Load(path);
      } catch (Exception e) {
        Console.Error.WriteLine(e.Message);
        return null;
      }

      Document doc = new Document(path);

      XmlElement docElem = xml.DocumentElement;
      Console.WriteLine(docElem.Name);
      XmlElement rootElem = (XmlElement)docElem.FirstChild;
      while (rootElem != null && rootElem.IsEmpty == false) {
        Console.WriteLine(rootElem.Name);
        rootElem = (XmlElement)rootElem.NextSibling;
      }

      return null;
    }
  }
}
