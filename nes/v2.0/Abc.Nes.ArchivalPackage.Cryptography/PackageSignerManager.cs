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
using Abc.Nes.Xades;
using Abc.Nes.Xades.Signature;
using System;
using System.IO;
using System.Linq;
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
                bool detachedSignaturePackageFiles = false);
        string Sign(string sourcePackageFilePath,
            X509Certificate2 cert,
            string outputPackageFilePath = null,
            Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
            Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
            string[] internalFiles = null,
            bool signPackageFile = true,
            bool detachedSignaturePackageFile = true,
            bool detachedSignaturePackageFiles = true);
        void SignInternalFile(string sourcePackageFilePath,
                string internalPath,
                X509Certificate2 cert,
                Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
                Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
                bool detachedSignaturePackageFile = false,
                string outputPackageFilePath = null);

        void SignInternalFile(
                string sourcePackageFilePath,
                ArchivalPackage.Model.ItemBase item,
                PackageManager mgr,
                X509Certificate2 cert,
                Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
                Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
                bool detachedSignaturePackageFile = false,
                string outputPackageFilePath = null);

        SignatureInfo GetSignatureInfo(string packageFilePath, string internalPath);
        SignatureInfo GetSignatureInfo(string xadesFilePath);
    }
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
            bool detachedSignaturePackageFiles = false) {

            if (cert == null) { throw new ArgumentNullException("cert"); }
            if (sourcePackageFilePath == null) { throw new ArgumentNullException("sourcePackageFilePath"); }
            if (!File.Exists(sourcePackageFilePath)) { throw new FileNotFoundException("Package file not found!", sourcePackageFilePath); }
            if (String.IsNullOrEmpty(outputPackageFilePath)) { outputPackageFilePath = new FileInfo(sourcePackageFilePath).FullName; }

            var xadesManager = new XadesManager();
            var mgr = new PackageManager();
            mgr.LoadPackage(sourcePackageFilePath);

            var items = mgr.GetAllFiles();
            if (items != null && signPackageFiles) {
                foreach (var item in items) {
                    if (item.FileName.ToLower().EndsWith(".xml")) {
                        SignXmlItem(item, xadesManager, cert, productionPlace, signerRole);
                    }
                    else if (item.FileName.ToLower().EndsWith(".pdf")) {
                        SignPdfItem(item, cert);
                    }
                    else {
                        if (detachedSignaturePackageFiles) {
                            // place signature in detached .xades file
                            SignDetachedOtherItem(mgr, item, xadesManager, cert, productionPlace, signerRole);
                        }
                        else {
                            SignOtherItem(item, xadesManager, cert, productionPlace, signerRole);
                        }
                    }
                }
            }

            mgr.Save(outputPackageFilePath, true);
            if (signPackageFile) {
                SignatureDocument result;

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
                        result = xadesManager.CreateEnvelopingSignature(stream, cert, productionPlace, signerRole);
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
            bool detachedSignaturePackageFiles = true) {

            if (cert == null) { throw new ArgumentNullException("cert"); }
            if (sourcePackageFilePath == null) { throw new ArgumentNullException("sourcePackageFilePath"); }
            if (!File.Exists(sourcePackageFilePath)) { throw new FileNotFoundException("Package file not found!", sourcePackageFilePath); }
            if (String.IsNullOrEmpty(outputPackageFilePath)) { outputPackageFilePath = new FileInfo(sourcePackageFilePath).FullName; }

            var xadesManager = new XadesManager();
            var mgr = new PackageManager();
            mgr.LoadPackage(sourcePackageFilePath);

            var items = mgr.GetAllFiles();
            if (items != null && internalFiles != null && internalFiles.Length > 0) {
                foreach (var item in items) {
                    if (!internalFiles.Contains(item.FilePath)) { continue; }
                    if (item.FileName.ToLower().EndsWith(".xml")) {
                        SignXmlItem(item, xadesManager, cert, productionPlace, signerRole);
                    }
                    else if (item.FileName.ToLower().EndsWith(".pdf")) {
                        SignPdfItem(item, cert);
                    }
                    else {
                        if (detachedSignaturePackageFiles) {
                            // place signature in detached .xades file
                            SignDetachedOtherItem(mgr, item, xadesManager, cert, productionPlace, signerRole);
                        }
                        else {
                            SignOtherItem(item, xadesManager, cert, productionPlace, signerRole);
                        }
                    }
                }
            }

            mgr.Save(outputPackageFilePath, true);
            if (signPackageFile) {
                SignatureDocument result;

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
                        result = xadesManager.CreateEnvelopingSignature(stream, cert, productionPlace, signerRole);
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
            Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
            Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
            bool detachedSignaturePackageFile = false,
            string outputPackageFilePath = null) {

            if (cert == null) { throw new ArgumentNullException("cert"); }
            if (sourcePackageFilePath == null) { throw new ArgumentNullException("sourcePackageFilePath"); }
            if (!File.Exists(sourcePackageFilePath)) { throw new FileNotFoundException("Package file not found!", sourcePackageFilePath); }
            if (String.IsNullOrEmpty(outputPackageFilePath)) { outputPackageFilePath = new FileInfo(sourcePackageFilePath).FullName; }

            var mgr = new PackageManager();
            mgr.LoadPackage(sourcePackageFilePath);
            var item = mgr.GetItemByFilePath(internalPath);
            if (item != null) {
                SignInternalFile(sourcePackageFilePath, item, mgr, cert, productionPlace, signerRole, detachedSignaturePackageFile, outputPackageFilePath);
            }
        }

        public void SignInternalFile(
            string sourcePackageFilePath,
            ArchivalPackage.Model.ItemBase item,
            PackageManager mgr,
            X509Certificate2 cert,
            Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
            Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
            bool detachedSignaturePackageFile = false,
            string outputPackageFilePath = null) {

            if (cert == null) { throw new ArgumentNullException("cert"); }
            if (sourcePackageFilePath == null) { throw new ArgumentNullException("sourcePackageFilePath"); }
            if (!File.Exists(sourcePackageFilePath)) { throw new FileNotFoundException("Package file not found!", sourcePackageFilePath); }
            if (String.IsNullOrEmpty(outputPackageFilePath)) { outputPackageFilePath = new FileInfo(sourcePackageFilePath).FullName; }

            var xadesManager = new XadesManager();
            if (mgr.Package.Documents == null || mgr.Package.Documents.IsEmpty) {
                mgr.LoadPackage(sourcePackageFilePath);
            }

            if (item.FileName.ToLower().EndsWith(".xml")) {
                // place signature inside root element
                SignXmlItem(item, xadesManager, cert, productionPlace, signerRole);
            }
            else if (item.FileName.ToLower().EndsWith(".pdf")) {
                // pades
                SignPdfItem(item, cert);
            }
            else {
                if (detachedSignaturePackageFile) {
                    // place signature in detached .xades file
                    SignDetachedOtherItem(mgr, item, xadesManager, cert, productionPlace, signerRole);
                }
                else {
                    SignOtherItem(item, xadesManager, cert, productionPlace, signerRole);
                }
            }

            mgr.Save(outputPackageFilePath, true);
        }

        public void Dispose() { }

        public SignatureInfo GetSignatureInfo(string packageFilePath, string internalPath) {
            if (String.IsNullOrEmpty(packageFilePath)) { throw new ArgumentNullException("packageFilePath"); }
            if (String.IsNullOrEmpty(internalPath)) { throw new ArgumentNullException("internalPath"); }
            if (!File.Exists(packageFilePath)) { throw new FileNotFoundException("Package file not found!", packageFilePath); }

            var mgr = new PackageManager();
            mgr.LoadPackage(packageFilePath);
            var item = mgr.GetItemByFilePath(internalPath) as ArchivalPackage.Model.DocumentFile;
            if (item != null) {

                if (internalPath.EndsWith(".xades") || internalPath.EndsWith(".xml")) {
                    return GetSignatureInfo(item.FileData.ToXElement());
                }
                else if (internalPath.EndsWith(".pdf")) {
                    return GetPadesInfo(item.FileData);
                }
                else {
                    var xadesInternalPath = $"{internalPath}.xades";
                    var xadesItem = mgr.GetItemByFilePath(xadesInternalPath) as ArchivalPackage.Model.DocumentFile;
                    if (xadesItem != null) {
                        return GetSignatureInfo(xadesItem.FileData.ToXElement());
                    }
                }
            }

            return default;
        }

        public SignatureInfo GetSignatureInfo(string xadesFilePath) {
            if (String.IsNullOrEmpty(xadesFilePath)) { throw new ArgumentNullException("xadesFilePath"); }
            if (!File.Exists(xadesFilePath)) { throw new FileNotFoundException("File not found!", xadesFilePath); }

            var xml = XElement.Load(xadesFilePath);
            if (xml != null) {
                return GetSignatureInfo(xml);
            }

            return default;
        }
    }
}
