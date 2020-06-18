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
    [XmlType(TypeName = "tworca-typ")]
    [XmlAnnotation(@"Podmiot (instytucja lub osoba) pełniący określoną rolę w przygotowaniu dokumentu. Pozwala na identyfikację osoby fizycznej lub prawnej, która przygotowała, modyfikowała, akceptowała/zatwierdziła, podpisała dokument.")]
    public class AuthorElement {
        [XmlElement("funkcja")]
        [XmlRequired]
        [XmlAnnotation("Określenie roli podmiotu w tworzeniu treści dokumentu.")]
        [XmlSimpleType(TypeName = "tworca-funkcja-typ", Annotation = "Zgodnie z referencyjnym słownikiem  funkcji użytkowanych w podmiocie, słownik musi zawierać co najmniej następujące elementy: utworzył, modyfikował, zatwierdził, podpisał.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ", EnumerationRestriction = new string[] { "podpisał", "utworzył", "modyfikował", "zatwierdził", "opublikowany" })]
        public List<string> Functions { get; set; }

        [XmlElement("podmiot")] [XmlRequired] public SubjectElement Subject { get; set; }
    }
}
