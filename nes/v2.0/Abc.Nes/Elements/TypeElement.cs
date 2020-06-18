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
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Abc.Nes.Elements {
    [XmlType(TypeName = "kategoria-typ")]
    [XmlAnnotation(@"Określenie kategorii i rodzaju dokumentu. 
Pozwala na odnajdywanie dokumentów wg określonych klas (np. tylko dokumentów tekstowych, tylko fotografii, tylko nagrań dźwiękowych) oraz rodzajów (np. tylko decyzji, postanowień, urzędowych poświadczeń odbioru).
Umożliwia podanie informacji o rodzaju dokumentu na wykazach dokumentów znajdujących się w danym zbiorze.")]
    public class TypeElement {
        [XmlElement("klasa")]
        [XmlRequired]
        [XmlAnnotation("Określenie typu dokumentu na bardzo ogólnym poziomie (np. tekst, dźwięk, obraz, obraz ruchomy itd).")]
        [XmlSimpleType(TypeName = "kategoria-klasa-typ", Annotation = "Słownik zdefiniowany w repozytorium interoperacyjności oparty o listę kategorii DCMI: http://dublincore.org/documents/dcmi-type-vocabulary/ (tekst, dźwięk, obraz nieruchomy, obraz ruchomy, zbiór dokumentów, zbiór danych, obiekt fizyczny, oprogramowanie).", BaseTypeName = "xs:string", EnumerationRestriction = new string[] { "tekst", "dźwięk", "obraz", "obraz ruchomy", "wideo" }, UnionMemberTypes = "ndap:niepusty-ciag-typ")]
        public string Class { get; set; }

        [XmlElement("rodzaj")]
        [XmlRequired]
        [XmlAnnotation("Dookreślenie typu dokumentu ze względu na cel jakiemu służy dokument (np. decyzja, prezentacja, faktura, ustawa, notatka, rozporządzenie, pismo itd).")]
        [XmlSimpleType(TypeName = "kategoria-rodzaj-typ", Annotation = "Dookreślenie typu dokumentu w ramach wskazanej klasy; Rekomendowane jest przygotowanie słownika rodzajów właściwego dla praktyki  kancelaryjnej podmiotu w odniesieniu do klasy tekst . Słownik taki będzie musiał zawierać co najmniej zestaw określeń wskazanych w repozytorium interoperacyjności.", EnumerationRestriction =new string[] { "decyzja", "prezentacja", "faktura", "ustawa", "notatka", "rozporządzenie", "pismo" }, UnionMemberTypes = "ndap:niepusty-ciag-typ", BaseTypeName = "xs:string")]
        public List<string> Kinds { get; set; }
    }
}
