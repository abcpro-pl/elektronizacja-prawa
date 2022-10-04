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
    [XmlType(TypeName = "podmiot-typ")]
    [XmlAnnotation(@"Element zawierający dane podmiotu. 
Jednostka (osoba fizyczna, instytucja), która może być stroną w jakiejś czynności związanej z dokumentem (tworzenie, odbieranie, akceptacja, podpisywanie, łączenie w grupy). ")]
    public class SubjectElement {
        [XmlRequired]
        [XmlChoiceIdentifier("SubjectType")]
        [XmlElement("osoba", typeof(PersonElement))]
        [XmlElement("instytucja", typeof(InstitutionElement))]
        public object Subject { get; set; }

        [XmlIgnore] public SubjectType SubjectType;
        [XmlIgnore] public PersonElement Person { get { return Subject as PersonElement; } set { SubjectType = SubjectType.osoba; Subject = value; } }
        [XmlIgnore] public InstitutionElement Institution { get { return Subject as InstitutionElement; } set { SubjectType = SubjectType.instytucja; Subject = value; } }
    }

    [XmlType(TypeName = "podmiot-typ")]
    [XmlAnnotation(@"Element zawierający dane podmiotu. 
Jednostka (osoba fizyczna, instytucja), która może być stroną w jakiejś czynności związanej z dokumentem (tworzenie, odbieranie, akceptacja, podpisywanie, łączenie w grupy). ")]
    public class SubjectElement16 {
        [XmlRequired]
        [XmlChoiceIdentifier("SubjectType")]
        [XmlElement("osoba", typeof(PersonElement16))]
        [XmlElement("instytucja", typeof(InstitutionElement16))]
        public object Subject { get; set; }

        [XmlIgnore] public SubjectType SubjectType;
        [XmlIgnore] public PersonElement16 Person { get { return Subject as PersonElement16; } set { SubjectType = SubjectType.osoba; Subject = value; } }
        [XmlIgnore] public InstitutionElement16 Institution { get { return Subject as InstitutionElement16; } set { SubjectType = SubjectType.instytucja; Subject = value; } }
    }

    [XmlType(IncludeInSchema = false)]
    public enum SubjectType {
        osoba,
        instytucja
    }
}
