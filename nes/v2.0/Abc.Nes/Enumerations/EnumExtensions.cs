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

namespace Abc.Nes {
    public static class EnumExtensions {
        public static string GetName(this AccessDateType type) { return type.GetXmlEnum(); }
        public static string GetName(this AccessType17 type) { return type.GetXmlEnum(); }
        public static string GetName(this AccessType type) { return type.GetXmlEnum(); }
        public static string GetName(this ContactType type) { return type.GetXmlEnum(); }
        public static string GetName(this CountryCodeType type) { return type.GetXmlEnum(); }
        public static string GetName(this EventDateType type) { return type.GetXmlEnum(); }
        public static string GetName(this DateRangeType type) { return type.GetXmlEnum(); }
        public static string GetName(this DocumentDateType type) { return type.GetXmlEnum(); }
        public static string GetName(this DocumentDateType17 type) { return type.GetXmlEnum(); }
        public static string GetName(this InstitutionIdType type) { return type.GetXmlEnum(); }
        public static string GetName(this IdTypes idType) { return idType.GetXmlEnum(); }
        public static string GetName(this PersonIdType type) { return type.GetXmlEnum(); }
        public static string GetName(this ArchivalCategoryType type) { return type.GetXmlEnum(); }
        public static string GetName(this BooleanValues type) { return type.GetXmlEnum(); }
        public static string GetName(this RecipientType type) { return type.GetXmlEnum(); }
        public static string GetName(this RelationType type) { return type.GetXmlEnum(); }
        public static string GetName(this FileSizeType type) { return type.GetXmlEnum(); }
        public static string GetName(this LanguageCode type) { return type.ToString(); }
        public static string GetName(this DocumentKindType type) { return type.GetXmlEnum(); }
        public static string GetName(this DocumentType type) { return type.GetXmlEnum(); }
        public static string GetName(this DocumentClassType type) { return type.GetXmlEnum(); }
        public static string GetName(this AuthorFunctionType type) { return type.GetXmlEnum(); }
    }
}
