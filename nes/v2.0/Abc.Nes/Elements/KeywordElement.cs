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
    [XmlType(TypeName = "tematyka-typ")]
    [XmlAnnotation(@"Słowa kluczowe dotyczące treści dokumentu.")]
    public class KeywordElement {
        [XmlElement("przedmiot")] [XmlAnnotation("Określenie tematyki treści dokumentu na bardzo dużym poziomie ogólności, nie będące nazwą własną (tj. nie nazwą geograficzną, osoby, obiektu, ulicy, instytucji itp.)")] public List<string> Matters { get; set; }
        [XmlElement("wspomnianaOsoba")] [XmlAnnotation("Określenie osoby o której traktuje treść dokumentu.")] public List<string> MentionedPeople { get; set; }
        [XmlElement("miejsce")] [XmlAnnotation("Określenie obszaru administracyjnego lub obiektów geograficznych o których traktuje treść.")] public List<string> Places { get; set; }
        [XmlElement("data")] [XmlAnnotation("Określenie okresów czasu lub konkretnych dat, których dotyczy treść dokumentu; posiada dwa podelementy: czasOd i czasDo.")] public List<DateElement> Dates { get; set; }
        [XmlElement("odbiorca")] [XmlAnnotation("Kategoria (grupa) osób lub instytucji, dla których dany dokument jest przeznaczony (do których kierowana jest treść).")]public List<string> Recipients { get; set; }
        [XmlElement("inne")] public List<KeywordDataElement> Others { get; set; }

    }

    [XmlType(TypeName = "tematyka-slownik-typ")]
    [XmlAnnotation(@"Inne wskazówki dotyczące treści, posiada dwa podelementy: klucz i wartość.")]
    public class KeywordDataElement {
        [XmlElement("klucz")]
        [XmlRequired]
        [XmlAnnotation("Klucz hasła tematycznego.")]
        [XmlSimpleType(TypeName = "klucz-nazwa-typ", Annotation = "Klucz hasła tematycznego.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ")]
        public string Key { get; set; }
        [XmlElement("wartosc")] [XmlRequired] [XmlAnnotation("Wartość hasła tematycznego.")] public string Value { get; set; }
    }
}
