/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace System {
    static class SystemExtensions {
        public static string GenerateId(this string text, int length = 0, bool toLower = false) {
            if (text is null) { throw new ArgumentNullException(nameof(text)); }

            text = Guid.NewGuid().ToString().Replace("{", String.Empty).Replace("}", String.Empty).ToUpper().Trim();
            if (length < 1 || length > text.Replace("-", String.Empty).Length) {
                return toLower ? text.ToLower() : text;
            }

            var s = text.Replace("-", String.Empty).Substring(0, length);
            return toLower ? s.ToLower() : s;
        }
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
        public static byte[] ToByteArray(this string text) {
            return Encoding.UTF8.GetBytes(text);
        }
        public static MemoryStream GetMemoryStream(this string text) {
            return new MemoryStream(text.ToByteArray());
        }
        public static bool IsNull(this object o) { return o == null; }
        public static bool IsNotNull(this object o) { return o != null; }
        public static bool IsNullOrEmpty(this string text) { return String.IsNullOrEmpty(text); }
        public static bool IsNotNullOrEmpty(this string text) { return !String.IsNullOrEmpty(text); }
        public static bool ContainsInTable(this string text, bool ignoreCase, bool equals, params string[] strings) {
            if (!String.IsNullOrEmpty(text)) {
                if (strings != null) {
                    foreach (string s in strings) {
                        if (ignoreCase) {
                            if (equals) {
                                if (s.ToLower() == text.ToLower()) {
                                    return true;
                                }
                            }
                            else {
                                if (text.ToLower().Contains(s.ToLower())) {
                                    return true;
                                }
                            }
                        }
                        else {
                            if (equals) {
                                if (s == text) {
                                    return true;
                                }
                            }
                            else {
                                if (text.Contains(s)) {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
