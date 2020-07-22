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
    public enum IdTypes {
        [XmlEnum("SystemID")] SystemID,
        [XmlEnum("znak sprawy")] ObjectMark,
        [XmlEnum("sygnatura akt")] ObjectSignatureMark,
        [XmlEnum("znak pisma")] DocumentMark,
        [XmlEnum("numer")] Number,
        [XmlEnum("id.ezd.puw")] PUW_ID,
        [XmlEnum("id.ezd.eDOK")] eDOK_ID,
        [XmlEnum("ePUAP CID")] ePuapClientId
    }
}
