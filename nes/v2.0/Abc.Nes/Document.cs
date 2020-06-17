/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes.Elements;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Abc.Nes {
    [XmlType(TypeName = "dokument-typ")]
    [XmlAnnotation("Element główny metadanych opisujący dokument.")]
    [XmlRoot(ElementName = "dokument", Namespace = "http://www.mswia.gov.pl/standardy/ndap")]
    public class Document : ElementBase {
        [XmlElement("identyfikator")] [XmlRequired] public List<IdentifierElement> Identifiers { get; set; }

    }
}
