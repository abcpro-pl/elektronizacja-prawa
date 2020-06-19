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
    public enum ArchivalCategoryType {
        [XmlEnum("A")] A,
        [XmlEnum("B")] B,
        [XmlEnum("B3")] B3,
        [XmlEnum("B5")] B5,
        [XmlEnum("B10")] B10,
        [XmlEnum("B15")] B15,
        [XmlEnum("B20")] B20,
        [XmlEnum("B40")] B40,
        [XmlEnum("B50")] B50,
        [XmlEnum("BE10")] BE10,
        [XmlEnum("BE15")] BE15,
        [XmlEnum("BE20")] BE20,
        [XmlEnum("BE40")] BE40,
        [XmlEnum("BE50")] BE50,
        [XmlEnum("BC")] BC,
    }
}
