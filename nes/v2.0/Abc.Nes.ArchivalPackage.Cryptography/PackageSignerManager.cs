/*=====================================================================================

	ABC NES.ArchivalPackage.Cryptography 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/


using Abc.Nes.Xades;
using Abc.Nes.Xades.Signature;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Abc.Nes.ArchivalPackage.Cryptography {
    public interface IPackageSignerManager : IDisposable {
        string Sign(string sourcePackageFilePath, X509Certificate2 cert, string outputPackageFilePath = null,
                Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
                Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
                bool signPackageFiles = true,
                bool signPackageFile = true,
                bool detachedSignaturePackageFile = false,
                bool detachedSignaturePackageFiles = false);
        void SignInternalFile(string sourcePackageFilePath, 
                string internalPath,
                X509Certificate2 cert,
                Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
                Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
                bool detachedSignaturePackageFile = false,
                string outputPackageFilePath = null);

        void SignInternalFile(
                string sourcePackageFilePath,
                Model.ItemBase item,
                PackageManager mgr,
                X509Certificate2 cert,
                Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
                Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
                bool detachedSignaturePackageFile = false,
                string outputPackageFilePath = null);

        //TODO: Add validate method
    }
    public partial class PackageSignerManager : IPackageSignerManager {

        public string Sign(string sourcePackageFilePath, X509Certificate2 cert, string outputPackageFilePath = null,
            Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
            Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
            bool signPackageFiles = true,
            bool signPackageFile = true,
            bool detachedSignaturePackageFile = false,
            bool detachedSignaturePackageFiles = false) {

            if (cert == null) { throw new ArgumentNullException("cert"); }
            if (sourcePackageFilePath == null) { throw new ArgumentNullException("filePath"); }

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

            mgr.Save(outputPackageFilePath);
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
                        result.Save(outputPackageFilePath);
                        return outputPackageFilePath;
                    }
                }
            }

            return default;
        }

        public void SignInternalFile(string sourcePackageFilePath, string internalPath,
            X509Certificate2 cert,
            Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
            Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
            bool detachedSignaturePackageFile = false,
            string outputPackageFilePath = null) {
            if (cert == null) { throw new ArgumentNullException("cert"); }
            if (sourcePackageFilePath == null) { throw new ArgumentNullException("filePath"); }

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
            Model.ItemBase item,
            PackageManager mgr,
            X509Certificate2 cert,
            Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
            Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
            bool detachedSignaturePackageFile = false,
            string outputPackageFilePath = null) {
            if (cert == null) { throw new ArgumentNullException("cert"); }
            if (sourcePackageFilePath == null) { throw new ArgumentNullException("filePath"); }

            if (!File.Exists(sourcePackageFilePath)) { throw new FileNotFoundException("Package file not found!", sourcePackageFilePath); }

            if (String.IsNullOrEmpty(outputPackageFilePath)) { outputPackageFilePath = new FileInfo(sourcePackageFilePath).FullName; }

            var xadesManager = new XadesManager();
            if (mgr.Package.Documents == null || mgr.Package.Documents.IsEmpty) {
                mgr.LoadPackage(sourcePackageFilePath);
            }

            if (item.FileName.ToLower().EndsWith(".xml")) {
                SignXmlItem(item, xadesManager, cert, productionPlace, signerRole);
            }
            else if (item.FileName.ToLower().EndsWith(".pdf")) {
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

            mgr.Save(outputPackageFilePath);
        }

        public void Dispose() { }
    }
}
