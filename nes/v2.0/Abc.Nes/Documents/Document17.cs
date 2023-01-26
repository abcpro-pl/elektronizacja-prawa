/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes.Elements;
using Abc.Nes.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Abc.Nes {
    /// <summary>
    /// The root v.1.7 metadata element that describes document in archival package.
    /// </summary>
    [XmlType(TypeName = "dokument-typ")]
    [XmlAnnotation("Element główny metadanych opisujący dokument.")]
    [XmlRoot(ElementName = "dokument", Namespace = "http://www.mswia.gov.pl/standardy/ndap")]
    public class Document17 : IDocument {
        [XmlAttribute("wersja")] public DocumentType DocumentType { get; set; } = DocumentType.Nes17;
        [XmlElement("odbiorca")] public List<RecipientElement17> Recipients { get; set; }
        [XmlElement("data")][XmlRequired] public List<DateElement17> Dates { get; set; }
        [XmlElement("dostep")][XmlRequired] public List<AccessElement17> Access { get; set; }
        [XmlElement("format")][XmlRequired] public List<FormatElement17> Formats { get; set; }
        [XmlElement("grupowanie")] public List<GroupingElement> Groupings { get; set; }
        [XmlElement("identyfikator")][XmlRequired] public List<IdentifierElement> Identifiers { get; set; }
        [XmlElement("jezyk")][XmlAnnotation("Określenie języka naturalnego zgodnie z normą ISO-639-2, użytego w dokumencie.")] public List<LanguageElement> Languages { get; set; }

        [XmlElement("opis")]
        [XmlAnnotation("Streszczenie, spis treści lub krótki opis treści dokumentu.")]
        [XmlSimpleType(TypeName = "dokument-opis-typ", Annotation = "Opis dokumentu.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ")]
        public string Description { get; set; }


        [XmlElement("uprawnienia")]
        [XmlAnnotation("Wskazanie podmiotu uprawnionego do dysponowania treścią dokumentu.")]
        [XmlSimpleType(TypeName = "uprawnienia-typ", Annotation = "Wskazanie podmiotu uprawnionego do dysponowania treścią dokumentu.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ")]
        public List<string> Rights { get; set; }

        [XmlElement("kwalifikacja")] public List<QualificationElement> Qualifications { get; set; }
        [XmlElement("relacja")] public List<RelationElement> Relations { get; set; }
        [XmlElement("status")][XmlAnnotation("Status dokumentu.")] public List<StatusElement> Statuses { get; set; }
        [XmlElement("tematyka")] public List<KeywordElement17> Keywords { get; set; }
        [XmlElement("tworca")][XmlRequired] public List<AuthorElement17> Authors { get; set; }
        [XmlElement("typ")][XmlRequired] public List<TypeElement17> Types { get; set; }
        [XmlElement("tytul")][XmlRequired] public List<TitleElement> Titles { get; set; }

        internal static bool InternalValidateXmlFile(string filePath) {
            try {
                var xmlText = System.IO.File.ReadAllText(filePath);
                var xml = XElement.Parse(xmlText);
                return InternalValidateXml(xml);
            }
            catch { }

            return default;
        }
        internal static bool InternalValidateXml(string xmlText) {
            try {
                if (xmlText.Contains("Metadane-1.7.xsd") || xmlText.Contains("wersja=\"1.7\"")) {
                    return true;
                }

                var xml = XElement.Parse(xmlText);
                return InternalValidateXml(xml);
            }
            catch { }

            return default;
        }
        internal static bool InternalValidateXml(XElement xml) {
            try {
                if (xml.Attribute("wersja").Value() == "1.7") { return true; }
                return xml.HasElements && (xml.Elements().First().Name.LocalName == "odbiorca" || xml.Elements().First().Name.LocalName == "data");
            }
            catch { }

            return default;
        }
    }
}
