﻿/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System.Xml.Serialization;

namespace Abc.Nes.Enumerations {
    [XmlType(TypeName = "dostepnosc-typ")]
    [XmlAnnotation(@"Słownik typów dostępności.")]
    public enum AccessType {
        [XmlEnum("publiczny")]
        Public,
        [XmlEnum("niepubliczny")]
        NonPublic,
        [XmlEnum("wyłączny")]
        Exclusive,
        [XmlEnum("wszystko")]
        All
    }

    [XmlType(TypeName = "dostepnosc-typ")]
    [XmlAnnotation(@"Słownik typów dostępności.")]
    public enum AccessType17 {
        [XmlEnum("wszystko")]
        All,
        [XmlEnum("metadane")]
        Metadata,
        [XmlEnum("niedostepne")]
        NotAvailable       
    }

    [XmlType(TypeName = "dostepnosc-data-typ")]
    [XmlAnnotation("Typ daty dostepności.")]
    public enum AccessDateType {
        [XmlEnum("dostępny po")]
        After
    }
}
