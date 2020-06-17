/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Abc.Nes.Elements {
    [XmlType(TypeName = "podmiot-typ")]
    [XmlAnnotation("Element zawierający dane podmiotu.")]
    public class SubjectElement : ElementBase {
        [XmlRequired]
        [XmlChoiceIdentifier("SubjectType")]
        [XmlElement("osoba", typeof(PersonElement))]
        [XmlElement("instytucja", typeof(InstitutionElement))]
        public object Subject { get; set; }

        [XmlIgnore] public SubjectType SubjectType;
    }

    [XmlType(IncludeInSchema = false)]
    public enum SubjectType {
        osoba,
        instytucja
    }
}
