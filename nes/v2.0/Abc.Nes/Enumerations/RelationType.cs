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
    public enum RelationType {
        [XmlEnum("ma odniesienie")]
        HasReference,
        [XmlEnum("odnosi się do")]
        IsReference,
        [XmlEnum("jest dekretacją")]
        IsAttribution,
        [XmlEnum("ma dekretację")]
        HasAttribution,
        [XmlEnum("ma podpis")]
        HasSignature,
        [XmlEnum("jest podpisem")]
        IsSignature,
        [XmlEnum("ma wersję")]
        HasVersion,
        [XmlEnum("jest wersją")]
        IsVersion,
        [XmlEnum("ma część")]
        HasPart,
        [XmlEnum("jest częścią")]
        IsPart,
        [XmlEnum("ma format")]
        HasFormat,
        [XmlEnum("jest formatem")]
        IsFormat
    }
}
