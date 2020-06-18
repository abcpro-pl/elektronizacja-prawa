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
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Abc.Nes {
    [XmlType(TypeName = "dokument-typ")]
    [XmlAnnotation("Element główny metadanych opisujący dokument.")]
    [XmlRoot(ElementName = "dokument", Namespace = "http://www.mswia.gov.pl/standardy/ndap")]
    public class Document {
        [XmlElement("identyfikator")] [XmlRequired] public List<IdentifierElement> Identifiers { get; set; }
        [XmlElement("tytul")] [XmlRequired] public List<TitleElement> Titles { get; set; }
        [XmlElement("data")] [XmlRequired] public List<DateElement> Dates { get; set; }
        [XmlElement("format")] [XmlRequired] public List<FormatElement> Formats { get; set; }
        [XmlElement("dostep")] [XmlRequired] public List<AccessElement> Access { get; set; }
        [XmlElement("typ")] [XmlRequired] public List<TypeElement> Types { get; set; }
        [XmlElement("grupowanie")] [XmlRequired] public List<GroupingElement> Groupings { get; set; }
        [XmlElement("tworca")] public List<AuthorElement> Authors { get; set; }
        [XmlElement("nadawca")] public List<SenderElement> Senders { get; set; }
        [XmlElement("odbiorca")] public List<RecipientElement> Recipients { get; set; }
        [XmlElement("relacja")] public List<RelationElement> Relations { get; set; }
    }
}
