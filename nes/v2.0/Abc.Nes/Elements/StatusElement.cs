/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System.Xml.Serialization;

namespace Abc.Nes.Elements {
    [XmlType(TypeName = "status-typ")]
    [XmlAnnotation(@"Status dokumentu.")]
    public class StatusElement {
        [XmlElement("rodzaj")]
        [XmlRequired]
        [XmlAnnotation("Rodzaj statusu.")]
        [XmlSimpleType(TypeName = "status-rodzaj-typ", Annotation = "Rodzaj statusu.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ")]
        public string Kind { get; set; }

        [XmlElement("wersja")]
        [XmlRequired]
        [XmlAnnotation("Wersja statusu.")]
        [XmlSimpleType(TypeName = "status-wersja-typ", Annotation = "Wersja statusu.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ")]
        public string Version { get; set; }

        [XmlElement("opis")]
        [XmlAnnotation("Opis statusu.")]
        [XmlSimpleType(TypeName = "status-opis-typ", Annotation = "Opis statusu.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ")]
        public string Description { get; set; }
    }
}
