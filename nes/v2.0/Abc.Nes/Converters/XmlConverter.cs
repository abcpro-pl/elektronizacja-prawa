/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes.Generators;
using Abc.Nes.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Abc.Nes.Converters {
    public class XmlConverter : IDisposable {
        public List<string> ValidationErrors { get; private set; }

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
                xmlText = UpdateToCurrentVersion(xmlText);
                using (var stream = xmlText.GetMemoryStream()) {
                    var serializer = new XmlSerializer(typeof(Document));
                    return serializer.Deserialize(stream) as Document;
                }
            }
            return default;
        }

        public Document ParseXml(XElement e) {
            if (e.IsNotNull()) {
                var xmlText = UpdateToCurrentVersion(e.ToString());
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

            var tmp = Path.Combine(Path.GetTempPath(), $"{string.Empty.GenerateId()}.xml");
            try {
                File.Copy(filePath, tmp, true);

                var xmlText = UpdateToCurrentVersion(File.ReadAllText(tmp));
                if (xmlText.IsNotNullOrEmpty()) { File.WriteAllText(tmp, xmlText); }

                Document result;
                var serializer = new XmlSerializer(typeof(Document));
                using (var reader = File.OpenText(tmp)) {
                    result = serializer.Deserialize(reader) as Document;
                }

                return result;
            }
            finally {
                try { File.Delete(tmp); } catch { }
            }
        }

        public bool Validate(string filePath) {
            if (String.IsNullOrEmpty(filePath)) { throw new ArgumentException(); }
            if (!File.Exists(filePath)) { throw new FileNotFoundException(); }

            return Validate(LoadXml(filePath));
        }

        public bool Validate(Document document) {
            if (document.IsNull()) { throw new ArgumentException(); }
            ValidationErrors = new List<string>();

            ValidateObject(document);

            //var properties = document.GetType().GetProperties();
            //foreach (var property in properties) {
            //    var rqAttr = property.GetCustomAttributes(typeof(XmlRequiredAttribute), false).FirstOrDefault() as XmlRequiredAttribute;
            //    if (rqAttr.IsNotNull() && rqAttr.Required) {
            //        var propVal = property.GetValue(document);
            //        if (propVal.IsNull()) {
            //            ValidationErrors.Add($"Wymagane pole '{property.Name}' nie posiada wartości!");
            //            continue;
            //        }
            //        if (propVal is ICollection) {
            //            if ((propVal as ICollection).Count == 0) {
            //                ValidationErrors.Add($"Wymagane pole listy '{property.Name}' nie posiada żadnych elementów!");
            //            }
            //        }
            //    }
            //}


            return ValidationErrors.Count == 0;
        }

        private void ValidateObject(object o) {
            if (o.GetType().GetCustomAttributes(typeof(XmlChoiceAttribute), false).FirstOrDefault().IsNotNull()) { return; }

            var properties = o.GetType().GetProperties();
            foreach (var property in properties) {
                var rqAttr = property.GetCustomAttributes(typeof(XmlRequiredAttribute), false).FirstOrDefault() as XmlRequiredAttribute;
                if (rqAttr.IsNotNull() && rqAttr.Required) {

                    var propVal = property.GetValue(o);
                    if (propVal.IsNull()) {
                        ValidationErrors.Add($"Required field '{property.Name}' of type '{o.GetType().FullName}' does not have a value!");
                        continue;
                    }
                    if (propVal is ICollection) {
                        if ((propVal as ICollection).Count == 0) {
                            ValidationErrors.Add($"Required list fiels '{property.Name}' of type '{o.GetType().FullName}' has no elements!");
                        }
                        else {
                            var collection = propVal as ICollection;
                            foreach (var item in collection) {
                                ValidateObject(item);
                            }
                        }
                    }
                    else {
                        ValidateObject(propVal); // rekurencyjnie
                    }
                }
            }

        }


        public bool ValidateWithSchema(string filePath) {
            if (String.IsNullOrEmpty(filePath)) { throw new ArgumentException(); }
            if (!File.Exists(filePath)) { throw new FileNotFoundException(); }

            ValidationErrors = new List<string>();

            var schemas = new XmlSchemaSet();
            TextReader reader = new StringReader(new XsdGenerator().GetSchema().ToString());
            var schema = XmlSchema.Read(reader, ValidationCallback);
            schemas.Add(schema);

            var document = XDocument.Load(filePath);
            document.Validate(schemas, ValidationCallback);

            return ValidationErrors.Count == 0;
        }

        void ValidationCallback(object sender, System.Xml.Schema.ValidationEventArgs args) {
            ValidationErrors.Add(args.Message);
        }

        public void Dispose() { }


        private string UpdateToCurrentVersion(string xmlText) {
            return new XmlUpdater().UpdateXmlText(xmlText);
        }
    }
}
