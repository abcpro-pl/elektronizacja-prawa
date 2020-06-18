/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Abc.Nes.Generators {
    public class XsdGenerator : IDisposable {
        public XElement GetSchema(Type type = null, string rootTypeName = null) {
            if (type.IsNull()) { type = typeof(Document); }
            if (rootTypeName.IsNull()) { rootTypeName = "dokument-typ"; }
            var schemas = new XmlSchemas();
            var exporter = new XmlSchemaExporter(schemas);
            var importer = new XmlReflectionImporter();
            var mapping = importer.ImportTypeMapping(type);
            exporter.ExportTypeMapping(mapping);

            using (var schemaWriter = new StringWriter()) {
                foreach (System.Xml.Schema.XmlSchema schema in schemas) {
                    schema.Write(schemaWriter);
                }

                var xsdText = schemaWriter.ToString();
                xsdText = ChangeXsdText(xsdText);

                var xsd = XElement.Parse(xsdText);
                xsd = ChangeXsd(xsd, rootTypeName);

                return xsd;
            }
        }

        private string ChangeXsdText(string xsdText) {
            xsdText = xsdText.Replace("xmlns:tns", "xmlns:ndap");
            xsdText = xsdText.Replace("\"tns:", "\"ndap:");

            xsdText = xsdText.Replace("xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"", "xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\"");

            return xsdText;
        }
        private XElement ChangeXsd(XElement xsd, string rootTypeName) {
            using (var controller = new XsdCustomAttributesGenerator()) {
                xsd = controller.AddCustomAttributes(xsd, true);
            }

            xsd.AddFirst(new XElement(XName.Get("import", xsd.Name.NamespaceName),
               new XAttribute("namespace", "http://www.w3.org/2000/09/xmldsig#"),
               new XAttribute("schemaLocation", "http://www.w3.org/TR/xmldsig-core/xmldsig-core-schema.xsd")));


            var rootType = xsd.Elements().Where(x => x.Name.LocalName == "complexType" && x.Attribute("name").IsNotNull() && x.Attribute("name").Value == rootTypeName).FirstOrDefault();
            if (rootType.IsNotNull()) {
                rootType.Descendants().Where(x => x.Name.LocalName == "extension").ToList().ForEach(x => {
                    (x.FirstNode as XElement).Add(new XElement(XName.Get("element", x.Name.NamespaceName),
                        new XAttribute("ref", "ds:Signature"),
                        new XAttribute("minOccurs", "0"),
                        new XAttribute("maxOccurs", "unbounded")
                        ));
                });
            }

            return xsd;
        }

        public void WriteSchema(string filePath) {
            GetSchema().Save(filePath);
        }
        public void Dispose() { }
    }
}
