﻿/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System;
using System.Xml.Serialization;

namespace Abc.Nes.Elements {
    [XmlType(TypeName = "adres-typ")]
    [XmlAnnotation("Element zawierający dane adresowe.")]
    public class AddressElement : ElementBase {
        [XmlElement("kodPocztowy")] [XmlAnnotation("Kod pocztowy")] public string ZipCode { get; set; }
        [XmlElement("poczta")] [XmlAnnotation("Nazwa urzędu pocztowego")] public string PostName { get; set; }
        [XmlElement("miejscowosc")] [XmlAnnotation("Nazwa miejscowości")] [XmlRequired] [XmlSimpleType(TypeName = "niepusty-ciag-typ", BaseTypeName = "xs:string", MinLength = 1, Pattern = @"(\r|\n|.)*\S(\r|\n|.)*", Annotation = "Typ definiujący nie pusty ciąg znaków.")] public string Location { get; set; }
        [XmlElement("ulica")] [XmlAnnotation("Nazwa ulicy")] public string StreetName { get; set; }
        [XmlElement("budynek")] [XmlAnnotation("Numer budynku")] public string BuildingNo { get; set; }
        [XmlElement("lokal")] [XmlAnnotation("Numer miszkania")] public string AppartmentNo { get; set; }
        [XmlElement("skrytkaPocztowa")] [XmlAnnotation("Adres skrytki pocztowej")] public string PostBox { get; set; }
        [XmlElement("uwagi")] [XmlAnnotation("Dodatkowe informacje o adresie")] public string Description { get; set; }
        [XmlElement("kraj")] [XmlAnnotation("Kod kraju")] public CountryCodeType Country { get; set; } = CountryCodeType.PL;

    }

    [XmlType(TypeName = "kraj-typ")]
    [XmlAnnotation("Strownik skrótów państw.")]
    public enum CountryCodeType {
        AF,
        AX,
        AL,
        DZ,
        AS,
        AD,
        AO,
        AI,
        AQ,
        AG,
        AR,
        AM,
        AW,
        AU,
        AT,
        AZ,
        BS,
        BH,
        BD,
        BB,
        BY,
        BE,
        BZ,
        BJ,
        BM,
        BT,
        BO,
        BA,
        BW,
        BV,
        BR,
        IO,
        BN,
        BG,
        BF,
        BI,
        KH,
        CM,
        CA,
        CV,
        KY,
        CF,
        TD,
        CL,
        CN,
        CX,
        CC,
        CO,
        KM,
        CG,
        CD,
        CK,
        CR,
        CI,
        HR,
        CU,
        CY,
        CZ,
        DK,
        DJ,
        DM,
        DO,
        EC,
        EG,
        SV,
        GQ,
        ER,
        EE,
        ET,
        FK,
        FO,
        FJ,
        FI,
        FR,
        GF,
        PF,
        TF,
        GA,
        GM,
        GE,
        DE,
        GH,
        GI,
        GR,
        GL,
        GD,
        GP,
        GU,
        GT,
        GG,
        GN,
        GW,
        GY,
        HT,
        HM,
        VA,
        HN,
        HK,
        HU,
        IS,
        IN,
        ID,
        IR,
        IQ,
        IE,
        IM,
        IL,
        IT,
        JM,
        JP,
        JE,
        JO,
        KZ,
        KE,
        KI,
        KP,
        KR,
        KW,
        KG,
        LA,
        LV,
        LB,
        LS,
        LR,
        LY,
        LI,
        LT,
        LU,
        MO,
        MK,
        MG,
        MW,
        MY,
        MV,
        ML,
        MT,
        MH,
        MQ,
        MR,
        MU,
        YT,
        MX,
        FM,
        MD,
        MC,
        MN,
        MS,
        MA,
        MZ,
        MM,
        NA,
        NR,
        NP,
        NL,
        AN,
        NC,
        NZ,
        NI,
        NE,
        NG,
        NU,
        NF,
        MP,
        NO,
        OM,
        PK,
        PW,
        PS,
        PA,
        PG,
        PY,
        PE,
        PH,
        PN,
        PL,
        PT,
        PR,
        QA,
        RE,
        RO,
        RU,
        RW,
        SH,
        KN,
        LC,
        PM,
        VC,
        WS,
        SM,
        ST,
        SA,
        SN,
        CS,
        SC,
        SL,
        SG,
        SK,
        SI,
        SB,
        SO,
        ZA,
        GS,
        ES,
        LK,
        SD,
        SR,
        SJ,
        SZ,
        SE,
        CH,
        SY,
        TW,
        TJ,
        TZ,
        TH,
        TL,
        TG,
        TK,
        TO,
        TT,
        TN,
        TR,
        TM,
        TC,
        TV,
        UG,
        UA,
        AE,
        GB,
        US,
        UM,
        UY,
        UZ,
        VU,
        VE,
        VN,
        VG,
        VI,
        WF,
        EH,
        YE,
        ZM,
        ZW
    }
}
