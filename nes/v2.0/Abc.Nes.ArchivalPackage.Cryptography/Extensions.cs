/*=====================================================================================

	ABC NES.ArchivalPackage.Cryptography 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/


using System;
using System.Xml.Serialization;

namespace Abc.Nes.ArchivalPackage.Cryptography {
    static class Extensions {
        public static string GetXmlEnum(this Enum value) {
            try {
                var fi = value.GetType().GetField(value.ToString());
                XmlEnumAttribute[] attributes = (XmlEnumAttribute[])fi.GetCustomAttributes(typeof(XmlEnumAttribute), false);
                return (attributes.Length > 0) ? attributes[0].Name : value.ToString();
            }
            catch {
                return string.Empty;
            }
        }
    }
}