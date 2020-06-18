﻿/*=====================================================================================

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
    [XmlType(TypeName = "instytucja-typ")]
    [XmlAnnotation("Element zawierający dane instytucji. Podmiot inny niż osoba fizyczna np. urząd, organizacja, przedsiębiorstwo. ")]
    public class InstitutionElement  {
        [XmlElement("id")] public List<InstitutionIdElement> Identifiers { get; set; }
        [XmlElement("nazwa")] [XmlRequired] [XmlAnnotation("Nazwa instytucji")] public string Name { get; set; }
        [XmlElement("adres")] public List<AddressElement> Addresses { get; set; }
        [XmlElement("kontakt")] public List<ContactElement> Contacts { get; set; }
        [XmlElement("komorka")] public InstitutionUnitElement Unit { get; set; }
        [XmlElement("pracownik")] public EmployeeElement Employee { get; set; }
    }
}
