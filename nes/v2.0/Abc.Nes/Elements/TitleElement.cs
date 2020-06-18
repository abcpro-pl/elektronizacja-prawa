/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Abc.Nes.Elements {
    [XmlType(TypeName = "tytul-typ")]
    [XmlAnnotation(@"Tytuł dokumentu. 
Pozwala na wstępne zorientowanie się czego dotyczy dokument o określonym tytule. Ma to szczególne znaczenie w przypadku prezentacji listy dokumentów należących do określonego zbioru – na przykład dokumentów należących do akt sprawy, dokumentów zarejestrowanych w ewidencji umów lub listy dokumentów będących wynikiem wyszukiwania. Tytuł pozwala zorientować się przeglądającemu listę, czy treść określonego dokumentu jest interesująca dla niego bez potrzeby zapoznawania się z treścią każdego dokumentu. 
Dodatkowym celem wyodrębnienia tytułu jest możliwość wyszukania dokumentu wg fragmentu lub całości tytułu, co w połączeniu z dodatkowymi warunkami wyszukiwania (np. data i twórca) może pozwolić na szybkie i precyzyjnie odnalezienie dokumentu w większym zbiorze.")]
    public class TitleElement  {
        [XmlElement("oryginalny")] [XmlRequired] public TitleWithLanguageCodeElement Original { get; set; }
        [XmlElement("alternatywny")] public List<TitleWithLanguageCodeElement> Alternative { get; set; }
    }
}
