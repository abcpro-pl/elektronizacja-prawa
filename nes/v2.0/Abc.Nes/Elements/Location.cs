using System;
using System.Xml.Serialization;

namespace Abc.Nes.Elements {
    [XmlType(TypeName = "lokalizacja-typ")]
    [XmlAnnotation(@"Lokalizacja dokumentu.")]
    public class Location16 {
        [XmlElement("typ")]
        [XmlAnnotation("Typ lokalizacji.")]
        [XmlRequired]
        [XmlSimpleType(TypeName = "lokalizacja-typ-typ", Annotation = "Opis typu lokalizacji.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ")]
        public string Type { get; set; }

        [XmlElement("podmiot")][XmlAnnotation("Podmiot - osoba lub instytucja.")] public SubjectElement16 Subject { get; set; }
        public bool ShouldSerializeSubject() { return Subject.IsNotNull(); }

        [XmlElement("adres")][XmlAnnotation("Adres.")] public string Address { get; set; }
    }
}
