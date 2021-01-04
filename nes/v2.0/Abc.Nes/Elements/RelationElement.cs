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
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Abc.Nes.Elements {
    [XmlType(TypeName = "relacja-typ")]
    [XmlAnnotation(@"Określenie bezpośredniego powiązania z innym dokumentem i rodzaju tego powiązania. Pozwala na odnalezienie dokumentów, które są bezpośrednio powiązane z danym dokumentem. Na przykład kolejnych wersji tego samego dokumentu, dekretacji dokumentu, dokumentów składających się z innych dokumentów, załączników do dokumentów, potwierdzeń doręczenia dokumentu, itd.")]
    public class RelationElement : ElementBase {
        [XmlElement("identyfikator")] [XmlRequired] public List<IdentifierElement> Identifiers { get; set; }

        [XmlElement("typRelacji")]
        [XmlSynonyms("typ")]
        [XmlRequired]
        [XmlAnnotation("Określenie rodzaju powiązania.")]
        [XmlSimpleType(TypeName = "typrelacji-typ", Annotation = "Standardowe typy relacji powinny być ujęte w słownik zawierający co najmniej typy relacji określone w repozytorium interoparacyjności.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ", EnumerationRestriction = typeof(RelationType))]
        public string Type { get; set; }

        public void SetType(RelationType type) {
            Type = type.GetXmlEnum();
        }    
    }
}
