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
    [XmlType(TypeName = "kontakt-typ")]
    [XmlAnnotation("Element zawierający dane kontaktowe.")]
    public class ContactElement {
        [XmlText] public string Value { get; set; } = String.Empty;

        [XmlAttribute("typKontaktu")]
        [XmlAnnotation("Wskazanie rodzaju kontaktu np. email, telefon, adres strony internetowej.")]
        [XmlRequired]
        [XmlSimpleType(Annotation = "Typy kontaktu", EnumerationRestriction = typeof(ContactType), BaseTypeName = "xs:string", TypeName = "kontakt-rodzaj-typ", UnionMemberTypes = "xs:string")]
        public string Type { get; set; }

        public void SetType(ContactType type) {
            Type = type.GetXmlEnum();
        }
        public static string GetContactType(ContactType type) {
            return type.GetXmlEnum();
        }
    }
}
