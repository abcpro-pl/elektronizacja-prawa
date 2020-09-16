﻿/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes.Elements;
using Abc.Nes.Enumerations;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Abc.Nes {
    /// <summary>
    /// The root v.1.7 metadata element that describes document.
    /// </summary>
    [XmlType(TypeName = "dokument-typ")]
    [XmlAnnotation("Element główny metadanych opisujący dokument.")]
    [XmlRoot(ElementName = "dokument", Namespace = "http://www.mswia.gov.pl/standardy/ndap")]
    public class Document17 : IDocument {
        [XmlIgnore] public DocumentType Type => DocumentType.Nes17;
        [XmlElement("odbiorca")] public List<RecipientElement17> Recipients { get; set; }
        [XmlElement("data")] [XmlRequired] public List<DateElement17> Dates { get; set; }
        [XmlElement("dostep")] [XmlRequired] public List<AccessElement> Access { get; set; }
        [XmlElement("format")] [XmlRequired] public List<FormatElement> Formats { get; set; }
        [XmlElement("grupowanie")] public List<GroupingElement> Groupings { get; set; }
        [XmlElement("identyfikator")] [XmlRequired] public List<IdentifierElement> Identifiers { get; set; }
        [XmlElement("jezyk")] [XmlAnnotation("Określenie języka naturalnego zgodnie z normą ISO-639-2, użytego w dokumencie.")] public List<TitleWithLanguageCodeElement> Languages { get; set; }

        [XmlElement("opis")]
        [XmlAnnotation("Streszczenie, spis treści lub krótki opis treści dokumentu.")]
        [XmlSimpleType(TypeName = "dokument-opis-typ", Annotation = "Opis dokumentu.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ")]
        public string Description { get; set; }


        [XmlElement("uprawnienia")]
        [XmlAnnotation("Wskazanie podmiotu uprawnionego do dysponowania treścią dokumentu.")]
        [XmlSimpleType(TypeName = "uprawnienia-typ", Annotation = "Wskazanie podmiotu uprawnionego do dysponowania treścią dokumentu.", BaseTypeName = "xs:string", UnionMemberTypes = "ndap:niepusty-ciag-typ")]
        public List<string> Rights { get; set; }

        [XmlElement("kwalifikacja")] public List<QualificationElement> Qualifications { get; set; }
        [XmlElement("relacja")] public List<RelationElement> Relations { get; set; }
        [XmlElement("status")] [XmlAnnotation("Status dokumentu.")] public List<StatusElement> Statuses { get; set; }
        [XmlElement("tematyka")] public List<KeywordElement> Keywords { get; set; }
        [XmlElement("tworca")] [XmlRequired] public List<AuthorElement> Authors { get; set; }
        [XmlElement("typ")] [XmlRequired] public List<TypeElement> Types { get; set; }
        [XmlElement("tytul")] [XmlRequired] public List<TitleElement> Titles { get; set; }
    }
}
