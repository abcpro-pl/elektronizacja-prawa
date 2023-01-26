/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes;
using Abc.Nes.Enumerations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace System.Xml.Serialization {
    public class NesXmlSerializer : XmlSerializer {
        public DocumentType DocumentType { get; private set; }
        public XElement Xml { get; private set; }
        public NesXmlSerializer(Type type) : base(type) {
            this.UnknownElement += this.ElementSynonymHandler;
            this.UnknownAttribute += AttributeSynonymHandler;
        }

        public NesXmlSerializer(Type type, XmlRootAttribute root) : base(type, root) {
            this.UnknownElement += this.ElementSynonymHandler;
            this.UnknownAttribute += AttributeSynonymHandler;
        }

        public void Serialize(XmlWriter xmlWriter, StringWriter stringWriter, object o) {
            if (o is Document) {
                DocumentType = DocumentType.Nes20;
            }
            else if (o is Document17) {
                DocumentType = DocumentType.Nes17;
            }
            else if (o is Document16) {
                DocumentType = DocumentType.Nes16;
            }

            base.Serialize(xmlWriter, o);
            if (stringWriter.IsNotNull()) {
                var xmlText = stringWriter.ToString();
                Xml = XElement.Parse(xmlText);
                ProcessSynonyms(Xml);
            }
        }

        private void ProcessSynonyms(XElement xml) {
            foreach (var e in xml.Elements()) {
                var attr = e.Attribute("ElementTypeFullName").Value();
                if (attr.IsNotNullOrEmpty()) {
                    var type = Type.GetType(attr);
                    if (type.IsNotNull()) {
                        foreach (var property in type.GetProperties()) {
                            var synonyms = property.GetCustomAttributes(typeof(XmlSynonymsAttribute), true) as XmlSynonymsAttribute[];
                            if (synonyms.IsNotNull() && synonyms.Length > 0) {
                                var synonym = synonyms.Where(x => x.Synonym.Type == this.DocumentType).FirstOrDefault();
                                if (synonym.IsNotNull()) {
                                    var propName = property.Name;
                                    var elemName = property.GetCustomAttribute(typeof(XmlElementAttribute), true) as XmlElementAttribute;
                                    if (elemName.IsNotNull()) { propName = elemName.ElementName; }

                                    var propXml = e.Elements().Where(x => x.Name.LocalName == propName).FirstOrDefault();
                                    if (propXml.IsNotNull()) {
                                        propXml.Name = XName.Get(synonym.Synonym.Name, propXml.Name.NamespaceName);
                                    }
                                    else {
                                        var attrName = property.GetCustomAttribute(typeof(XmlAttributeAttribute), true) as XmlAttributeAttribute;
                                        if (attrName.IsNotNull()) { propName = attrName.AttributeName; }

                                        var attrXml = e.Attributes().Where(x => x.Name.LocalName == propName).FirstOrDefault();
                                        if (attrXml.IsNotNull()) {
                                            e.Add(new XAttribute(synonym.Synonym.Name, attrXml.Value));
                                            attrXml.Remove();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    e.Attribute("ElementTypeFullName").Remove();
                }

                ProcessSynonyms(e);
            }
        }

        protected void AttributeSynonymHandler(object sender, XmlAttributeEventArgs e) {
            var member = XmlSynonymsAttribute.GetMember(e.ObjectBeingDeserialized, e.Attr.LocalName);
            if (member.IsNull()) { return; }

            if (member.PropertyType.FullName == "System.String") {
                member.SetValue(e.ObjectBeingDeserialized, e.Attr.Value);
            }
            else if (member.PropertyType.IsEnum) {
                object o = Enum.Parse(member.PropertyType, e.Attr.Value);
                member.SetValue(e.ObjectBeingDeserialized, o);
            }
            else if (member.PropertyType.FullName == "System.Int32") {
                object o = e.Attr.Value.ToInt();
                member.SetValue(e.ObjectBeingDeserialized, o);
            }
            else if (member.PropertyType.FullName == "System.Double") {
                object o = e.Attr.Value.ToDouble();
                member.SetValue(e.ObjectBeingDeserialized, o);
            }
            else {
                object value;
                var serializer = new XmlSerializer(member.PropertyType, new XmlRootAttribute(e.Attr.LocalName));
                using (var reader = new StringReader(e.Attr.OuterXml)) {
                    value = serializer.Deserialize(reader);
                }
                member.SetValue(e.ObjectBeingDeserialized, value);
            }
        }

        protected void ElementSynonymHandler(object sender, XmlElementEventArgs e) {
            PropertyInfo property = XmlSynonymsAttribute.GetMember(e.ObjectBeingDeserialized, e.Element.LocalName);
            if (property.IsNull() || !property.CanWrite) { return; }

            object value = e.Element.InnerText;
            object objectBeingDeserialized = e.ObjectBeingDeserialized;

            if (property.PropertyType.FullName == "System.String") {
                property.SetValue(objectBeingDeserialized, value);
            }
            else if (property.PropertyType.IsEnum) {
                var o = Enum.Parse(property.PropertyType, e.Element.Value);
                property.SetValue(objectBeingDeserialized, o);
            }
            else if (property.PropertyType.FullName == "System.Int32") {
                var o = e.Element.Value.ToInt();
                property.SetValue(objectBeingDeserialized, o);
            }
            else if (property.PropertyType.FullName == "System.Double") {
                var o = e.Element.Value.ToDouble();
                property.SetValue(objectBeingDeserialized, o);
            }
            else {
                var serializer = new XmlSerializer(property.PropertyType, new XmlRootAttribute(e.Element.LocalName));
                using (var reader = new StringReader(e.Element.OuterXml)) {
                    value = serializer.Deserialize(reader);
                }
                property.SetValue(e.ObjectBeingDeserialized, value);
            }
        }
    }
}
