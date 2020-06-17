/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System;
using System.Xml.Serialization;

namespace Abc.Nes.Elements {
    [XmlType(TypeName = "osoba-identyfikator-typ")]
    [XmlAnnotation("Identyfikator osoby.")]
    public class PersonIdElement {
        [XmlText] public string Value { get; set; } = String.Empty;
        [XmlAttribute("typId")] [XmlRequired] public PersonIdType Type { get; set; }
    }

    [XmlType(TypeName = "osoba-id-typ")]
    [XmlAnnotation("Identyfikator osoby.")]
    public enum PersonIdType {
        [XmlEnum("PESEL")]
        Id,
        [XmlEnum("NIP")]
        TaxId
    }
}
