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
using System.Xml.Serialization;

namespace Abc.Nes.Elements {
    [XmlType(TypeName = "adres-typ")]
    [XmlAnnotation("Element zawierający dane adresowe.")]
    public class AddressElement  {
        [XmlElement("kodPocztowy")] [XmlAnnotation("Kod pocztowy")] public string ZipCode { get; set; }
        [XmlElement("poczta")] [XmlAnnotation("Nazwa urzędu pocztowego")] public string PostName { get; set; }
       
        [XmlElement("miejscowosc")] 
        [XmlAnnotation("Nazwa miejscowości")] 
        [XmlRequired] 
        [XmlSimpleType(TypeName = "niepusty-ciag-typ", BaseTypeName = "xs:string", MinLength = 1, Pattern = @"(\r|\n|.)*\S(\r|\n|.)*", Annotation = "Typ definiujący nie pusty ciąg znaków.")] 
        public string Location { get; set; }
        [XmlElement("ulica")] [XmlAnnotation("Nazwa ulicy")] public string StreetName { get; set; }
        [XmlElement("budynek")] [XmlAnnotation("Numer budynku")] public string BuildingNo { get; set; }
        [XmlElement("lokal")] [XmlAnnotation("Numer miszkania")] public string AppartmentNo { get; set; }
        [XmlElement("skrytkaPocztowa")] [XmlAnnotation("Adres skrytki pocztowej")] public string PostBox { get; set; }
        [XmlElement("uwagi")] [XmlAnnotation("Dodatkowe informacje o adresie")] public string Description { get; set; }
        [XmlElement("kraj")] [XmlAnnotation("Kod kraju")] public CountryCodeType Country { get; set; } = CountryCodeType.PL;

        [XmlElement("gmina")] [XmlAnnotation("Nazwa gminy")] public string Community { get; set; }
        [XmlElement("powiat")] [XmlAnnotation("Nazwa powiatu")] public string County { get; set; }
        [XmlElement("wojewodztwo")] [XmlAnnotation("Nazwa województwa")] public string Voivodeship { get; set; }

    }
}
