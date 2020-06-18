﻿/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using System.Xml.Serialization;

namespace Abc.Nes.Enumerations {
    [XmlType(TypeName = "kod-jezyka-typ")]
    [XmlAnnotation("Słownik kodów języka, w którym sporządzono dokument.")]
    public enum LanguageCode {
        abk,
        ace,
        ach,
        ada,
        aar,
        afh,
        afr,
        afa,
        aka,
        akk,
        alb,
        ale,
        alg,
        tut,
        amh,
        apa,
        ara,
        arc,
        arp,
        arn,
        arw,
        arm,
        art,
        asm,
        ast,
        ath,
        aus,
        map,
        ava,
        ave,
        awa,
        aym,
        aze,
        ban,
        bat,
        bal,
        bam,
        bai,
        bad,
        bnt,
        bas,
        bak,
        baq,
        btk,
        bej,
        bel,
        bem,
        ben,
        ber,
        bho,
        bih,
        bik,
        bin,
        bis,
        nob,
        bos,
        bra,
        bre,
        bug,
        bul,
        bua,
        bur,
        cad,
        car,
        spa,
        cat,
        cau,
        ceb,
        cel,
        cai,
        chg,
        cmc,
        cha,
        che,
        chr,
        nya,
        chy,
        chb,
        chi,
        chn,
        chp,
        cho,
        zha,
        chu,
        chk,
        chv,
        cop,
        cor,
        cos,
        cre,
        mus,
        crp,
        cpe,
        cpf,
        cpp,
        scr,
        cus,
        cze,
        dak,
        dan,
        day,
        del,
        din,
        div,
        doi,
        dgr,
        dra,
        dua,
        dut,
        dum,
        dyu,
        dzo,
        efi,
        egy,
        eka,
        elx,
        eng,
        enm,
        ang,
        epo,
        est,
        ewe,
        ewo,
        fan,
        fat,
        fao,
        fij,
        fin,
        fiu,
        fon,
        fre,
        frm,
        fro,
        fry,
        fur,
        ful,
        gaa,
        gla,
        glg,
        lug,
        gay,
        gba,
        gez,
        geo,
        ger,
        nds,
        gmh,
        goh,
        gem,
        kik,
        gil,
        gon,
        gor,
        got,
        grb,
        grc,
        gre,
        grn,
        guj,
        gwi,
        hai,
        hau,
        haw,
        heb,
        her,
        hil,
        him,
        hin,
        hmo,
        hit,
        hmn,
        hun,
        hup,
        iba,
        ice,
        ido,
        ibo,
        ijo,
        ilo,
        smn,
        inc,
        ine,
        ind,
        ina,
        ile,
        iku,
        ipk,
        ira,
        gle,
        mga,
        sga,
        iro,
        ita,
        jpn,
        jav,
        jrb,
        jpr,
        kab,
        kac,
        kal,
        kam,
        kan,
        kau,
        kaa,
        kar,
        kas,
        kaw,
        kaz,
        kha,
        khm,
        khi,
        kho,
        kmb,
        kin,
        kir,
        kom,
        kon,
        kok,
        kor,
        kos,
        kpe,
        kro,
        kua,
        kum,
        kur,
        kru,
        kut,
        lad,
        lah,
        lam,
        lao,
        lat,
        lav,
        ltz,
        lez,
        lin,
        lit,
        loz,
        lub,
        lua,
        lui,
        smj,
        lun,
        luo,
        lus,
        mac,
        mad,
        mag,
        mai,
        mak,
        mlg,
        may,
        mal,
        mlt,
        mnc,
        mdr,
        man,
        mni,
        mno,
        glv,
        mao,
        mar,
        chm,
        mah,
        mwr,
        mas,
        myn,
        men,
        mic,
        min,
        mis,
        moh,
        mol,
        mkh,
        lol,
        mon,
        mos,
        mul,
        mun,
        nah,
        nau,
        nav,
        nde,
        nbl,
        ndo,
        nep,
        nia,
        nic,
        ssa,
        niu,
        non,
        nai,
        sme,
        nor,
        nno,
        nub,
        nym,
        nyn,
        nyo,
        nzi,
        oci,
        oji,
        ori,
        orm,
        osa,
        oss,
        oto,
        pal,
        pau,
        pli,
        pam,
        pag,
        pan,
        pap,
        paa,
        per,
        peo,
        phi,
        phn,
        pon,
        pol,
        por,
        pra,
        pro,
        pus,
        que,
        roh,
        raj,
        rap,
        rar,
        roa,
        rum,
        rom,
        run,
        rus,
        sal,
        sam,
        smi,
        smo,
        sad,
        sag,
        san,
        sat,
        srd,
        sas,
        sco,
        sel,
        sem,
        scc,
        srr,
        shn,
        sna,
        sid,
        sgn,
        bla,
        snd,
        sin,
        sit,
        sio,
        sms,
        den,
        sla,
        slo,
        slv,
        sog,
        som,
        son,
        snk,
        wen,
        nso,
        sot,
        sai,
        sma,
        suk,
        sux,
        sun,
        sus,
        swa,
        ssw,
        swe,
        syr,
        tgl,
        tah,
        tai,
        tgk,
        tmh,
        tam,
        tat,
        tel,
        ter,
        tet,
        tha,
        tib,
        tig,
        tir,
        tem,
        tiv,
        tli,
        tpi,
        tkl,
        tog,
        ton,
        tsi,
        tso,
        tsn,
        tum,
        tup,
        tur,
        ota,
        tuk,
        tvl,
        tyv,
        twi,
        uga,
        uig,
        ukr,
        umb,
        und,
        urd,
        uzb,
        vai,
        ven,
        vie,
        vol,
        vot,
        wak,
        wal,
        wln,
        war,
        was,
        wel,
        wol,
        xho,
        sah,
        yao,
        yap,
        yid,
        yor,
        ypk,
        znd,
        zap,
        zen,
        zul,
        zun
    }
}
