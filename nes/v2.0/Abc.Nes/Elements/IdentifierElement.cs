/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System.Xml.Serialization;

namespace Abc.Nes.Elements {
    [XmlType(TypeName = "identyfikator-typ")]
    [XmlAnnotation(@"Element zawierający identyfikator wg. ustalonej typologii oraz opcjonalnie podmiot, który nadał.
Umożliwia jednoznaczną identyfikację dokumentu w określonym zbiorze. 
Pozwala na szybkie odnalezienie tego samego dokumentu na podstawie jego identyfikatora w różnym czasie. 
Stwarza podstawy do tworzenia jednoznacznych powiązań (relacji) między dokumentami w określonym zbiorze np. przyporządkowując załączniki, notatki, potwierdzenia wysłania, potwierdzenia doręczenia, podpisy elektronicznie, opinie itd.")]
    public class IdentifierElement {
        [XmlElement("typidentyfikatora")] [XmlAnnotation("Typ identyfikatora np. Znak sprawy.")] [XmlRequired] public string Type { get; set; }
        [XmlElement("wartoscId")] [XmlAnnotation("Wartość identyfikatora np. ABC-A.123.77.3.2011.JW.")] [XmlRequired] public string Value { get; set; }
        [XmlElement("podmiot")] public SubjectElement Subject { get; set; }
    }
}
