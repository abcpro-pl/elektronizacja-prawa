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
    [XmlType(TypeName = "kwalifikacja-typ")]
    [XmlAnnotation(@"Kategoria archiwalna dokumentu oraz informacje o tym, kto i kiedy nadał kategorię. 
Pozwala na określenie, jak długo dany dokument (grupa dokumentów) powinien być przechowywany, oraz czy i kiedy zostanie przekazany do archiwum państwowego. 
Umożliwi łatwe ustalenie, jakie materiały można ze zbiorów w danym roku wycofać, dla jakich trzeba ustalić ponownie kategorię archiwalną, a jakie podlegają obowiązkowi przekazania do archiwum państwowego.")]
    public class QualificationElement {
        [XmlElement("kategoria")]
        [XmlRequired]
        [XmlAnnotation("Kategoria archiwalna.")]
        [XmlSimpleType(TypeName = "kategoria-archiwalna-typ", Annotation = "Kategoria archiwalna zgodnie z wykazem akt.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ", EnumerationRestriction = typeof(ArchivalCategoryType))]
        public string Type { get; set; }

        [XmlElement("data")]
        [XmlRequired]
        [XmlAnnotation("Data nadania kategorii archiwalnej.")]
        [XmlSimpleType(TypeName = "czas-typ", UnionMemberTypes = "xs:gYear xs:gYearMonth xs:date xs:dateTime", Annotation = "Format YYYY-MM-DD gdzie YYYY to cztery cyfry określenia roku, MM to dwie cyfry określenia miesiąca (04 dla kwietnia) a DD to dwie cyfry określenia dnia w dacie.")]
        public string Date { get; set; }

        [XmlElement("podmiot")] [XmlRequired] public SubjectElement Subject { get; set; }

        public void SetType(ArchivalCategoryType type) {
            Type = type.GetXmlEnum();
        }       
    }


    [XmlType(TypeName = "kwalifikacja-typ")]
    [XmlAnnotation(@"Kategoria archiwalna dokumentu oraz informacje o tym, kto i kiedy nadał kategorię. 
Pozwala na określenie, jak długo dany dokument (grupa dokumentów) powinien być przechowywany, oraz czy i kiedy zostanie przekazany do archiwum państwowego. 
Umożliwi łatwe ustalenie, jakie materiały można ze zbiorów w danym roku wycofać, dla jakich trzeba ustalić ponownie kategorię archiwalną, a jakie podlegają obowiązkowi przekazania do archiwum państwowego.")]
    public class QualificationElement16 {
        [XmlElement("kategoria")]
        [XmlRequired]
        [XmlAnnotation("Kategoria archiwalna.")]
        [XmlSimpleType(TypeName = "kategoria-archiwalna-typ", Annotation = "Kategoria archiwalna zgodnie z wykazem akt.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ", EnumerationRestriction = typeof(ArchivalCategoryType))]
        public string Type { get; set; }

        [XmlElement("data")]
        [XmlRequired]
        [XmlAnnotation("Data nadania kategorii archiwalnej.")]
        [XmlSimpleType(TypeName = "czas-typ", UnionMemberTypes = "xs:gYear xs:gYearMonth xs:date xs:dateTime", Annotation = "Format YYYY-MM-DD gdzie YYYY to cztery cyfry określenia roku, MM to dwie cyfry określenia miesiąca (04 dla kwietnia) a DD to dwie cyfry określenia dnia w dacie.")]
        public string Date { get; set; }

        [XmlElement("podmiot")][XmlRequired] public SubjectElement16 Subject { get; set; }

        public void SetType(ArchivalCategoryType type) {
            Type = type.GetXmlEnum();
        }
    }
}
