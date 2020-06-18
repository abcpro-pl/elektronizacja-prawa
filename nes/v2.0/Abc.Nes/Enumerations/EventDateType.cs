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

namespace Abc.Nes.Enumerations {
    [XmlType(TypeName = "data-zdarzenia-typ")]
    [XmlAnnotation("Słownik zdarzeń zwiazanych z datą dokumentu.")]
    public enum EventDateType {
        [XmlEnum("")]
        None,
        [XmlEnum("opublikowany")]
        Published,
        [XmlEnum("utworzony")]
        Created,
        [XmlEnum("data na piśmie")]
        DocumentDate,
        [XmlEnum("uzyskany")]
        Requested,
        [XmlEnum("doręczony")]
        Recieved,
        [XmlEnum("wysłany")]
        Sent,
        [XmlEnum("zatwierdzony")]
        Approved,
        [XmlEnum("podpisany")]
        Signed,
        [XmlEnum("zmodyfikowany")]
        Modified,
        [XmlEnum("data rejestracji")]
        Registred,
    }
}
