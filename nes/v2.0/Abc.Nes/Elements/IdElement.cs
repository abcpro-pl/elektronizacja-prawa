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
using System;
using System.Xml.Serialization;

namespace Abc.Nes.Elements {
    [XmlType(TypeName = "id-typ")]
    [XmlAnnotation("Identyfikator elementu wraz ze zdefiniowanym typem.")]
    public class IdElement {
        [XmlText] public string Value { get; set; } = String.Empty;

        [XmlAttribute("typId")]
        [XmlSynonyms("typ")]
        [XmlRequired]
        [XmlAnnotation("Typ identyfikatora.")]
        [XmlSimpleType(TypeName = "id-rodzaj-typ", Annotation = "Typ identyfikatora.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ", EnumerationRestriction = typeof(InstitutionIdType))]
        public string Type { get; set; }

        public static string GetInstitutionIdType(InstitutionIdType type) { return type.GetXmlEnum(); }
    }
}
