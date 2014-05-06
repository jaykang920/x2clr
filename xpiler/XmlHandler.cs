// Copyright (c) 2013, 2014 Jae-jun Kang
// See the file COPYING for license details.

using System;
using System.Xml;

namespace x2
{
    class XmlHandler : InputHandler
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
                    case "ref":
                        if (ParseReference(doc, elem) == false)
                        {
                            return false;
                        }
                        break;
                    case "consts":
                        if (ParseConsts(doc, elem) == false)
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

        private bool ParseReference(Document doc, XmlElement elem)
        {
            var target = elem.GetAttribute("target");
            if (String.IsNullOrEmpty(target))
            {
                return false;
            }
            Reference reference = new Reference();
            reference.Target = target;
            doc.References.Add(reference);
            return true;
        }

        private bool ParseConsts(Document doc, XmlElement elem)
        {
            var name = elem.GetAttribute("name");
            var type = elem.GetAttribute("type");
            if (String.IsNullOrEmpty(name))
            {
                return false;
            }
            if (String.IsNullOrEmpty(type))
            {
                type = "int32";  // default type
            }
            var def = new ConstsDef();
            def.Name = name;
            def.Type = type;

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
                    case "const":
                        if (ParseConstant(def, child) == false)
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

        private bool ParseConstant(ConstsDef def, XmlElement elem)
        {
            var name = elem.GetAttribute("name");
            if (String.IsNullOrEmpty(name))
            {
                return false;
            }
            var element = new ConstsDef.Constant();
            element.Name = name;
            element.Value = elem.InnerText.Trim();
            def.Constants.Add(element);
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
            property.DefaultValue = elem.InnerText.Trim();
            def.Properties.Add(property);

            property.TypeSpec = Types.Parse(type);
            if (property.TypeSpec == null)
            {
                return false;
            }

            return true;
        }
    }
}
