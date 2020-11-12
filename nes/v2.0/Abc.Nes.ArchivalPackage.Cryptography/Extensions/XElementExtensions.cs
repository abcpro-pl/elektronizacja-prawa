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
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Abc.Nes.ArchivalPackage.Cryptography {
    static class XElementExtensions {
        public static XElement ToXElement(this byte[] bytes) {
            if (bytes != null) {
                try {
                    using (MemoryStream ms = new MemoryStream(bytes)) {
                        using (TextReader sr = new StreamReader(ms)) {
                            var text = sr.ReadToEnd();
                            text = text.Replace("> </", ">\u00A0</");

                            var test = XElement.Parse(text);
                            if (test != null) {
                                return test;
                            }
                        }
                    }
                }
                catch {
                    // wczytanie jako tekst
                    try {
                        var text = bytes.ToText();
                        text = text.Replace("> </", ">\u00A0</");

                        var test = XElement.Parse(text.Trim());
                        if (test != null) {
                            return test;
                        }
                    }
                    catch { }
                }
            }

            return null;
        }

        public static string ToText(this byte[] bytes) {
            if (bytes!=null) {
                return Encoding.UTF8.GetString(bytes);
            }
            return String.Empty;
        }
    }
}
