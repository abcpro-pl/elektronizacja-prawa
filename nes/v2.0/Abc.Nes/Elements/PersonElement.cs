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
    [XmlType(TypeName = "osoba-typ")]
    [XmlAnnotation("Element zawierający dane osoby fizycznej.")]
    public class PersonElement  {
        [XmlElement("id")] public List<PersonIdElement> Identifiers { get; set; }
        [XmlElement("nazwisko")] [XmlRequired] [XmlAnnotation("Nazwisko")] public string Surname { get; set; }
        [XmlElement("imie")] [XmlAnnotation("Imiona")] public List<string> FirstNames { get; set; }
        [XmlElement("adres")] [XmlAnnotation("Adresy")] public List<AddressElement> Addresses { get; set; }
        [XmlElement("kontakt")] [XmlAnnotation("Kontakty")] public List<ContactElement> Contacts { get; set; }
    }

    [XmlType(TypeName = "osoba-typ")]
    [XmlAnnotation("Element zawierający dane osoby fizycznej.")]
    public class PersonElement16 {
        [XmlElement("id")] public List<PersonIdElement> Identifiers { get; set; }
        [XmlElement("nazwisko")][XmlRequired][XmlAnnotation("Nazwisko")] public string Surname { get; set; }
        [XmlElement("imie")][XmlAnnotation("Imię")] public string FirstName { get; set; }
        [XmlElement("adres")][XmlAnnotation("Adresy")] public List<AddressElement> Addresses { get; set; }
        [XmlElement("kontakt")][XmlAnnotation("Kontakty")] public List<ContactElement> Contacts { get; set; }
    }
}
