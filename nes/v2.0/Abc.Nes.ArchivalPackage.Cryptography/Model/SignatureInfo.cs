/*=====================================================================================

	ABC NES.ArchivalPackage.Cryptography 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes.Cryptography;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Abc.Nes.ArchivalPackage.Cryptography.Model {
    public class SignatureInfo {
        public string Author { get; internal set; }
        public DateTime CreateDate { get; internal set; }
        public SignatureType SignatureType { get; internal set; }
        public string Publisher { get; internal set; }
        public string SignatureNumber { get; internal set; }
        public X509Certificate2 Certificate { get; internal set; }
        public string FileName { get; internal set; }

        public string CommitmentTypeIndication { get; internal set; }
        public string ClaimedRole { get; internal set; }
        public string Organization { get; internal set; }
    }

    public class SignatureVerifyInfo {
        public string FileName { get; internal set; }
        public string SignatureName { get; internal set; }
        public bool IsValid { get; internal set; }
        public CertificateValidationInfo CertValidationInfo { get; internal set; }
        public string Message { get; internal set; }
    }

    public class SignAndVerifyInfo {
        public SignatureInfo[] SignInfo { get; internal set; }
        public SignatureVerifyInfo[] VerifyInfo { get; internal set; }
    }
}
