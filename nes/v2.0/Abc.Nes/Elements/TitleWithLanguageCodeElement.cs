/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes.Enumerations;
using System;
using System.Xml.Serialization;

namespace Abc.Nes.Elements {
    [XmlType(TypeName = "tytul-z-jezykiem-typ")]
    [XmlAnnotation("Element zawierający tytuł dokumentu z podanie kodu języka, w którym go sporządzono.")]
    public class TitleWithLanguageCodeElement : ElementBase {
        [XmlText] public string Value { get; set; } = String.Empty;

        [XmlAttribute("kodJezyka")]
        [XmlSynonyms("jezyk", DocumentType.Nes17)]
        [XmlSynonyms("jezyk", DocumentType.Nes16)]
        [XmlRequired(false)]
        public LanguageCode Type { get; set; } = LanguageCode.pol;
    }

    [XmlType(TypeName = "jezyk-typ")]
    [XmlAnnotation("Element zawierający nazwę i kod języka, w którym go sporządzono.")]
    public class LanguageElement : ElementBase {
        [XmlText] public string Value { get; set; } = String.Empty;

        [XmlAttribute("kodJezyka")]
        [XmlSynonyms("kod", DocumentType.Nes17)]
        [XmlSynonyms("kod", DocumentType.Nes16)]
        [XmlRequired]
        public LanguageCode Type { get; set; }
    }
}
