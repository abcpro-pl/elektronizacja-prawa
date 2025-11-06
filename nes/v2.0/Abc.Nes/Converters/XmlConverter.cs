/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes.Exceptions;
using Abc.Nes.Generators;
using Abc.Nes.Utils;
using Abc.Nes.Validators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
        bool Validate(IDocument document, string filePath);
        IValidationResult GetValidationResult(string filePath);
        IValidationResult GetValidationResult(IDocument document, string filePath);
        bool ValidateWithSchema(string filePath, Type typeOfDocument = null, string rootTypeName = null);
    }
    public class XmlConverter : IXmlConverter {
        public List<string> ValidationErrors { get; private set; }

        public XElement GetXml(IDocument doc) {
            var type = doc.GetType();
            var xmlserializer = new NesXmlSerializer(type);
            var stringWriter = new Utf8StringWriter();
            using (var writer = System.Xml.XmlWriter.Create(stringWriter, new System.Xml.XmlWriterSettings() {
                Encoding = Encoding.UTF8
            })) {
                xmlserializer.Serialize(writer, stringWriter, doc);
               
                XNamespace xsiNs = "http://www.w3.org/2001/XMLSchema-instance";
                if (doc.DocumentType == Enumerations.DocumentType.Nes17) {
                    xmlserializer.Xml.Add(new XAttribute(xsiNs + "schemaLocation", "http://www.mswia.gov.pl/standardy/ndap Metadane-1.7.xsd"));
                }
                else if (doc.DocumentType == Enumerations.DocumentType.Nes16) {
                    xmlserializer.Xml.Add(new XAttribute(xsiNs + "schemaLocation", "http://www.mswia.gov.pl/standardy/ndap Metadane-1.6.xsd"));
                }
                else if (doc.DocumentType == Enumerations.DocumentType.Nes20) {
                    xmlserializer. Xml.Add(new XAttribute(xsiNs + "schemaLocation", "http://www.mswia.gov.pl/standardy/ndap Metadane-2.0.xsd"));
                }
                return xmlserializer.Xml;
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
                IDocument doc;
                Exception doc20loadException;
                Exception doc16loadException;
                Exception doc17loadException;
                if (Document.InternalValidateXml(xmlText)) {
                    xmlText = UpdateToCurrentVersion(xmlText);
                    using (var stream = xmlText.GetMemoryStream()) {
                        var serializer = new NesXmlSerializer(typeof(Document));
                        return serializer.Deserialize(stream) as Document;
                    }
                }
                else if (Document17.InternalValidateXml(xmlText)) {
                    using (var stream = xmlText.GetMemoryStream()) {
                        var serializer = new NesXmlSerializer(typeof(Document17));
                        return serializer.Deserialize(stream) as Document17;
                    }
                }
                else if (Document16.InternalValidateXml(xmlText)) {
                    using (var stream = xmlText.GetMemoryStream()) {
                        var serializer = new NesXmlSerializer(typeof(Document16));
                        return serializer.Deserialize(stream) as Document16;
                    }
                }
                else if (Document.TryLoadXml(xmlText, out doc, out doc20loadException)) {
                    return doc;
                }
                else if(Document17.TryLoadXml(xmlText, out doc, out doc17loadException)) {
                    return doc;
                }
                else if (Document16.TryLoadXml(xmlText, out doc, out doc16loadException)) {
                    return doc;
                }
                //failed to load document xml
                var sb = new StringBuilder();
                if (doc20loadException != null || doc17loadException != null || doc16loadException != null) {
                    //sb.Append("Treść xml nie pasuje do schematu dokumentu");
                }
                string err20 = string.Empty, err17 = string.Empty, err16 = string.Empty;
                if (doc20loadException != null) {
                    err20 = HandleDocumentLoadException(doc20loadException, "(schemat w wersji 2.0)");
                    sb.AppendLine(err20);
                }
                if (doc17loadException != null) {
                    err17 = HandleDocumentLoadException(doc17loadException);
                }
                if(doc16loadException != null) {
                    err16 = HandleDocumentLoadException(doc16loadException);
                }
                if(err17 == err16 && err17 != string.Empty) {
                    err17 += "(schemat w wersji 1.7 i 1.6)";
                    sb.AppendLine(err17);
                } else {
                    if (err17 != string.Empty) {
                        err17 += "(schemat w wersji 1.7)";
                        sb.AppendLine(err17);
                    }
                    if (err16 != string.Empty) {
                        err16 += "(schemat w wersji 1.6)";
                        sb.AppendLine(err16);
                    }
                }

                if (sb.Length > 0) {
                    throw new DocumentSchemaException(sb.ToString());
                }
            }
            return default;
        }

        private static string HandleDocumentLoadException(Exception exception, string schemaName = "") {
            bool xmlException = exception.Message.Contains("There is an error in XML document");
            if (xmlException) {
                if (exception.InnerException != null) {
                    //Instance validation error: 'daty skrajne' is not a valid value for EventDateType.
                    bool instanceValidError = exception.InnerException.Message.Contains("Instance validation error:");
                    bool invalidValueError = exception.InnerException.Message.Contains("is not a valid value for");
                    if (instanceValidError && invalidValueError) {
                        var regex = new Regex(@"Instance validation error: (?<value>.*) is not a valid value for (?<element>.*)\.");
                        var match = regex.Match(exception.InnerException.Message);

                        if (match.Groups["value"].Success && match.Groups["element"].Success) {
                            
                            
                            //sb.Append(match.Groups["value"]);
                            //sb.Append(" nie pasuje do elementu: ");
                            var ttt = Type.GetType("Abc.Nes.Enumerations." + match.Groups["element"].Value);
                            var attr = ttt.GetCustomAttribute<XmlTypeAttribute>();
                            //sb.Append(attr.TypeName);

                            var errorStr = $"Wartość: {match.Groups["value"]} nie jest zgodny z elementem: <{attr.TypeName}> {schemaName}";
                            return errorStr;
                        }
                    }
                }
            }

            /*
             * Plik: sprawy/BS.6740.340.2024.xml
Składnia pliku <nazwa_pliku>.xml z folderu <nazwa_folderu> nie jest zgodna z żadnym z wspieranych / obowiązujących schematów metadanych. 
Wartość: ‘daty skrajne’ nie jest zgodny z elementem < data-zdarzenia-typ>  (schemat w wersji 2.0)
Wartość: 'niepubliczny' nie jest zgodny z elementem <dostepnosc-typ> (schemat 1.6 i 1.7)
Brak możliwości wyświetlenia paczki niezgodnej z obowiązującymi schematami. 
             */
            return string.Empty;
        }

        public IDocument ParseXml(XElement e) {
            if (e.IsNotNull()) {
                if (Document.InternalValidateXml(e)) {
                    var xmlText = UpdateToCurrentVersion(e.ToString());
                    using (var stream = xmlText.GetMemoryStream()) {
                        var serializer = new NesXmlSerializer(typeof(Document));
                        return serializer.Deserialize(stream) as Document;
                    }
                }
                else if (Document16.InternalValidateXml(e)) {
                    var xmlText = e.ToString();
                    using (var stream = xmlText.GetMemoryStream()) {
                        var serializer = new NesXmlSerializer(typeof(Document16));
                        return serializer.Deserialize(stream) as Document16;
                    }
                }
                else if (Document17.InternalValidateXml(e)) {
                    var xmlText = e.ToString();
                    using (var stream = xmlText.GetMemoryStream()) {
                        var serializer = new NesXmlSerializer(typeof(Document17));
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

            return Validate(LoadXml(filePath), filePath);
        }

        public bool Validate(IDocument document, string filePath) {
            ValidationErrors = new List<string>();
            using (var validator = new DocumentValidator()) {
                var result = validator.Validate(document, filePath);
                ValidationErrors.AddRange(result.Select(x => x.DefaultMessage));
            }

            return ValidationErrors.Count == 0;
        }

        public IValidationResult GetValidationResult(string filePath) {
            if (String.IsNullOrEmpty(filePath)) { throw new ArgumentException(); }
            if (!File.Exists(filePath)) { throw new FileNotFoundException(); }

            return GetValidationResult(LoadXml(filePath), filePath);
        }
        public IValidationResult GetValidationResult(IDocument document, string filePath) {
            ValidationErrors = new List<string>();
            using (var validator = new DocumentValidator()) {
                var result = validator.Validate(document, filePath);
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
