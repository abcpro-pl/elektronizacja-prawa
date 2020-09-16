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

namespace System.Xml.Serialization {
    public class NesXmlSerializer : XmlSerializer {
        public DocumentType DocumentType { get; private set; }
        public NesXmlSerializer(Type type) : base(type) {
            this.UnknownElement += this.ElementSynonymHandler;
            this.UnknownAttribute += AttributeSynonymHandler;            
        }

        public NesXmlSerializer(Type type, XmlRootAttribute root) : base(type, root) {
            this.UnknownElement += this.ElementSynonymHandler;
            this.UnknownAttribute += AttributeSynonymHandler;            
        }        

        public new void Serialize(TextWriter textWriter, object o) {
            if (o is IDocument) { this.DocumentType = (o as IDocument).Type; }
            base.Serialize(textWriter, o);
        }

        protected void AttributeSynonymHandler(object sender, XmlAttributeEventArgs e) {
            var member = XmlSynonymsAttribute.GetMember(e.ObjectBeingDeserialized, e.Attr.Name);
            if (member.IsNull()) { return; }

            object value;
            var serializer = new XmlSerializer(member.PropertyType, new XmlRootAttribute(e.Attr.Name));
            using (var reader = new StringReader(e.Attr.OuterXml)) {
                value = serializer.Deserialize(reader);
            }
            member.SetValue(e.ObjectBeingDeserialized, value);
        }

        protected void ElementSynonymHandler(object sender, XmlElementEventArgs e) {
            var member = XmlSynonymsAttribute.GetMember(e.ObjectBeingDeserialized, e.Element.Name);
            if (member.IsNull()) { return; }

            object value;
            var serializer = new XmlSerializer(member.PropertyType, new XmlRootAttribute(e.Element.Name));
            using (var reader = new StringReader(e.Element.OuterXml)) {
                value = serializer.Deserialize(reader);
            }
            member.SetValue(e.ObjectBeingDeserialized, value);
        }
    }
}
