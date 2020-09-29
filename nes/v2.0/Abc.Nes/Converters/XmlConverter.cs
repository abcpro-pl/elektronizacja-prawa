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
using Abc.Nes.Validators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Abc.Nes.Converters {
    public interface IXmlConverter : IDisposable {
        XElement GetXml(IDocument doc);
        XElement WriteXml(IDocument doc, string filePath);
        IDocument ParseXml(string xmlText);
        IDocument ParseXml(XElement e);
        IDocument LoadXml(string filePath);

        List<string> ValidationErrors { get; }
        bool Validate(string filePath);
        bool Validate(IDocument document);
        IValidationResult GetValidationResult(string filePath);
        IValidationResult GetValidationResult(IDocument document);
        bool ValidateWithSchema(string filePath, Type typeOfDocument = null, string rootTypeName = null);
    }
    public class XmlConverter : IXmlConverter {
        public List<string> ValidationErrors { get; private set; }

        public XElement GetXml(IDocument doc) {
            var type = doc.GetType();
            var xmlserializer = new XmlSerializer(type);
            var stringWriter = new Utf8StringWriter();
            using (var writer = System.Xml.XmlWriter.Create(stringWriter, new System.Xml.XmlWriterSettings() {
                Encoding = Encoding.UTF8
            })) {
                xmlserializer.Serialize(writer, doc);
                var xmlText = stringWriter.ToString();
                return XElement.Parse(xmlText);
            }
        }

        public XElement WriteXml(IDocument doc, string filePath) {
            var xml = GetXml(doc);
            if (xml.IsNotNull()) {
                xml.Save(filePath);
            }
            return xml;
        }

        public IDocument ParseXml(string xmlText) {
            if (xmlText.IsNotNullOrEmpty()) {
                if (Document.InternalValidateXml(xmlText)) {
                    xmlText = UpdateToCurrentVersion(xmlText);
                    using (var stream = xmlText.GetMemoryStream()) {
                        var serializer = new XmlSerializer(typeof(Document));
                        return serializer.Deserialize(stream) as Document;
                    }
                }
                else if (Document17.InternalValidateXml(xmlText)) {
                    using (var stream = xmlText.GetMemoryStream()) {
                        var serializer = new XmlSerializer(typeof(Document17));
                        return serializer.Deserialize(stream) as Document17;
                    }
                }
            }
            return default;
        }

        public IDocument ParseXml(XElement e) {
            if (e.IsNotNull()) {
                if (Document.InternalValidateXml(e)) {
                    var xmlText = UpdateToCurrentVersion(e.ToString());
                    using (var stream = xmlText.GetMemoryStream()) {
                        var serializer = new XmlSerializer(typeof(Document));
                        return serializer.Deserialize(stream) as Document;
                    }
                }
                else if (Document17.InternalValidateXml(e)) {
                    var xmlText = e.ToString();
                    using (var stream = xmlText.GetMemoryStream()) {
                        var serializer = new XmlSerializer(typeof(Document17));
                        return serializer.Deserialize(stream) as Document17;
                    }
                }
            }
            return default;
        }

        public IDocument LoadXml(string filePath) {
            if (String.IsNullOrEmpty(filePath)) { throw new ArgumentException(); }
            if (!File.Exists(filePath)) { throw new FileNotFoundException(); }

            var tmp = Path.Combine(Path.GetTempPath(), $"{string.Empty.GenerateId()}.xml");
            try {
                File.Copy(filePath, tmp, true);
                var xmlText = File.ReadAllText(tmp);
                return ParseXml(xmlText);
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

        public bool Validate(IDocument document) {
            ValidationErrors = new List<string>();
            using (var validator = new DocumentValidator()) {
                var result = validator.Validate(document);
                ValidationErrors.AddRange(result.Select(x => x.DefaultMessage));
            }

            return ValidationErrors.Count == 0;
        }

        public IValidationResult GetValidationResult(string filePath) {
            if (String.IsNullOrEmpty(filePath)) { throw new ArgumentException(); }
            if (!File.Exists(filePath)) { throw new FileNotFoundException(); }

            return GetValidationResult(LoadXml(filePath));
        }
        public IValidationResult GetValidationResult(IDocument document) {
            ValidationErrors = new List<string>();
            using (var validator = new DocumentValidator()) {
                var result = validator.Validate(document);
                ValidationErrors.AddRange(result.Select(x => x.DefaultMessage));
                return result;
            }
        }

        public bool ValidateWithSchema(string filePath, Type typeOfDocument = null, string rootTypeName = null) {
            if (String.IsNullOrEmpty(filePath)) { throw new ArgumentException(); }
            if (!File.Exists(filePath)) { throw new FileNotFoundException(); }

            ValidationErrors = new List<string>();

            var schemas = new XmlSchemaSet();
            TextReader reader = new StringReader(new XsdGenerator().GetSchema(typeOfDocument, rootTypeName).ToString());
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
