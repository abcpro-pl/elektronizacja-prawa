/*=====================================================================================
		
	Autor: (C)2009-2022 ITORG Krzysztof Radzimski
	http://itorg.pl

  ===================================================================================*/

using System;
using System.Text;

namespace MyFirstPlugin.Extensions {
    static class StringExtensionsClass {
        public static bool IsNull(this object o) {
            return o == null;
        }
        public static bool IsNotNull(this object o) {
            return o != null;
        }
        public static bool IsNotNullOrEmpty(this string text) {
            return !String.IsNullOrEmpty(text);
        }
        public static bool IsNullOrEmpty(this string text) {
            return String.IsNullOrEmpty(text);
        }
        public static byte[] ToByteArray(this string text) {
            if (text.IsNotNull()) {
                return Encoding.UTF8.GetBytes(text);
            }

            return null;
        }
        public static string FromByteArray(this byte[] bytes) {
            if (bytes.IsNotNull()) {
                return Encoding.UTF8.GetString(bytes);
            }

            return String.Empty;
        }
        public static string GenerateId() {
            return Guid.NewGuid().ToString().Replace("{", String.Empty).Replace("}", String.Empty).ToUpper().Trim();
        }

        public static int ToInt(string text) {
            try {
                return Convert.ToInt32(text);
            }
            catch {
                return 0;
            }
        }

        public static bool ToBool(string text) {
            try {
                return Convert.ToBoolean(text);
            }
            catch {
                return false;
            }
        }

        public static bool HasPolishChars(this string source) {
            return source.ContainsInTable(true, false, "ą", "ę", "ź", "ć", "ż", "ń", "ł", "ó", "ś");
        }

        public static string RemovePolishChars(this string source) {
            string s = "";

            if (source != null) {
                if (source.HasPolishChars()) {
                    foreach (char c in source) {
                        switch (c) {
                            case 'ą': { s += 'a'; break; }
                            case 'ę': { s += 'e'; break; }
                            case 'ź': { s += 'z'; break; }
                            case 'ć': { s += 'c'; break; }
                            case 'ż': { s += 'z'; break; }
                            case 'ń': { s += 'n'; break; }
                            case 'ł': { s += 'l'; break; }
                            case 'ó': { s += 'o'; break; }
                            case 'ś': { s += 's'; break; }

                            case 'Ą': { s += 'A'; break; }
                            case 'Ę': { s += 'E'; break; }
                            case 'Ź': { s += 'Z'; break; }
                            case 'Ć': { s += 'C'; break; }
                            case 'Ż': { s += 'Z'; break; }
                            case 'Ń': { s += 'N'; break; }
                            case 'Ł': { s += 'L'; break; }
                            case 'Ó': { s += 'O'; break; }
                            case 'Ś': { s += 'S'; break; }

                            default: { s += c; break; }
                        }
                    }
                }
                else {
                    s = source;
                }
            }

            return s;
        }

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

        public static string GetDateName(this DateTime date, string datePrefix) {
            string monthName = System.Globalization.DateTimeFormatInfo.CurrentInfo.MonthGenitiveNames[date.Month - 1];
            return String.Format("{3} {0} {1} {2} r.", date.Day, monthName, date.Year, datePrefix);
        }
    }
}
