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
    [XmlType(TypeName = "wielkosc-format-typ")]
    [XmlAnnotation("Element definiujący wielkość dokumentu. Posiada atrybut miara określający miarę wielkości.")]
    public class SizeElement : ElementBase {
        [XmlText] public string Value { get; set; } = String.Empty;

        [XmlAttribute("miara")]
        [XmlSynonyms("jednostka", DocumentType.Nes17)]
        [XmlSynonyms("jednostka", DocumentType.Nes16)]
        [XmlRequired]
        [XmlSimpleType(Annotation = "Miara wielkości dokumentu.", TypeName = "miara-typ", UnionMemberTypes = "ndap:niepusty-ciag-typ", BaseTypeName = "xs:string", EnumerationRestriction = typeof(FileSizeType)/*new string[] { "bajt", "b", "kB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" }*/)]
        public string Measure { get; set; }

        public void SetMeasure(FileSizeType sizeType) { Measure = sizeType.GetName(); }
    }
}
