/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes.Enumerations;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Xml.Serialization {
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class XmlSynonymsAttribute : Attribute {
        public XmlSynonym Synonym { get; }
        private XmlSynonymsAttribute() { }
        public XmlSynonymsAttribute(string name, DocumentType type = DocumentType.Nes17) : this() {
            Synonym = new XmlSynonym(name, type);
        }

        public static IEnumerable<XmlSynonym> GetXmlSynonyms(object obj) {
            Type type = obj.GetType();

            var attributes = type.GetCustomAttributes(typeof(XmlSynonymsAttribute), true);
            if (attributes.IsNotNull() && attributes.Length > 0) {
                return attributes.Cast<XmlSynonymsAttribute>().Select(x => x.Synonym);
            }

            return null;
        }

        public static PropertyInfo GetMember(object obj, string name) {
            Type type = obj.GetType();
            foreach (var member in type.GetProperties()) {
                var attributes = member.GetCustomAttributes(typeof(XmlSynonymsAttribute), true);
                if (attributes.IsNotNull() && attributes.Length > 0) {
                    var synonyms = attributes.Cast<XmlSynonymsAttribute>();
                    if (synonyms.Select(x => x.Synonym.Name).Contains(name)) {
                        return member;
                    }
                }
            }

            return null;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TargetDocumentTypeAttribute : Attribute {
        public DocumentType DocumentType { get; set; }
        public TargetDocumentTypeAttribute(DocumentType documentType) {
            DocumentType = documentType;
        }
    }

    public class XmlSynonym {
        public DocumentType Type { get; }
        public string Name { get; }
        public XmlSynonym(string name, DocumentType type = DocumentType.Nes17) {
            Type = type;
            Name = name;
        }
    }
}
