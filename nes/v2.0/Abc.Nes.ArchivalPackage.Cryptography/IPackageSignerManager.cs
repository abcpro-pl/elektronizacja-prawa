/*=====================================================================================

	ABC NES.ArchivalPackage.Cryptography 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes.ArchivalPackage.Cryptography.Model;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace Abc.Nes.ArchivalPackage.Cryptography {
    public interface IPackageSignerManager : IDisposable {
        string Sign(string sourcePackageFilePath, X509Certificate2 cert, string outputPackageFilePath = null,
                Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
                Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
                bool signPackageFiles = true,
                bool signPackageFile = true,
                bool detachedSignaturePackageFile = false,
                bool detachedSignaturePackageFiles = false,
                bool addTimeStamp = false,
                string timeStampServerUrl = "http://time.certum.pl");
        string Sign(string sourcePackageFilePath,
            X509Certificate2 cert,
            string outputPackageFilePath = null,
            Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
            Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
            string[] internalFiles = null,
            bool signPackageFile = true,
            bool detachedSignaturePackageFile = true,
            bool detachedSignaturePackageFiles = true,
            bool addTimeStamp = false,
            string timeStampServerUrl = "http://time.certum.pl");
        void SignInternalFile(string sourcePackageFilePath,
                string internalPath,
                X509Certificate2 cert,
                Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
                Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
                bool detachedSignaturePackageFile = false,
                string outputPackageFilePath = null,
                bool addTimeStamp = false,
                string timeStampServerUrl = "http://time.certum.pl");

        void SignInternalFile(
                string sourcePackageFilePath,
                ArchivalPackage.Model.ItemBase item,
                PackageManager mgr,
                X509Certificate2 cert,
                Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
                Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
                bool detachedSignaturePackageFile = false,
                string outputPackageFilePath = null,
                bool addTimeStamp = false,
                string timeStampServerUrl = "http://time.certum.pl");

        void SignPdfFile(
                string sourceFilePath,
                X509Certificate2 cert,
                string reason = "Formalne zatwierdzenie (Proof of approval)",
                string location = null,
                bool addTimeStamp = false,
                string timeStampServerUrl = "http://time.certum.pl",
                byte[] apperancePngImage = null,
                PdfSignatureLocation apperancePngImageLocation = PdfSignatureLocation.Custom,
                float apperanceLocationX = 30F,
                float apperanceLocationY = 650F,
                float apperanceWidth = 200F,
                float apperanceHeight = 50F,
                float margin = 10F,
                string outputFilePath = null);

        SignatureInfo[] GetSignatureInfos(PackageManager mgr, string internalPath);
        SignatureInfo[] GetSignatureInfos(PackageManager mgr, ArchivalPackage.Model.DocumentFile item);
        SignatureInfo[] GetSignatureInfos(string packageFilePath);
        SignatureInfo[] GetSignatureInfos(string packageFilePath, string internalPath);
        SignatureInfo[] GetXadesSignatureInfos(string xadesFilePath);
        SignatureInfo[] GetXadesSignatureInfos(XElement xades, string fileName = null);

        SignatureVerifyInfo[] VerifySignatures(string packageFilePath);
        SignatureVerifyInfo[] VerifyXadesSignature(string xadesFilePath);
    }
}
