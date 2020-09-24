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
using System.Xml.Serialization;

namespace Abc.Nes.Elements {
    [XmlType(TypeName = "odbiorca-typ")]
    [XmlAnnotation(@"Podmioty, do których dokument jest adresowany.")]
    public class RecipientElement {
		[XmlElement("doWiadomosci")]
		[XmlRequired(false)]
		[XmlAnnotation("Pozwala na wskazanie, że dany podmiot nie głównym odbiorcą dokumentu (otrzymał do wiadomości).")] 
		public BooleanValues CC { get; set; }

		[XmlElement("podmiot")] [XmlRequired] public SubjectElement Subject { get; set; }
	}

	/// <summary>
	/// The v.1.7 metadata element that describes recipient.
	/// </summary>
	[XmlType(TypeName = "odbiorca-typ")]
	[XmlAnnotation(@"Podmioty, do których dokument jest adresowany.")]
	public class RecipientElement17 {		
		[XmlElement("podmiot")] [XmlRequired] public SubjectElement Subject { get; set; }
		[XmlElement("rodzaj")] [XmlRequired] public RecipientType Kind { get; set; }
	}
}
