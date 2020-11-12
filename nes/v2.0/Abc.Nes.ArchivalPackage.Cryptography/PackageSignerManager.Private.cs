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
using Abc.Nes.ArchivalPackage.Model;
using Abc.Nes.Xades;
using Abc.Nes.Xades.Signature.Parameters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace Abc.Nes.ArchivalPackage.Cryptography {
    partial class PackageSignerManager {
        private const string SigTextFormat = "Signed by: {0} \r\nSigned on: {1:MM/dd/yyyy HH:mm:ss}";
        private void SignPdfItem(ItemBase item, X509Certificate2 cert) {
            var input = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
            var output = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
            var temp = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
            try {
                File.WriteAllBytes(input, (item as DocumentFile).FileData);
                var reader = new iTextSharp.text.pdf.PdfReader(input);
                var outputStream = new FileStream(output, FileMode.Create);
                var pdfStamper = iTextSharp.text.pdf.PdfStamper.CreateSignature(reader, outputStream, '\0', null, true);

                var cp = new Org.BouncyCastle.X509.X509CertificateParser();
                var chain = new[] { cp.ReadCertificate(cert.RawData) };

                var appearance = pdfStamper.SignatureAppearance;

                SetSigPosition(appearance, reader.AcroFields.GetSignatureNames().Count);
                SetSigText(appearance, chain);
                SetSigCryptoFromX509(cert, chain, appearance);

                //pdfStamper.Close();
                reader.Close();
                outputStream.Close();

                item.Init(File.ReadAllBytes(output));
            }
            finally {
                try {
                    if (File.Exists(input)) { File.Delete(input); }
                    if (File.Exists(output)) { File.Delete(output); }
                    if (File.Exists(temp)) { File.Delete(temp); }
                }
                catch { }
            }
        }
        private void SetSigText(iTextSharp.text.pdf.PdfSignatureAppearance sigAppearance, IList<Org.BouncyCastle.X509.X509Certificate> chain) {
            sigAppearance.SignDate = DateTime.Now;
            var signedBy = iTextSharp.text.pdf.PdfPkcs7.GetSubjectFields(chain[0]).GetField("CN");
            var signedOn = sigAppearance.SignDate;
            sigAppearance.Layer2Text = String.Format(SigTextFormat, signedBy, signedOn);
        }
        private void SetSigCryptoFromX509(X509Certificate2 cert, Org.BouncyCastle.X509.X509Certificate[] chain, iTextSharp.text.pdf.PdfSignatureAppearance appearance) {
            appearance.SetCrypto(null, chain, null, iTextSharp.text.pdf.PdfSignatureAppearance.WincerSigned);
            appearance.CryptoDictionary = new iTextSharp.text.pdf.PdfSignature(iTextSharp.text.pdf.PdfName.AdobePpkms, iTextSharp.text.pdf.PdfName.AdbePkcs7Sha1) {
                Date = new iTextSharp.text.pdf.PdfDate(appearance.SignDate),
                Name = iTextSharp.text.pdf.PdfPkcs7.GetSubjectFields(chain[0]).GetField("CN"),
                Reason = appearance.Reason,
                Location = appearance.Location
            };

            const int csize = 4000;
            var exc = new Dictionary<iTextSharp.text.pdf.PdfName, int> { { iTextSharp.text.pdf.PdfName.Contents, csize * 2 + 2 } };
            var htable = new System.Collections.Hashtable(exc);
            appearance.PreClose(htable);

            var sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();

            var s = appearance.RangeStream;
            int read;
            var buff = new byte[8192];
            while ((read = s.Read(buff, 0, 8192)) > 0) {
                sha.TransformBlock(buff, 0, read, buff, 0);
            }
            sha.TransformFinalBlock(buff, 0, 0);
            var pk = SignMsg(sha.Hash, cert, false);

            var outc = new byte[csize];

            var dic2 = new iTextSharp.text.pdf.PdfDictionary();

            Array.Copy(pk, 0, outc, 0, pk.Length);

            dic2.Put(iTextSharp.text.pdf.PdfName.Contents, new iTextSharp.text.pdf.PdfString(outc).SetHexWriting(true));

            appearance.Close(dic2);
        }
        private void SetSigPosition(iTextSharp.text.pdf.PdfSignatureAppearance sigAppearance, int oldSigCount) {
            //Note: original formula from QuangNgV, ll = lower left, ur = upper right, coordinates are calculated relative from the lower left of the pdf page
            float llx = (100 + 20) * (oldSigCount % 5),
                    lly = (25 + 20) * (oldSigCount / 5),
                    urx = llx + 100,
                    ury = lly + 25;
            sigAppearance.SetVisibleSignature(new iTextSharp.text.Rectangle(llx, lly, urx, ury), 1, null);
        }
        private byte[] SignMsg(Byte[] msg, X509Certificate2 signerCert, bool detached) {
            //  Place message in a ContentInfo object.
            //  This is required to build a SignedCms object.
            var contentInfo = new System.Security.Cryptography.Pkcs.ContentInfo(msg);

            //  Instantiate SignedCms object with the ContentInfo above.
            //  Has default SubjectIdentifierType IssuerAndSerialNumber.
            var signedCms = new System.Security.Cryptography.Pkcs.SignedCms(contentInfo, detached);

            //  Formulate a CmsSigner object for the signer.
            var cmsSigner = new System.Security.Cryptography.Pkcs.CmsSigner(signerCert);

            // Include the following line if the top certificate in the
            // smartcard is not in the trusted list.
            cmsSigner.IncludeOption = X509IncludeOption.EndCertOnly;

            //  Sign the CMS/PKCS #7 message. The second argument is
            //  needed to ask for the pin.
            signedCms.ComputeSignature(cmsSigner, false);

            //  Encode the CMS/PKCS #7 message.
            return signedCms.Encode();
        }

        //private void SignPdfItem(ItemBase item, X509Certificate2 cert) {
        //    var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
        //    try {
        //        File.WriteAllBytes(tempFilePath, (item as DocumentFile).FileData);
        //        using (var stream = new FileStream(tempFilePath, FileMode.Open)) {
        //            using (var doc = new Aspose.Pdf.Document(stream)) {
        //                var field1 = new Aspose.Pdf.Forms.SignatureField(doc.Pages[1], new Aspose.Pdf.Rectangle(10, 10, 300, 50));
        //                var externalSignature = new Aspose.Pdf.Forms.ExternalSignature(cert);
        //                field1.PartialName = "sig1";
        //                doc.Form.Add(field1, 1);
        //                field1.Sign(externalSignature);
        //            }
        //        }
        //        item.Init(File.ReadAllBytes(tempFilePath));
        //    }
        //    finally {
        //        try {
        //            if (File.Exists(tempFilePath)) { File.Delete(tempFilePath); }
        //        }
        //        catch { }
        //    }

        //}

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
                                var xadesMetadataFileDocument = converter.ParseXml(xadesMetadataFileDocumentXml) as Document;
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

        private SignatureInfo GetPadesInfo(byte[] fileData) {
            throw new NotImplementedException();
        }

        private SignatureInfo GetSignatureInfo(XElement e) {
            throw new NotImplementedException();
            //return default;
        }
    }
}
