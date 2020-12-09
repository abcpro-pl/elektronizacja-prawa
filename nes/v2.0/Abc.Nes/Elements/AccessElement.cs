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
    [XmlType(TypeName = "dostep-typ")]
    [XmlAnnotation(@"Element zawierający status dostępności dokumentu. 
Określenie zasad i warunków regulujących dostęp do dokumentów. 
Informacja o dostępności nie może naruszać przepisów regulujących dostęp do danych (np. do danych osobowych, informacji niejawnych itd.)")]
    public class AccessElement {
        [XmlElement("dostepnosc")] [XmlRequired] public AccessType Access { get; set; }
        [XmlElement("uwagi")] [XmlAnnotation("Dodatkowe informacje o adresie")] public string Description { get; set; }
        [XmlElement("data")] public AccessDateElement Date { get; set; }
    }

    [XmlType(TypeName = "dostep-typ")]
    [XmlAnnotation(@"Element zawierający status dostępności dokumentu. 
Określenie zasad i warunków regulujących dostęp do dokumentów. 
Informacja o dostępności nie może naruszać przepisów regulujących dostęp do danych (np. do danych osobowych, informacji niejawnych itd.)")]
    public class AccessElement17 {
        [XmlIgnore] public const DocumentType DOCUMENT_TYPE = DocumentType.Nes17;
        [XmlElement("dostepnosc")] [XmlRequired] public AccessType17 Access { get; set; }
        [XmlElement("uwagi")] [XmlAnnotation("Dodatkowe informacje o adresie")] public string Description { get; set; }
        [XmlElement("data")] public AccessDateElement Date { get; set; }
    }

    [XmlType(TypeName = "dostep-data-typ")]
    [XmlAnnotation("Data dostępności dokumentu.")]
    public class AccessDateElement {
        [XmlElement("typ")] public AccessDateType Type { get; set; }

        [XmlElement("czas")]
        [XmlRequired]
        [XmlAnnotation("Data dostępności dokumentu.")]
        [XmlSimpleType(TypeName = "czas-typ", UnionMemberTypes = "xs:gYear xs:gYearMonth xs:date xs:dateTime", Annotation = "Czas zapisany jako rok lub rok i miesiąc lub jako pełna data.")]
        public string Date { get; set; }
        public bool ShouldSerializeDate() { return Date.IsNotNullOrEmpty(); }
    }
}
