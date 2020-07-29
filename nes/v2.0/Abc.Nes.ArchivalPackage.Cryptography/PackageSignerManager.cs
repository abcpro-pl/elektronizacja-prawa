/*=====================================================================================

	ABC NES.ArchivalPackage.Cryptography 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/


using Abc.Nes.ArchivalPackage.Model;
using Abc.Nes.Xades;
using Abc.Nes.Xades.Signature;
using Abc.Nes.Xades.Signature.Parameters;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Abc.Nes.ArchivalPackage.Cryptography {
    public class PackageSignerManager : IDisposable {
        public void Sign(string filePath, X509Certificate2 cert, string outputFilePath = null,
            Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
            Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
            bool signPackageFiles = true,
            bool signPackageFile = true,
            bool detachedPackageFile = false) {

            if (cert == null) { throw new ArgumentNullException("cert"); }
            if (filePath == null) { throw new ArgumentNullException("filePath"); }

            if (!File.Exists(filePath)) { throw new FileNotFoundException("Package file not found!", filePath); }

            if (String.IsNullOrEmpty(outputFilePath)) { outputFilePath = new FileInfo(filePath).FullName; }

            var xadesManager = new XadesManager();
            var mgr = new PackageManager();
            mgr.LoadPackage(filePath);

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
                        SignOtherItem(item, xadesManager, cert, productionPlace, signerRole);
                        // TODO: sign other file types in .xades file in the future
                    }
                }
            }

            mgr.Save(outputFilePath);
            if (signPackageFile) {
                SignatureDocument result;

                if (detachedPackageFile) {
                    // place signature in detached .xades file
                    result = xadesManager.CreateDetachedSignature(outputFilePath, cert, productionPlace, signerRole);
                    if (result != null) {
                        result.Save($"{outputFilePath}.xades");
                    }
                }
                else {
                    using (var stream = new FileStream(outputFilePath, FileMode.Open)) {
                        result = xadesManager.CreateEnvelopingSignature(stream, cert, productionPlace, signerRole);
                    }
                    if (result != null) {
                        result.Save(outputFilePath);
                    }
                }                
            }
        }

        private void SignPdfItem(ItemBase item, X509Certificate2 cert) {
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
            try {
                File.WriteAllBytes(tempFilePath, (item as DocumentFile).FileData);
                using (var stream = new FileStream(tempFilePath, FileMode.Open)) {
                    using (var doc = new Aspose.Pdf.Document(stream)) {
                        var field1 = new Aspose.Pdf.Forms.SignatureField(doc.Pages[1], new Aspose.Pdf.Rectangle(10, 10, 300, 50));
                        var externalSignature = new Aspose.Pdf.Forms.ExternalSignature(cert);
                        field1.PartialName = "sig1";
                        doc.Form.Add(field1, 1);
                        field1.Sign(externalSignature);
                    }
                }
                item.Init(File.ReadAllBytes(tempFilePath));
            }
            finally {
                try {
                    if (File.Exists(tempFilePath)) { File.Delete(tempFilePath); }
                }
                catch { }
            }

        }

        private void SignXmlItem(ItemBase item,
                             XadesManager xadesManager,
                             X509Certificate2 cert,
                             SignatureProductionPlace productionPlace = null,
                             SignerRole signerRole = null) {
            var ms = new MemoryStream((item as DocumentFile).FileData);
            var result = xadesManager.AppendSignatureToXmlFile(ms, cert, productionPlace, signerRole);
            if (result != null) {
                using (var msOutput = new MemoryStream()) {
                    result.Save(msOutput);
                    item.Init(msOutput.ToArray());
                }
            }
        }

        private void SignOtherItem(ItemBase item,
                     XadesManager xadesManager,
                     X509Certificate2 cert,
                     SignatureProductionPlace productionPlace = null,
                     SignerRole signerRole = null) {
            var ms = new MemoryStream((item as DocumentFile).FileData);
            var result = xadesManager.CreateEnvelopingSignature(ms, cert, productionPlace, signerRole);
            if (result != null) {
                using (var msOutput = new MemoryStream()) {
                    result.Save(msOutput);
                    item.Init(msOutput.ToArray());
                }
            }
        }

        public void Dispose() { }
    }
}
