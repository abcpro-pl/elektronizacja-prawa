/*=====================================================================================

	ABC NES.ArchivalPackage 
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
using System.Xml.Serialization;

namespace Abc.Nes.ArchivalPackage {
    static class SystemExtensions {
        public static string FromByteArray(this byte[] bytes) {
            if (bytes.IsNotNull()) {
                return Encoding.UTF8.GetString(bytes);
            }
            return default;
        }
        public static byte[] ToByteArray(this XElement e) {
            if (e.IsNotNull()) {
                return Encoding.UTF8.GetBytes(e.ToString());
            }
            return default;
        }
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
        public static bool IsNotNullOrEmpty(this string text) { return !String.IsNullOrEmpty(text); }
        public static bool IsNullOrEmpty(this string text) { return String.IsNullOrEmpty(text); }
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
        public static string RemovePolishChars(this string source) {
            string result = String.Empty;

            if (source != null) {
                if (source.HasPolishChars()) {
                    foreach (char c in source) {
                        switch (c) {
                            case 'ą': { result += 'a'; break; }
                            case 'ę': { result += 'e'; break; }
                            case 'ź': { result += 'z'; break; }
                            case 'ć': { result += 'c'; break; }
                            case 'ż': { result += 'z'; break; }
                            case 'ń': { result += 'n'; break; }
                            case 'ł': { result += 'l'; break; }
                            case 'ó': { result += 'o'; break; }
                            case 'ś': { result += 's'; break; }

                            case 'Ą': { result += 'A'; break; }
                            case 'Ę': { result += 'E'; break; }
                            case 'Ź': { result += 'Z'; break; }
                            case 'Ć': { result += 'C'; break; }
                            case 'Ż': { result += 'Z'; break; }
                            case 'Ń': { result += 'N'; break; }
                            case 'Ł': { result += 'L'; break; }
                            case 'Ó': { result += 'O'; break; }
                            case 'Ś': { result += 'S'; break; }

                            default: { result += c; break; }
                        }
                    }
                }
                else {
                    result = source;
                }
            }

            return result;
        }
        public static string RemoveIllegalCharacters(this string source) {
            return source.Replace("\\", String.Empty)
               .Replace("/", String.Empty)
               .Replace("*", String.Empty)
               .Replace("?", String.Empty)
               .Replace(":", String.Empty)
               .Replace("=", String.Empty)
               .Replace(";", String.Empty)
               .Replace(",", "");

        }
        public static bool HasPolishChars(this string source) {
            return source.ContainsInTable(true, false, "ą", "ę", "ź", "ć", "ż", "ń", "ł", "ó", "ś");
        }

    }
}
