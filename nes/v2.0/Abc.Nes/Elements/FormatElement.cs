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
    [XmlType(TypeName = "format-typ")]
    [XmlAnnotation(@"Element zawierający informacje o formacie dokumentu. 
Pozwala na wskazanie formatu i rozmiaru dokumentu. Umożliwi w przyszłości wyszukiwanie dokumentów wg ich rozmiaru lub formatu. 
Pozwoli na odróżnienie dokumentów elektronicznych od nieelektronicznych celem sprawdzenia czy dany zasób informacyjny zawiera odniesienia do dokumentów zapisanych na nośnikach fizycznych(papier, taśmy audio i wideo ale także przenośne informatyczne nośniki danych na których zapisano e - dokumentację znajdującą się poza zbiorem dostępnym bezpośrednio).Będzie można również wyszukać dokumentację wymagającą przeniesienia na inne nośniki co ułatwi konserwację zasobu. 
W przypadku dokumentów elektronicznych powoli także na zarządzanie dokumentacją zapisaną w przestarzałych technologicznie formatach w przypadku gdy system operacyjny nie będzie umiał rozpoznać formatu (informacja o formacie będzie wskazówką naprowadzającą co to jest za zasób). W szczególności będzie można wyszukać i wyodrębnić dokumentację w przestarzałych formatach celem przygotowania jej konwersji do formatów nowszych lub powiązania z oprogramowaniem zapewniającym możliwość wizualizacji treści.")]
    public class FormatElement {
        [XmlElement("typFormatu")]
        [XmlAnnotation("Typ formatu")]
        [XmlRequired]
        [XmlSimpleType(TypeName = "niepusty-ciag-typ", BaseTypeName = "xs:string", MinLength = 1, Pattern = @"(\r|\n|.)*\S(\r|\n|.)*", Annotation = "Typ definiujący nie pusty ciąg znaków.")]
        public string Type { get; set; }

        [XmlElement("specyfikacja")] [XmlAnnotation("Specyficzne dla danego typu formatu sposoby zapisu.")] public string Specification { get; set; }
        public bool ShouldSerializeSpecification() { return Specification.IsNotNullOrEmpty(); }

        [XmlElement("niekompletnosc")] [XmlAnnotation("Wskazanie, że dokument w formacie wskazanym przez format.typFormatu jest niekompletnym odwzorowaniem cyfrowym dokumentu nieelektronicznego.")] public BooleanValues Uncompleted { get; set; }
        public bool ShouldSerializeUncompleted() { return Uncompleted != BooleanValues.None; }

        [XmlElement("wielkosc")] public SizeElement Size { get; set; }
    }
}
