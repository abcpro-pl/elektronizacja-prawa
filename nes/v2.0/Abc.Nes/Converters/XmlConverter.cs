/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes.Utils;
using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Abc.Nes.Converters {
    public class XmlConverter : IDisposable {
        public XElement GetXml(Document doc) {
            var xmlserializer = new XmlSerializer(typeof(Document));
            var stringWriter = new Utf8StringWriter();
            using (var writer = System.Xml.XmlWriter.Create(stringWriter, new System.Xml.XmlWriterSettings() {
                Encoding = Encoding.UTF8
            })) {
                xmlserializer.Serialize(writer, doc);
                var xmlText = stringWriter.ToString();
                return XElement.Parse(xmlText);
            }
        }

        public XElement WriteXml(Document doc, string filePath) {
            var xml = GetXml(doc);
            if (xml.IsNotNull()) {
                xml.Save(filePath);
            }
            return xml;
        }

        public Document ParseXml(string xmlText) {
            if (xmlText.IsNotNullOrEmpty()) {
                using (var stream = xmlText.GetMemoryStream()) {
                    var serializer = new XmlSerializer(typeof(Document));
                    return serializer.Deserialize(stream) as Document;
                }
            }
            return default;
        }

        public Document LoadXml(string filePath) {
            if (String.IsNullOrEmpty(filePath)) { throw new ArgumentException(); }
            if (!File.Exists(filePath)) { throw new FileNotFoundException(); }

            Document result;
            var serializer = new XmlSerializer(typeof(Document));
            using (var reader = File.OpenText(filePath)) {
                result = serializer.Deserialize(reader) as Document;
            }

            return result;
        }


        public void Dispose() { }
    }
}
