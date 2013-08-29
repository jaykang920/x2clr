// Copyright (c) 2013 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Xml;

namespace xpiler
{
    class XmlHandler : Handler
    {
        public bool Handle(string path, out Document doc)
        {
            doc = null;
            var xml = new XmlDocument();
            try
            {
                xml.Load(path);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return false;
            }

            var rootElem = xml.DocumentElement;
            if (rootElem.Name != "x2")
            {
                // Not a valid x2 document.
                return true;
            }
            doc = new Document();
            doc.Namespace = rootElem.GetAttribute("namespace");

            var node = rootElem.FirstChild;
            for ( ; node != null; node = node.NextSibling)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                var elem = (XmlElement)node;
                switch (elem.Name)
                {
                    case "enum":
                        if (ParseEnum(doc, elem) == false)
                        {
                            return false;
                        }
                        break;
                    case "cell":
                    case "event":
                        if (ParseCell(doc, elem) == false)
                        {
                            return false;
                        }
                        break;
                    default:
                        break;
                }
            }
            return true;
        }

        private bool ParseEnum(Document doc, XmlElement elem)
        {
            var name = elem.GetAttribute("name");
            if (String.IsNullOrEmpty(name))
            {
                return false;
            }
            var def = new EnumDef();
            def.Name = name;

            var node = elem.FirstChild;
            for ( ; node != null; node = node.NextSibling)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                var child = (XmlElement)node;
                if (child.IsEmpty)
                {
                    continue;
                }
                switch (child.Name)
                {
                    case "element":
                        if (ParseEnumElement(def, child) == false)
                        {
                            return false;
                        }
                        break;
                    default:
                        break;
                }
            }
            doc.Definitions.Add(def);
            return true;
        }

        private bool ParseEnumElement(EnumDef def, XmlElement elem)
        {
            var name = elem.GetAttribute("name");
            if (String.IsNullOrEmpty(name))
            {
                return false;
            }
            var element = new EnumDef.Element();
            element.Name = name;
            element.Value = elem.InnerText.Trim();
            def.Elements.Add(element);
            return true;
        }

        private bool ParseCell(Document doc, XmlElement elem)
        {
            var name = elem.GetAttribute("name");
            if (String.IsNullOrEmpty(name))
            {
                return false;
            }
            var isEvent = (elem.Name == "event");
            var id = elem.GetAttribute("id");
            if (isEvent && String.IsNullOrEmpty(id))
            {
                return false;
            }
            CellDef def = (isEvent ? new EventDef() : new CellDef());
            def.Name = name;
            if (isEvent)
            {
                ((EventDef)def).Id = id;
            }
            def.Base = elem.GetAttribute("extends");

            var node = elem.FirstChild;
            for ( ; node != null; node = node.NextSibling)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    continue;
                }
                var child = (XmlElement)node;
                switch (child.Name)
                {
                    case "property":
                        if (ParseCellProperty(def, child) == false)
                        {
                            return false;
                        }
                        break;
                    default:
                        break;
                }
            }
            doc.Definitions.Add(def);
            return true;
        }

        private bool ParseCellProperty(CellDef def, XmlElement elem)
        {
            var name = elem.GetAttribute("name");
            var type = elem.GetAttribute("type");
            if (String.IsNullOrEmpty(name))
            {
                return false;
            }
            if (String.IsNullOrEmpty(type))
            {
                return false;
            }
            var property = new CellDef.Property();
            property.Name = name;
            property.Type = type;
            property.Subtype = elem.GetAttribute("subtype");
            property.DefaultValue = elem.InnerText.Trim();
            def.Properties.Add(property);
            return true;
        }
    }
}
