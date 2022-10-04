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
    public enum DocumentClassType {
        [XmlEnum("tekst")] Text,
        [XmlEnum("dźwięk")] Sound,
        [XmlEnum("obraz")] Picture,
        [XmlEnum("obraz ruchomy")] Gif,
        [XmlEnum("wideo")] Video,
        [XmlEnum("kolekcja")] Collection,
        [XmlEnum("zbiór danych")] DataSet,
        [XmlEnum("zdarzenie")] Event,
        [XmlEnum("obraz")] Image,
        [XmlEnum("zasób interaktywny")] InteractiveResource,
        [XmlEnum("obraz ruchomy")] MovingImage,
        [XmlEnum("obiekt fizyczny")] PhysicalObject,
        [XmlEnum("oprogramowanie")] Software,
        [XmlEnum("obraz nieruchomy")] StillImage
    }
}
