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
    [XmlType(TypeName = "data-przypisania-typ")]
    [XmlAnnotation(@"Element zawierający dane o dacie dokumentu. 
Umożliwia wyszukiwanie i sortowanie dokumentów lub ich grupy według czasu zdarzeń z nimi związanych, jak również wyszukiwanie / filtrowanie wg rodzajów zdarzeń.")]
    [XmlChoice]
    public class DateElement {
        [XmlIgnore] public const DocumentType DOCUMENT_TYPE = DocumentType.Nes20;

        [XmlGroup(Name = "data-zdarzenia-grupa", Annotation = "Data zdarzenia.")]
        [XmlElement("typDaty")]
        [XmlRequired]
        public EventDateType Type { get; set; }
        public bool ShouldSerializeType() { return Type != EventDateType.None; }

        [XmlGroup(Name = "data-zdarzenia-grupa")]
        [XmlElement("czas")]
        [XmlRequired]
        [XmlAnnotation("Data zdarzenia")]
        [XmlSimpleType(TypeName = "czas-typ", UnionMemberTypes = "xs:gYear xs:gYearMonth xs:date xs:dateTime", Annotation = "Czas zapisany jako rok lub rok i miesiąc lub jako pełna data.")]
        public string Date { get; set; }
        public bool ShouldSerializeDate() { return Date.IsNotNullOrEmpty(); }

        [XmlGroup(Name = "daty-zakres-grupa", Annotation = "Zakres dat.")]
        [XmlElement("zakresDat")]
        [XmlRequired]
        public DateRangeType Range { get; set; }
        public bool ShouldSerializeRange() { return Range != DateRangeType.None; }

        [XmlGroup(Name = "daty-zakres-grupa")]
        [XmlElement("czasOd")]
        [XmlRequired]
        [XmlAnnotation("Data skrajna Od")]
        [XmlSimpleType(TypeName = "czas-typ", UnionMemberTypes = "xs:gYear xs:gYearMonth xs:date xs:dateTime", Annotation = "Czas zapisany jako rok lub rok i miesiąc lub jako pełna data.")]
        public string DateFrom { get; set; }
        public bool ShouldSerializeDateFrom() { return DateFrom.IsNotNullOrEmpty(); }

        [XmlGroup(Name = "daty-zakres-grupa")]
        [XmlElement("czasDo")]
        [XmlRequired]
        [XmlAnnotation("Data skrajna Do")]
        [XmlSimpleType(TypeName = "czas-typ", UnionMemberTypes = "xs:gYear xs:gYearMonth xs:date xs:dateTime", Annotation = "Czas zapisany jako rok lub rok i miesiąc lub jako pełna data.")]
        public string DateTo { get; set; }
        public bool ShouldSerializeDateTo() { return DateTo.IsNotNullOrEmpty(); }

        public DateTime GetDate() { try { return Convert.ToDateTime(Date); } catch { } return default; }
        public DateTime GetDateFrom() { try { return Convert.ToDateTime(DateFrom); } catch { } return default; }
        public DateTime GetDateTo() { try { return Convert.ToDateTime(DateTo); } catch { } return default; }
    }

    /// <summary>
    /// The v.1.7 metadata element that describes date information.
    /// </summary>
    [XmlType(TypeName = "data-dokumentu-typ")]
    [XmlAnnotation(@"Element zawierający dane o dacie dokumentu. 
Umożliwia wyszukiwanie i sortowanie dokumentów lub ich grupy według czasu zdarzeń z nimi związanych, jak również wyszukiwanie / filtrowanie wg rodzajów zdarzeń.")]
    [XmlChoice]
    public class DateElement17 {
        [XmlIgnore] public const DocumentType DOCUMENT_TYPE = DocumentType.Nes17;


        [XmlElement("typ")]
        [XmlRequired]
        public DocumentDateType17 Type { get; set; }
        public bool ShouldSerializeType() { return Type != DocumentDateType17.None; }

        // -----------------------------data-dokumentu-grupa ------------------------------------       
        [XmlGroup(Name = "data-dokumentu-grupa")]
        [XmlElement("czas")]
        [XmlRequired]
        [XmlAnnotation("Data dokumentu")]
        [XmlSimpleType(TypeName = "czas-typ", UnionMemberTypes = "xs:gYear xs:gYearMonth xs:date xs:dateTime", Annotation = "Czas zapisany jako rok lub rok i miesiąc lub jako pełna data.")]
        public string Date { get; set; }
        // -----------------------------data-dokumentu-grupa ------------------------------------


        // -----------------------------przedzial-czasu-grupa -----------------------------------
        [XmlGroup(Name = "przedzial-czasu-grupa")]
        [XmlElement("od")]
        [XmlRequired]
        [XmlAnnotation("Data skrajna Od")]
        [XmlSimpleType(TypeName = "czas-typ", UnionMemberTypes = "xs:gYear xs:gYearMonth xs:date xs:dateTime", Annotation = "Czas zapisany jako rok lub rok i miesiąc lub jako pełna data.")]
        public string DateFrom { get; set; }
        public bool ShouldSerializeDateFrom() { return DateFrom.IsNotNullOrEmpty(); }

        [XmlGroup(Name = "przedzial-czasu-grupa")]
        [XmlElement("do")]
        [XmlRequired]
        [XmlAnnotation("Data skrajna Do")]
        [XmlSimpleType(TypeName = "czas-typ", UnionMemberTypes = "xs:gYear xs:gYearMonth xs:date xs:dateTime", Annotation = "Czas zapisany jako rok lub rok i miesiąc lub jako pełna data.")]
        public string DateTo { get; set; }
        public bool ShouldSerializeDateTo() { return DateTo.IsNotNullOrEmpty(); }

        // -----------------------------przedzial-czasu-grupa -----------------------------------

        public DateTime GetDate() { try { return Convert.ToDateTime(Date); } catch { } return default; }
        public DateTime GetDateFrom() { try { return Convert.ToDateTime(DateFrom); } catch { } return default; }
        public DateTime GetDateTo() { try { return Convert.ToDateTime(DateTo); } catch { } return default; }
    }
}
