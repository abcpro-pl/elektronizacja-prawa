/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

namespace System.Xml.Serialization {
    [AttributeUsage(AttributeTargets.Property)]
    public class XmlRequiredAttribute : Attribute {
        public bool Required { get; set; } = true;
        public XmlRequiredAttribute() { }
        public XmlRequiredAttribute(bool required) : this() {
            Required = required;
        }
    }
}
