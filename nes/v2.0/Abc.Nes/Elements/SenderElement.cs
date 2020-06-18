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
    [XmlType(TypeName = "nadawca-typ")]
    [XmlAnnotation(@"Podmiot (instytucja lub osoba) który został odnotowany jako nadawca dokumentu zarejestrowanego w dokumentacji organu lub jednostki.")]
    public class SenderElement {
        [XmlElement("podmiot")] [XmlRequired] public SubjectElement Subject { get; set; }
    }
}
