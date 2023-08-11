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
using Abc.Nes.Common;
using Abc.Nes.Xades.Signature.Parameters;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace Abc.Nes.ArchivalPackage.Cryptography {
    public interface IPackageSignerManager : IDisposable {
        string Sign(string sourcePackageFilePath, X509Certificate2 cert, string outputPackageFilePath = null,
                SignatureProductionPlace productionPlace = null,
                SignerRole signerRole = null,
                bool signPackageFiles = true,
                bool signPackageFile = true,
                bool detachedSignaturePackageFile = false,
                bool detachedSignaturePackageFiles = false,
                DateTime? signDate = null,
                bool addTimeStamp = false,
                string timeStampServerUrl = "http://time.certum.pl");
        string Sign(string sourcePackageFilePath,
            X509Certificate2 cert,
            string outputPackageFilePath = null,
            SignatureProductionPlace productionPlace = null,
            SignerRole signerRole = null,
            string[] internalFiles = null,
            bool signPackageFile = true,
            bool detachedSignaturePackageFile = true,
            bool detachedSignaturePackageFiles = true,
            DateTime? signDate = null,
            bool addTimeStamp = false,
            string timeStampServerUrl = "http://time.certum.pl");
        void SignInternalFile(string sourcePackageFilePath,
                string internalPath,
                X509Certificate2 cert,
                SignatureProductionPlace productionPlace = null,
                SignerRole signerRole = null,
                bool detachedSignaturePackageFile = false,
                string outputPackageFilePath = null,
                DateTime? signDate = null,
                bool addTimeStamp = false,
                string timeStampServerUrl = "http://time.certum.pl",
                CommitmentTypeId commitmentTypeId = CommitmentTypeId.ProofOfApproval);

        void SignInternalFile(
                string sourcePackageFilePath,
                ArchivalPackage.Model.ItemBase item,
                PackageManager mgr,
                X509Certificate2 cert,
                SignatureProductionPlace productionPlace = null,
                SignerRole signerRole = null,
                bool detachedSignaturePackageFile = false,
                string outputPackageFilePath = null,
                DateTime? signDate = null,
                bool addTimeStamp = false,
                string timeStampServerUrl = "http://time.certum.pl",
                CommitmentTypeId commitmentTypeId = CommitmentTypeId.ProofOfApproval);

        void SignPdfFile(
                string sourceFilePath,
                X509Certificate2 cert,
                CommitmentTypeId reason = CommitmentTypeId.ProofOfApproval,
                string location = null,
                DateTime? signDate = null,
                bool addTimeStamp = false,
                string timeStampServerUrl = "http://time.certum.pl",
                string tsaPolicy = null,
                string tsaLogin = null,
                string tsaPassword = null,
                X509Certificate2 tsaCert = null,
                byte[] apperancePngImage = null,
                PdfSignatureLocation apperancePngImageLocation = PdfSignatureLocation.Custom,
                float apperanceLocationX = 30F,
                float apperanceLocationY = 650F,
                float apperanceWidth = 200F,
                float apperanceHeight = 50F,
                float margin = 10F,
                string outputFilePath = null,
                bool addSignatureApperance = true,
                bool imageAsBackground = true,
                bool allowMultipleSignatures = false);

        SignatureInfo[] GetSignatureInfos(PackageManager mgr, string internalPath);
        SignatureInfo[] GetSignatureInfos(PackageManager mgr, ArchivalPackage.Model.DocumentFile item);
        SignatureInfo[] GetSignatureInfos(string packageFilePath);
        SignatureInfo[] GetSignatureInfos(string packageFilePath, string internalPath);
        SignatureInfo[] GetFileSignatureInfos(string filePath, string internalPath);
        SignatureInfo[] GetXadesSignatureInfos(string xadesFilePath);
        SignatureInfo[] GetXadesSignatureInfos(XElement xades, string fileName = null);

        SignatureVerifyInfo[] VerifySignatures(string packageFilePath);
        SignatureVerifyInfo[] VerifySignatures(string packageFilePath, string internalPath);
        SignatureVerifyInfo[] VerifyFileSignatures(string filePath, string internalPath);
        SignatureVerifyInfo[] VerifyXadesSignature(string xadesFilePath);

        SignAndVerifyInfo GetSignAndVerifyInfo(string packageFilePath, string internalPath);
        SignAndVerifyInfo GetSignAndVerifyInfo(PackageManager mgr, string internalPath);
        SignAndVerifyInfo GetSignAndVerifyInfo(PackageManager mgr, ArchivalPackage.Model.DocumentFile item);
        SignAndVerifyInfo[] GetSignAndVerifyInfo(string packageFilePath);
        SignAndVerifyInfo[] GetSignAndVerifyInfo(PackageManager mgr);

        SignAndVerifyInfo GetPdfFileSignAndVerifyInfo(string filePath);
    }
}
