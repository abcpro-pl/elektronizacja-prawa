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
    [XmlType(TypeName = "instytucja-pracownik-typ")]
    [XmlAnnotation("Element zawierający dane pracownika instytucji. Pozwala opisać dane identyfikujące pracownika instytucji odpowiedzialnego za czynności przy tworzeniu i obiegu dokumentów.")]
    public class EmployeeElement  {
        [XmlElement("nazwisko")] [XmlRequired] [XmlAnnotation("Nazwisko")] public string Surname { get; set; }
        [XmlElement("imie")] [XmlAnnotation("Imiona")] public List<string> FirstNames { get; set; }
        [XmlElement("kontakt")] [XmlAnnotation("Kontakty")] public List<ContactElement> Contacts { get; set; }
        [XmlElement("stanowisko")] [XmlAnnotation("Stanowisko zajmowane przez pracownika")] public string Position { get; set; }
    }

    [XmlType(TypeName = "instytucja-jednostka-pracownik-typ")]
    [XmlAnnotation("Element zawierający dane pracownika instytucji. Pozwala opisać dane identyfikujące pracownika instytucji odpowiedzialnego za czynności przy tworzeniu i obiegu dokumentów.")]
    public class EmployeeOrgElement {
        [XmlElement("id")] public List<IdElement> Identifiers { get; set; }
        [XmlElement("nazwisko")] [XmlRequired] [XmlAnnotation("Nazwisko")] public string Surname { get; set; }
        [XmlElement("imie")] [XmlAnnotation("Imię")] public string FirstName { get; set; }
        [XmlElement("kontakt")] [XmlAnnotation("Kontakty")] public List<ContactElement> Contacts { get; set; }
        [XmlElement("funkcja")] [XmlAnnotation("Stanowisko zajmowane przez pracownika")] public string Position { get; set; }
    }
}
