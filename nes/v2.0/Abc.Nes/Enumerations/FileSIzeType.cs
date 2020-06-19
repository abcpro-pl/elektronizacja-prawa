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
    public enum FileSizeType {
        [XmlEnum("bajt")] bajt,
        [XmlEnum("b")] b,
        [XmlEnum("kB")] kB,
        [XmlEnum("MB")] MB,
        [XmlEnum("GB")] GB,
        [XmlEnum("TB")] TB,
        [XmlEnum("PB")] PB,
        [XmlEnum("EB")] EB,
        [XmlEnum("ZB")] ZB,
        [XmlEnum("YB")] YB
    }
}
