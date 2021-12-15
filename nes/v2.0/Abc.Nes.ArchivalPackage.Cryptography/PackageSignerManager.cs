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
using Abc.Nes.Common.Models;
using Abc.Nes.Xades;
using Abc.Nes.Xades.Signature;
using Abc.Nes.Xades.Signature.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace Abc.Nes.ArchivalPackage.Cryptography {
    public partial class PackageSignerManager : IPackageSignerManager {
        public string Sign(
            string sourcePackageFilePath,
            X509Certificate2 cert,
            string outputPackageFilePath = null,
            Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
            Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
            bool signPackageFiles = true,
            bool signPackageFile = true,
            bool detachedSignaturePackageFile = false,
            bool detachedSignaturePackageFiles = false,
            bool addTimeStamp = false,
            string timeStampServerUrl = "http://time.certum.pl"
            ) {

            if (cert == null) { throw new ArgumentNullException("cert"); }
            if (sourcePackageFilePath == null) { throw new ArgumentNullException("sourcePackageFilePath"); }
            if (!File.Exists(sourcePackageFilePath)) { throw new FileNotFoundException("Package file not found!", sourcePackageFilePath); }
            if (String.IsNullOrEmpty(outputPackageFilePath)) { outputPackageFilePath = new FileInfo(sourcePackageFilePath).FullName; }

            var xadesManager = new XadesManager();
            var mgr = new PackageManager();
            mgr.LoadPackage(sourcePackageFilePath, out var exception);

            var items = mgr.GetAllFiles();
            if (items != null && signPackageFiles) {
                foreach (var item in items) {
                    if (item.FileName.ToLower().EndsWith(".xml")) {
                        SignXmlItem(item, xadesManager, cert, productionPlace, signerRole, addTimeStamp ? XadesFormat.XadesT : XadesFormat.XadesBes, timeStampServerUrl, CommitmentTypeId.ProofOfOrigin);
                    }
                    else if (item.FileName.ToLower().EndsWith(".pdf")) {
                        string location = productionPlace.IsNotNull() ? productionPlace.City : null;
                        SignPdfItem(item, cert, addTimeStamp, CommitmentTypeId.ProofOfOrigin, location, timeStampServerUrl);
                    }
                    else {
                        if (detachedSignaturePackageFiles) {
                            // place signature in detached .xades file
                            SignDetachedOtherItem(mgr, item, xadesManager, cert, productionPlace, signerRole, addTimeStamp, timeStampServerUrl, CommitmentTypeId.ProofOfOrigin);
                        }
                        else {
                            SignOtherItem(item, xadesManager, cert, productionPlace, signerRole, addTimeStamp, timeStampServerUrl, CommitmentTypeId.ProofOfOrigin);
                        }
                    }
                }
            }

            mgr.Save(outputPackageFilePath, true);
            if (signPackageFile) {
                SignatureDocument result;
                Xades.Upgraders.SignatureFormat? sigFormat = addTimeStamp ? Xades.Upgraders.SignatureFormat.XAdES_T : (Xades.Upgraders.SignatureFormat?)null;

                if (detachedSignaturePackageFile) {
                    // place signature in detached .xades file
                    result = xadesManager.CreateDetachedSignature(outputPackageFilePath, cert, productionPlace, signerRole);
                    if (result != null) {
                        var resultFilePath = $"{outputPackageFilePath}.xades";
                        result.Save(resultFilePath);
                        return resultFilePath;
                    }
                }
                else {
                    using (var stream = new FileStream(outputPackageFilePath, FileMode.Open)) {
                        result = xadesManager.CreateEnvelopingSignature(stream, cert, productionPlace, signerRole,
                            null, sigFormat, timeStampServerUrl);
                    }
                    if (result != null) {
                        if (!outputPackageFilePath.ToLower().EndsWith(".xades")) {
                            outputPackageFilePath = $"{outputPackageFilePath}.xades";
                        }
                        result.Save(outputPackageFilePath);
                        return outputPackageFilePath;
                    }
                }
            }

            return default;
        }

        public string Sign(string sourcePackageFilePath,
            X509Certificate2 cert,
            string outputPackageFilePath = null,
            Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
            Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
            string[] internalFiles = null,
            bool signPackageFile = true,
            bool detachedSignaturePackageFile = true,
            bool detachedSignaturePackageFiles = true,
            bool addTimeStamp = false,
            string timeStampServerUrl = "http://time.certum.pl") {

            if (cert == null) { throw new ArgumentNullException("cert"); }
            if (sourcePackageFilePath == null) { throw new ArgumentNullException("sourcePackageFilePath"); }
            if (!File.Exists(sourcePackageFilePath)) { throw new FileNotFoundException("Package file not found!", sourcePackageFilePath); }
            if (String.IsNullOrEmpty(outputPackageFilePath)) { outputPackageFilePath = new FileInfo(sourcePackageFilePath).FullName; }

            var xadesManager = new XadesManager();
            var mgr = new PackageManager();
            mgr.LoadPackage(sourcePackageFilePath, out var exception);

            var items = mgr.GetAllFiles();
            if (items != null && internalFiles != null && internalFiles.Length > 0) {
                foreach (var item in items) {
                    if (!internalFiles.Contains(item.FilePath)) { continue; }
                    if (item.FileName.ToLower().EndsWith(".xml")) {
                        SignXmlItem(item, xadesManager, cert, productionPlace, signerRole, addTimeStamp ? XadesFormat.XadesT : XadesFormat.XadesBes, timeStampServerUrl, CommitmentTypeId.ProofOfOrigin);
                    }
                    else if (item.FileName.ToLower().EndsWith(".pdf")) {
                        SignPdfItem(item, cert, addTimeStamp, CommitmentTypeId.ProofOfOrigin, timeStampServerUrl: timeStampServerUrl);
                    }
                    else {
                        if (detachedSignaturePackageFiles) {
                            // place signature in detached .xades file
                            SignDetachedOtherItem(mgr, item, xadesManager, cert, productionPlace, signerRole, addTimeStamp, timeStampServerUrl, CommitmentTypeId.ProofOfOrigin);
                        }
                        else {
                            SignOtherItem(item, xadesManager, cert, productionPlace, signerRole, addTimeStamp, timeStampServerUrl, CommitmentTypeId.ProofOfOrigin);
                        }
                    }
                }
            }

            mgr.Save(outputPackageFilePath, true);
            if (signPackageFile) {
                SignatureDocument result;
                Xades.Upgraders.SignatureFormat? sigFormat = addTimeStamp ? Xades.Upgraders.SignatureFormat.XAdES_T : (Xades.Upgraders.SignatureFormat?)null;

                if (detachedSignaturePackageFile) {
                    // place signature in detached .xades file
                    result = xadesManager.CreateDetachedSignature(outputPackageFilePath, cert, productionPlace, signerRole,
                          sigFormat, timeStampServerUrl);

                    if (result != null) {
                        var resultFilePath = $"{outputPackageFilePath}.xades";
                        result.Save(resultFilePath);
                        return resultFilePath;
                    }
                }
                else {
                    using (var stream = new FileStream(outputPackageFilePath, FileMode.Open)) {
                        result = xadesManager.CreateEnvelopingSignature(stream, cert, productionPlace, signerRole,
                            null, sigFormat, timeStampServerUrl);
                    }
                    if (result != null) {
                        if (!outputPackageFilePath.ToLower().EndsWith(".xades")) {
                            outputPackageFilePath = $"{outputPackageFilePath}.xades";
                        }
                        result.Save(outputPackageFilePath);
                        return outputPackageFilePath;
                    }
                }
            }

            return default;
        }

        public void SignInternalFile(
            string sourcePackageFilePath,
            string internalPath,
            X509Certificate2 cert,
            SignatureProductionPlace productionPlace = null,
            SignerRole signerRole = null,
            bool detachedSignaturePackageFile = false,
            string outputPackageFilePath = null,
            bool addTimeStamp = false,
            string timeStampServerUrl = "http://time.certum.pl",
            CommitmentTypeId commitmentTypeId = CommitmentTypeId.ProofOfApproval) {

            if (cert == null) { throw new ArgumentNullException("cert"); }
            if (sourcePackageFilePath == null) { throw new ArgumentNullException("sourcePackageFilePath"); }
            if (!File.Exists(sourcePackageFilePath)) { throw new FileNotFoundException("Package file not found!", sourcePackageFilePath); }
            if (String.IsNullOrEmpty(outputPackageFilePath)) { outputPackageFilePath = new FileInfo(sourcePackageFilePath).FullName; }

            var mgr = new PackageManager();
            mgr.LoadPackage(sourcePackageFilePath, out var exception);
            var item = mgr.GetItemByFilePath(internalPath);
            if (item != null) {
                SignInternalFile(sourcePackageFilePath, item, mgr, cert, productionPlace, signerRole, detachedSignaturePackageFile,
                    outputPackageFilePath, addTimeStamp, timeStampServerUrl, commitmentTypeId);
            }
        }

        public void SignInternalFile(
            string sourcePackageFilePath,
            ArchivalPackage.Model.ItemBase item,
            PackageManager mgr,
            X509Certificate2 cert,
            SignatureProductionPlace productionPlace = null,
            SignerRole signerRole = null,
            bool detachedSignaturePackageFile = false,
            string outputPackageFilePath = null,
            bool addTimeStamp = false,
            string timeStampServerUrl = "http://time.certum.pl",
            CommitmentTypeId commitmentTypeId = CommitmentTypeId.ProofOfApproval) {

            if (cert == null) { throw new ArgumentNullException("cert"); }
            if (sourcePackageFilePath == null) { throw new ArgumentNullException("sourcePackageFilePath"); }
            if (!File.Exists(sourcePackageFilePath)) { throw new FileNotFoundException("Package file not found!", sourcePackageFilePath); }
            if (String.IsNullOrEmpty(outputPackageFilePath)) { outputPackageFilePath = new FileInfo(sourcePackageFilePath).FullName; }

            var xadesManager = new XadesManager();
            if (mgr.Package.Documents == null || mgr.Package.Documents.IsEmpty) {
                mgr.LoadPackage(sourcePackageFilePath, out var exception);
            }

            if (item.FileName.ToLower().EndsWith(".xml")) {
                // place signature inside root element
                SignXmlItem(item, xadesManager, cert, productionPlace, signerRole, addTimeStamp ? XadesFormat.XadesT : XadesFormat.XadesBes, timeStampServerUrl, commitmentTypeId);
            }
            else if (item.FileName.ToLower().EndsWith(".pdf")) {
                // pades
                SignPdfItem(item, cert, addTimeStamp, commitmentTypeId, timeStampServerUrl: timeStampServerUrl);
            }
            else {
                if (detachedSignaturePackageFile) {
                    // place signature in detached .xades file
                    SignDetachedOtherItem(mgr, item, xadesManager, cert, productionPlace, signerRole, addTimeStamp, timeStampServerUrl, commitmentTypeId);
                }
                else {
                    SignOtherItem(item, xadesManager, cert, productionPlace, signerRole, addTimeStamp, timeStampServerUrl, commitmentTypeId);
                }
            }

            mgr.Save(outputPackageFilePath, true);
        }

        public void SignPdfFile(string sourceFilePath, PdfSignatureOptions options, string outputFilePath = null) {

            if (options.Certificate == null) { throw new ArgumentNullException("cert"); }
            if (sourceFilePath == null) { throw new ArgumentNullException("filePath"); }
            if (!File.Exists(sourceFilePath)) { throw new FileNotFoundException("File not found!", sourceFilePath); }
            if (outputFilePath == null) { outputFilePath = sourceFilePath; }
            if (options.AddTimestamp && options.TimestampOptions == null) throw new ArgumentNullException("TimestampOptions");

            if (!options.SignDate.HasValue) options.SignDate = DateTime.Now;


            signPdfFile(sourceFilePath, options, outputFilePath);
        }

        public void SignPdfFile(
                string sourceFilePath,
                X509Certificate2 cert,
                CommitmentTypeId reason = CommitmentTypeId.ProofOfApproval,
                string location = null,
                DateTime? signDate = null,
                bool addTimeStamp = false,
                string timeStampServerUrl = "http://time.certum.pl",
                string tsaPolicy=null, 
                string tsaLogin=null,
                string tsaPassword=null,
                X509Certificate2 tsaCert=null,
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
                bool allowMultipleSignatures = false) {
            if (cert == null) { throw new ArgumentNullException("cert"); }
            if (sourceFilePath == null) { throw new ArgumentNullException("filePath"); }
            if (!File.Exists(sourceFilePath)) { throw new FileNotFoundException("File not found!", sourceFilePath); }
            if (outputFilePath == null) { outputFilePath = sourceFilePath; }

            if (!signDate.HasValue) signDate = DateTime.Now;

            signPdfFile(sourceFilePath, cert, reason, location, signDate.Value,
                addTimeStamp, timeStampServerUrl, tsaPolicy, tsaCert, tsaLogin, tsaPassword, 
                apperancePngImage, apperancePngImageLocation, apperanceLocationX, apperanceLocationY,
                apperanceWidth, apperanceHeight, margin,
                outputFilePath, 
                addSignatureApperance, imageAsBackground,
                allowMultipleSignatures);
        }

        public void Dispose() { }

        public SignatureInfo[] GetSignatureInfos(string packageFilePath) {
            var list = new List<SignatureInfo>();
            using (var mgr = new PackageManager()) {
                mgr.LoadPackage(packageFilePath, out var exception);
                foreach (var file in mgr.Package.GetAllFiles()) {
                    if (file is ArchivalPackage.Model.DocumentFile) {
                        var item = file as ArchivalPackage.Model.DocumentFile;
                        var result = GetSignatureInfos(mgr, item);
                        if (result != null && result.Length > 0) { list.AddRange(result); }
                    }
                }
            }
            return list.ToArray();
        }
        public SignatureInfo[] GetSignatureInfos(PackageManager mgr, string internalPath) {
            if (mgr == null) { throw new ArgumentNullException("mgr"); }
            if (String.IsNullOrEmpty(internalPath)) { throw new ArgumentNullException("internalPath"); }

            var list = new List<SignatureInfo>();
            var item = mgr.GetItemByFilePath(internalPath) as ArchivalPackage.Model.DocumentFile;
            if (item != null) {
                var result = GetSignatureInfos(mgr, item);
                if (result != null && result.Length > 0) { list.AddRange(result); }
            }

            return list.ToArray();
        }
        public SignatureInfo[] GetSignatureInfos(PackageManager mgr, ArchivalPackage.Model.DocumentFile item) {
            if (mgr == null) { throw new ArgumentNullException("mgr"); }
            if (item == null) { throw new ArgumentNullException("item"); }

            var list = new List<SignatureInfo>();
            var internalPath = item.FilePath;
            if (internalPath.EndsWith(".xades") || internalPath.EndsWith(".xml")) {
                var result = GetXadesSignatureInfos(item.FileData.ToXElement(), internalPath);
                if (result != null && result.Length > 0) { list.AddRange(result); }
            }
            else if (internalPath.EndsWith(".pdf")) {
                var result = GetPadesInfos(item.FileData, internalPath);
                if (result != null && result.Length > 0) { list.AddRange(result); }
            }
            else if (internalPath.EndsWith(".zip")) {
                var result = GetZipxInfos(item.FileData, internalPath);
                if (result != null && result.Length > 0) { list.AddRange(result); }
            }
            else {
                var xadesInternalPath = $"{internalPath}.xades";
                var xadesItem = mgr.GetItemByFilePath(xadesInternalPath) as ArchivalPackage.Model.DocumentFile;
                if (xadesItem != null) {
                    var result = GetXadesSignatureInfos(xadesItem.FileData.ToXElement(), xadesInternalPath);
                    if (result != null && result.Length > 0) { list.AddRange(result); }
                }
            }

            return list.ToArray();
        }
        public SignatureInfo[] GetSignatureInfos(string packageFilePath, string internalPath) {
            if (String.IsNullOrEmpty(packageFilePath)) { throw new ArgumentNullException("packageFilePath"); }
            if (String.IsNullOrEmpty(internalPath)) { throw new ArgumentNullException("internalPath"); }
            if (!File.Exists(packageFilePath)) { throw new FileNotFoundException("Package file not found!", packageFilePath); }

            var mgr = new PackageManager();
            mgr.LoadPackage(packageFilePath, out var exception);
            return GetSignatureInfos(mgr, internalPath);
        }

        public SignatureInfo[] GetXadesSignatureInfos(string xadesFilePath) {
            if (String.IsNullOrEmpty(xadesFilePath)) { throw new ArgumentNullException("xadesFilePath"); }
            if (!File.Exists(xadesFilePath)) { throw new FileNotFoundException("File not found!", xadesFilePath); }

            var xml = XElement.Load(xadesFilePath);
            if (xml != null) {
                return GetXadesSignatureInfos(xml, new FileInfo(xadesFilePath).Name);
            }

            return default;
        }
        public SignatureInfo[] GetXadesSignatureInfos(XElement xades, string fileName = null) {
            if (xades != null) {
                var signatures = xades.DescendantsAndSelf().Where(x => x.Name.LocalName == "Signature").ToArray();
                if (signatures.Length > 0) {
                    var list = new List<SignatureInfo>();
                    foreach (var signature in signatures) {
                        var result = GetSignatureInfo(signature);
                        if (result != null) { list.Add(result); }
                    }
                    if (fileName != null) {
                        foreach (var item in list) { item.FileName = fileName; }
                    }

                    return list.ToArray();
                }
            }
            return default;
        }

        public SignatureVerifyInfo[] VerifyXadesSignature(string xadesFilePath) {
            var list = new List<SignatureVerifyInfo>();
            var result = VerifyXadesSignatures(xadesFilePath, Path.GetFileName(xadesFilePath));
            if (result != null && result.Length > 0) { list.AddRange(result); }
            return list.ToArray();
        }
        public SignatureVerifyInfo[] VerifySignatures(string packageFilePath, string internalPath) {
            var list = new List<SignatureVerifyInfo>();
            using (var mgr = new PackageManager()) {
                mgr.LoadPackage(packageFilePath, out var exception);
                var file = mgr.GetItemByFilePath(internalPath);
                if (file != null) {
                    var item = file as ArchivalPackage.Model.DocumentFile;
                    if (item != null) {
                        VerifySignatures(list, mgr, item, internalPath);
                    }
                }
            }

            return list.ToArray();
        }
        public SignatureVerifyInfo[] VerifySignatures(string packageFilePath) {
            var list = new List<SignatureVerifyInfo>();

            using (var mgr = new PackageManager()) {
                mgr.LoadPackage(packageFilePath, out var exception);
                foreach (var file in mgr.Package.GetAllFiles()) {
                    if (file is ArchivalPackage.Model.DocumentFile) {
                        var item = file as ArchivalPackage.Model.DocumentFile;
                        var internalPath = item.FilePath;
                        VerifySignatures(list, mgr, item, internalPath);
                    }
                }
            }


            return list.ToArray();
        }

        public SignAndVerifyInfo GetSignAndVerifyInfo(string packageFilePath, string internalPath) {
            if (String.IsNullOrEmpty(packageFilePath)) { throw new ArgumentNullException("packageFilePath"); }
            if (String.IsNullOrEmpty(internalPath)) { throw new ArgumentNullException("internalPath"); }
            if (!File.Exists(packageFilePath)) { throw new FileNotFoundException("Package file not found!", packageFilePath); }

            var mgr = new PackageManager();
            mgr.LoadPackage(packageFilePath, out var exception);

            return GetSignAndVerifyInfo(mgr, internalPath);
        }
        public SignAndVerifyInfo GetSignAndVerifyInfo(PackageManager mgr, string internalPath) {
            if (mgr == null) { throw new ArgumentNullException("mgr"); }
            if (String.IsNullOrEmpty(internalPath)) { throw new ArgumentNullException("internalPath"); }

            var item = mgr.GetItemByFilePath(internalPath) as ArchivalPackage.Model.DocumentFile;
            if (item != null) {
                return GetSignAndVerifyInfo(mgr, item);
            }
            return default;
        }
        public SignAndVerifyInfo GetSignAndVerifyInfo(PackageManager mgr, ArchivalPackage.Model.DocumentFile item) {
            if (mgr == null) { throw new ArgumentNullException("mgr"); }
            if (item == null) { throw new ArgumentNullException("item"); }

            var internalPath = item.FilePath;
            var temp = Path.Combine(Path.GetTempPath(), $"ABCPRO.NES\\{Path.GetFileNameWithoutExtension(internalPath)}");

            try {
                if (!Directory.Exists(temp)) { Directory.CreateDirectory(temp); }
                SignAndVerifyInfo result = null;

                if (internalPath.EndsWith(".xades") || internalPath.EndsWith(".xml")) {
                    result = GetXmlSignAndVerifyInfo(mgr, item, temp, internalPath);
                }
                else if (internalPath.EndsWith(".pdf")) {
                    result = GetPadesSignAndVerifyInfo(item.FileData, internalPath);
                }
                else if (internalPath.EndsWith(".zip")) {
                    result = GetZipSignAndVerifyInfo(item.FileData, temp, internalPath);
                }
                else { }
                return result;
            }
            catch { }
            finally {
                try { if (Directory.Exists(temp)) { Directory.Delete(temp, true); } } catch { }
            }

            return default;
        }

        public SignAndVerifyInfo[] GetSignAndVerifyInfo(string packageFilePath) {
            if (String.IsNullOrEmpty(packageFilePath)) { throw new ArgumentNullException("packageFilePath"); }
            if (!File.Exists(packageFilePath)) { throw new FileNotFoundException("Package file not found!", packageFilePath); }
            var list = new List<SignAndVerifyInfo>();
            using (var mgr = new PackageManager()) {
                mgr.LoadPackage(packageFilePath, out var exception);
                var files = mgr.Package.GetAllFiles();
                foreach (var item in files) {
                    if (item is ArchivalPackage.Model.DocumentFile) {
                        var result = GetSignAndVerifyInfo(mgr, item as ArchivalPackage.Model.DocumentFile);
                        if (result != null && result.SignInfo != null && result.VerifyInfo != null) {
                            list.Add(result);
                        }
                    }
                }
            }
            return list.ToArray();
        }

        public SignAndVerifyInfo[] GetSignAndVerifyInfo(PackageManager mgr) {
            if (mgr == null) { throw new ArgumentNullException("mgr"); }
            var files = mgr.Package.GetAllFiles();
            var list = new List<SignAndVerifyInfo>();
            foreach (var item in files) {
                if (item is ArchivalPackage.Model.DocumentFile) {
                    var result = GetSignAndVerifyInfo(mgr, item as ArchivalPackage.Model.DocumentFile);
                    if (result != null) {
                        list.Add(result);
                    }
                }
            }
            return list.ToArray();
        }
    }
}
