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
    public enum ContactType {
        [XmlEnum("telefon")] Phone,
        [XmlEnum("faks")] Fax,
        [XmlEnum("email")] Email,
        [XmlEnum("url")] Url,
        [XmlEnum("skype")] Skype,
        [XmlEnum("facebook")] Facebook,
        [XmlEnum("youtube")] YouTube,
        [XmlEnum("instagram")] Instagram,
        [XmlEnum("tiktok")] TikTok,
        [XmlEnum("teams")] Teams,
        [XmlEnum("snapchat")] Snapchat,
        [XmlEnum("wuze")] Wuze,
        [XmlEnum("messanger")] Messanger,
        [XmlEnum("zoom")] Zoom
    }
}
