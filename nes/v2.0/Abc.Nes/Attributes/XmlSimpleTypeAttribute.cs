﻿/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System.Collections.Generic;

namespace System.Xml.Serialization {
    [AttributeUsage(AttributeTargets.Property)]
    public class XmlSimpleTypeAttribute : Attribute {
        public string Prefix { get; set; } = "ndap";
        public string TypeName { get; set; }
        public string BaseTypeName { get; set; } = "xs:string";
        public int MinLength { get; set; }
        public string Pattern { get; set; }
        public List<string> Enumeration { get; set; }
        public string Annotation { get; set; }
        public XmlSimpleTypeAttribute() { }
    }
}
