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

namespace Abc.Nes.Elements {
    [XmlType(TypeName = "grupowanie-typ")]
    [XmlAnnotation(@"Wskazanie przynależności do akt sprawy lub innego zbioru dokumentów. Pozwala na łączenie dokumentów w grupy.")]
    public class GroupingElement {
        [XmlElement("typGrupy")]
        [XmlSynonyms("typ")]
        [XmlRequired]
        [XmlAnnotation("Określenie typu grupy dokumentów np. sprawa, rejestr umów, rejestr skarg i wniosków itd.")]
        [XmlSimpleType(TypeName = "grupowanie-typgrupy-typ", Annotation = "Dla zbioru/grupy będącej sprawą założoną zgodnie z jednolitym rzeczowym wykazem akt typ przyjmuje wartość = „znak sprawy”. Dla pozostałych grup – inny dowolny tekst w sposób jednoznaczny charakteryzujący wyodrębniony zbiór/grupę dokumentów.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ")]
        public string Type { get; set; }

        [XmlElement("kodGrupy")]
        [XmlSynonyms("kod")]
        [XmlRequired]
        [XmlAnnotation("Numer albo ciąg znaków będący identyfikatorem grupy dokumentów. W danym kontekście (miejscu w strukturze obiegu dokumentów) wartość powinna być unikatowa.")]
        [XmlSimpleType(TypeName = "grupowanie-kodgrupy-typ", Annotation = "Dla grupy będącej sprawą założoną zgodnie z jednolitym rzeczowym wykazem akt, kod przyjmuje wartość zgodną z przyjętym w podmiocie sposobem znakowania spraw. Dla pozostałych grup – unikatowy identyfikator wyodrębnionego zbioru/grupy.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ")]
        public string Code { get; set; }

        [XmlElement("opis")]
        [XmlRequired]
        [XmlAnnotation("Hasło klasyfikacyjne wykazu akt w przypadku sprawy; w przypadku grupy dokumentów innej niż sprawa - tekstowy opis grupy.")]
        [XmlSimpleType(TypeName = "grupowanie-opis-typ", Annotation = "Dla grupy będącej sprawą założoną zgodnie z jednolitym rzeczowym wykazem akt – hasło klasyfikacyjne wykazu akt odpowiadające pozycji tego wykazu użytej do nadania znaku sprawy. Dla pozostałych grup tekst dodatkowo objaśniający w sposób zwięzły cel wyodrębnienia zbioru/grupy dokumentów.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ")]
        public string Description { get; set; }
    }
}
