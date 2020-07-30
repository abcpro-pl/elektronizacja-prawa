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
using System.Xml.Serialization;

namespace Abc.Nes.ArchivalPackage.Cryptography {
    public interface IPackageSignerManager : IDisposable {
        string Sign(string sourcePackageFilePath, X509Certificate2 cert, string outputPackageFilePath = null,
                Abc.Nes.Xades.Signature.Parameters.SignatureProductionPlace productionPlace = null,
                Abc.Nes.Xades.Signature.Parameters.SignerRole signerRole = null,
                bool signPackageFiles = true,
                bool signPackageFile = true,
                bool detachedSignaturePackageFile = false,
                bool detachedSignaturePackageFiles = false);
    }
    public class PackageSignerManager : IPackageSignerManager {
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

        private void SignDetachedOtherItem(PackageManager mgr, ItemBase item,
                    XadesManager xadesManager,
                    X509Certificate2 cert,
                    SignatureProductionPlace productionPlace = null,
                    SignerRole signerRole = null) {
            var tempFilePath = Path.Combine(Path.GetTempPath(), (item as DocumentFile).FileName);
            File.WriteAllBytes(tempFilePath, (item as DocumentFile).FileData);
            var result = xadesManager.CreateDetachedSignature(tempFilePath, cert, productionPlace, signerRole);
            if (result != null) {
                using (var msOutput = new MemoryStream()) {
                    result.Save(msOutput);
                    var xadesFile = new DocumentFile() {
                        FileName = $"{(item as DocumentFile).FileName}.xades",
                        FileData = msOutput.ToArray()
                    };
                    var folder = mgr.GetParentFolder(item);
                    if (folder != null) {
                        folder.AddItem(xadesFile);

                        // add metadata
                        var metadataFile = mgr.GetMetadataFile(item);
                        if (metadataFile != null) {
                            using (var converter = new Converters.XmlConverter()) {
                                var xadesMetadataFileDocumentXml = converter.GetXml(metadataFile.Document);
                                var xadesMetadataFileDocument = converter.ParseXml(xadesMetadataFileDocumentXml);
                                xadesMetadataFileDocument.Description = $"Digital signature of file {(item as DocumentFile).FileName}.";
                                var xadesMetadataFile = new MetadataFile() {
                                    FileName = $"{xadesFile.FileName}.xml",
                                    Document = xadesMetadataFileDocument
                                };

                                var metadataFolder = mgr.GetParentFolder(metadataFile);
                                if (metadataFolder != null) {
                                    metadataFolder.AddItem(xadesMetadataFile);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Dispose() { }
    }

    static class Extensions {
        public static string GetXmlEnum(this Enum value) {
            try {
                var fi = value.GetType().GetField(value.ToString());
                XmlEnumAttribute[] attributes = (XmlEnumAttribute[])fi.GetCustomAttributes(typeof(XmlEnumAttribute), false);
                return (attributes.Length > 0) ? attributes[0].Name : value.ToString();
            }
            catch {
                return string.Empty;
            }
        }
    }
}
