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
using System.Xml.Serialization;

namespace Abc.Nes.Elements {
    [XmlType(TypeName = "tworca-typ")]
    [XmlAnnotation(@"Podmiot (instytucja lub osoba) pełniący określoną rolę w przygotowaniu dokumentu. Pozwala na identyfikację osoby fizycznej lub prawnej, która przygotowała, modyfikowała, akceptowała/zatwierdziła, podpisała dokument.")]
    public class AuthorElement {
        [XmlElement("funkcja")]
        [XmlRequired]
        [XmlAnnotation("Określenie roli podmiotu w tworzeniu treści dokumentu.")]
        [XmlSimpleType(TypeName = "tworca-funkcja-typ", Annotation = "Zgodnie z referencyjnym słownikiem  funkcji użytkowanych w podmiocie, słownik musi zawierać co najmniej następujące elementy: utworzył, modyfikował, zatwierdził, podpisał.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ", EnumerationRestriction = typeof(AuthorFunctionType))]
        public List<string> Functions { get; set; }

        public void SetKind(AuthorFunctionType functionType) {
            if (Functions.IsNull()) { Functions = new List<string>(); }
            Functions.Add(functionType.GetXmlEnum());
        }

        public static string GetAuthorFunctionType(AuthorFunctionType functionType) {
            return functionType.GetXmlEnum();
        }

        [XmlElement("podmiot")] [XmlRequired] public SubjectElement Subject { get; set; }
    }
}
