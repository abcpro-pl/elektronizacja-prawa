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
        public static string GetAccessDataType(this AccessDateType type) { return type.GetXmlEnum(); }
        public static string GetAccessType17(this AccessType17 type) { return type.GetXmlEnum(); }
        public static string GetAccessType(this AccessType type) { return type.GetXmlEnum(); }
        public static string GetContactType(this ContactType type) { return type.GetXmlEnum(); }
        public static string GetEventDateType(this EventDateType type) { return type.GetXmlEnum(); }
        public static string GetDateRangeType(this DateRangeType type) { return type.GetXmlEnum(); }
        public static string GetDocumentDateType(this DocumentDateType type) { return type.GetXmlEnum(); }
        public static string GetInstitutionIdType(this InstitutionIdType type) { return type.GetXmlEnum(); }
        public static string GetIdTypes(this IdTypes idType) { return idType.GetXmlEnum(); }
        public static string GetPersonIdType(this PersonIdType type) { return type.GetXmlEnum(); }
        public static string GetArchivalCategoryType(this ArchivalCategoryType type) { return type.GetXmlEnum(); }
        public static string GetBooleanValues(this BooleanValues type) { return type.GetXmlEnum(); }
        public static string GetRecipientType(this RecipientType type) { return type.GetXmlEnum(); }
        public static string GetRelationType(this RelationType type) { return type.GetXmlEnum(); }
        public static string GetSizeType(this FileSizeType type) { return type.GetXmlEnum(); }
        public static string GetLanguageCode(this LanguageCode type) { return type.ToString(); }
        public static string GetDocumentKindType(this DocumentKindType type) { return type.GetXmlEnum(); }
        public static string GetDocumentClassType(this DocumentClassType type) { return type.GetXmlEnum(); }
    }
}
