using Abc.Nes.Elements;
using Abc.Nes.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Abc.Nes {
    /// <summary>
    /// The root v.1.6 metadata element that describes document in archival package.
    /// </summary>
    [XmlType(TypeName = "dokument-typ")]
    [XmlAnnotation("Element główny metadanych opisujący dokument.")]
    [XmlRoot(ElementName = "dokument", Namespace = "http://www.mswia.gov.pl/standardy/ndap")]
    public class Document16 : IDocument {
        [XmlAttribute("wersja")] public DocumentType DocumentType { get; set; } = DocumentType.Nes16;
        [XmlElement("odbiorca")] public List<RecipientElement16> Recipients { get; set; }
        [XmlElement("data")][XmlRequired] public List<DateElement16> Dates { get; set; }
        [XmlElement("dostep")][XmlRequired] public List<AccessElement16> Access { get; set; }
        [XmlElement("format")][XmlRequired] public List<FormatElement16> Formats { get; set; }
        [XmlElement("grupowanie")] public List<GroupingElement> Groupings { get; set; }
        [XmlElement("identyfikator")][XmlRequired] public List<IdentifierElement16> Identifiers { get; set; }
        [XmlElement("jezyk")][XmlAnnotation("Określenie języka naturalnego zgodnie z normą ISO-639-2, użytego w dokumencie.")] public List<LanguageElement> Languages { get; set; }
        [XmlElement("lokalizacja")]
        public Location16 Location { get; set; } = new Location16() {
            Address = String.Empty,
            Type = String.Empty
        };

        [XmlElement("opis")]
        [XmlAnnotation("Streszczenie, spis treści lub krótki opis treści dokumentu.")]
        [XmlSimpleType(TypeName = "dokument-opis-typ", Annotation = "Opis dokumentu.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ")]
        public string Description { get; set; } = String.Empty;

        [XmlElement("uprawnienia")]
        [XmlAnnotation("Wskazanie podmiotu uprawnionego do dysponowania treścią dokumentu.")]
        [XmlSimpleType(TypeName = "uprawnienia-typ", Annotation = "Wskazanie podmiotu uprawnionego do dysponowania treścią dokumentu.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ")]
        public List<string> Rights { get; set; }
        [XmlElement("kwalifikacja")] public List<QualificationElement16> Qualifications { get; set; }
        [XmlElement("relacja")] public List<RelationElement16> Relations { get; set; }
        [XmlElement("status")][XmlAnnotation("Status dokumentu.")] public List<StatusElement> Statuses { get; set; }
        [XmlElement("tematyka")] public List<KeywordElement16> Keywords { get; set; }
        [XmlElement("tworca")][XmlRequired] public List<AuthorElement16> Authors { get; set; }
        [XmlElement("typ")][XmlRequired] public List<TypeElement16> Types { get; set; }
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
                if (xmlText.Contains("Metadane-1.6.xsd") || xmlText.Contains("wersja=\"1.6\"")) {
                    return true;
                }
            }
            catch { }

            return default;
        }
        internal static bool InternalValidateXml(XElement xml) {
            try {
                if (xml.Attribute("wersja").Value() == "1.6") { return true; }
            }
            catch { }

            return default;
        }
    }
}
