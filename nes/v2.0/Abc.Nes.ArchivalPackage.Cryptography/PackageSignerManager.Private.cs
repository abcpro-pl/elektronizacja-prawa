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
using iText.Kernel.Pdf;
using iText.Signatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace Abc.Nes.ArchivalPackage.Cryptography {
    partial class PackageSignerManager {
        private iText.Kernel.Geom.Rectangle GetApperanceImageRect(
                PdfSignatureLocation apperancePngImageLocation,
                float pageWidth,
                float pageHeight,
                float apperanceLocationX = 0,
                float apperanceLocationY = 0,
                float apperanceWidth = 200F,
                float apperanceHeight = 50F,
                float margin = 10F) {
            var result = new iText.Kernel.Geom.Rectangle(apperanceLocationX, apperanceLocationY, apperanceWidth, apperanceHeight);
            if (apperancePngImageLocation != PdfSignatureLocation.Custom) {
                switch (apperancePngImageLocation) {
                    case PdfSignatureLocation.BottomLeft: { result = new iText.Kernel.Geom.Rectangle(margin, margin, apperanceWidth, apperanceHeight); break; }
                    case PdfSignatureLocation.BottomRight: { result = new iText.Kernel.Geom.Rectangle(pageWidth - (margin + apperanceWidth), margin, apperanceWidth, apperanceHeight); break; }
                    case PdfSignatureLocation.TopLeft: { result = new iText.Kernel.Geom.Rectangle(margin, pageHeight - (margin + apperanceHeight), apperanceWidth, apperanceHeight); break; }
                    case PdfSignatureLocation.TopRight: { result = new iText.Kernel.Geom.Rectangle(pageWidth - (margin + apperanceWidth), pageHeight - (margin + apperanceHeight), apperanceWidth, apperanceHeight); break; }
                }
            }

            return result;
        }
        private void signPdfFile(
                string sourceFilePath,
                X509Certificate2 cert,
                string reason,
                string location,
                bool addTimeStamp,
                string timeStampServerUrl,
                byte[] apperancePngImage,
                PdfSignatureLocation apperancePngImageLocation,
                float apperanceLocationX,
                float apperanceLocationY,
                float apperanceWidth,
                float apperanceHeight,
                float margin,
                string outputFilePath) {

            if (apperancePngImage == null) { apperancePngImage = Properties.Resources.nes_stamp; }

            var temp = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
            ITSAClient tsa = null;

            if (!HasSignatures(sourceFilePath)) {
                float pageWidth = 0;
                float pageHeight = 0;
                using (var reader = new PdfReader(sourceFilePath)) {
                    using (var pdfDoc = new PdfDocument(reader, new StampingProperties())) {
                        var page = pdfDoc.GetPage(1);
                        var pageRect = page.GetPageSize();
                        pageWidth = pageRect.GetWidth();
                        pageHeight = pageRect.GetHeight();
                    }
                }

                using (var reader = new PdfReader(sourceFilePath)) {
                    using (var fs = new FileStream(temp, FileMode.Create)) {
                        var signer = new PdfSigner(reader, fs, new StampingProperties());
                        var rect = GetApperanceImageRect(apperancePngImageLocation, pageWidth, pageHeight,
                            apperanceLocationX, apperanceLocationY, apperanceWidth, apperanceHeight, margin);
                        var appearance = signer.GetSignatureAppearance();
                        appearance
                            .SetPageRect(rect)
                            .SetPageNumber(1)
                            .SetReason(reason)
                            .SetReasonCaption("Rodzaj: ")
                            .SetLocation(location)
                            .SetLocationCaption("Miejsce: ")
                            .SetImage(iText.IO.Image.ImageDataFactory.CreatePng(apperancePngImage));

                        appearance.SetRenderingMode(PdfSignatureAppearance.RenderingMode.DESCRIPTION);
                        signer.SetFieldName($"sig-{Guid.NewGuid()}");

                        if (addTimeStamp) { tsa = new TSAClientBouncyCastle(timeStampServerUrl); }
                        var signature = new X509Certificate2Signature(cert, HashAlgorithmName.SHA256.Name);
                        signer.SignDetached(signature, GetChain(cert), null, null, tsa, 0, PdfSigner.CryptoStandard.CADES);
                    }
                }

                if (addTimeStamp) {
                    using (var reader = new PdfReader(temp)) {
                        var ocsp = new OcspClientBouncyCastle(null);
                        var crl = new CrlClientOnline();

                        using (var writer = new PdfWriter(outputFilePath)) {
                            using (var pdfDoc = new PdfDocument(reader, writer, new StampingProperties().UseAppendMode())) {
                                var v = new LtvVerification(pdfDoc);
                                var signatureUtil = new SignatureUtil(pdfDoc);
                                var names = signatureUtil.GetSignatureNames();
                                var sigName = names[names.Count - 1];
                                var pkcs7 = signatureUtil.ReadSignatureData(sigName);
                                if (pkcs7.IsTsp()) {
                                    v.AddVerification(sigName, ocsp, crl, LtvVerification.CertificateOption.WHOLE_CHAIN,
                                        LtvVerification.Level.OCSP_CRL, LtvVerification.CertificateInclusion.NO);
                                }
                                else {
                                    foreach (var name in names) {
                                        v.AddVerification(name, ocsp, crl, LtvVerification.CertificateOption.WHOLE_CHAIN,
                                             LtvVerification.Level.OCSP_CRL, LtvVerification.CertificateInclusion.NO);
                                    }
                                }
                                v.Merge();
                                pdfDoc.Close();
                            }
                            writer.Close();
                        }
                    }
                }

                try {
                    if (File.Exists(temp)) { File.Delete(temp); }
                }
                catch { }
            }
        }
        private void SignPdfItem(ItemBase item, X509Certificate2 cert, bool addTimeStamp, string reason = "eADM Signing", string location = null, string timeStampServerUrl = "http://time.certum.pl") {
            var input = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
            var output = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
            var temp = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

            if (location == null) { location = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToUpper(); }

            ITSAClient tsa = null;

            File.WriteAllBytes(input, (item as DocumentFile).FileData);

            if (!HasSignatures(input)) {
                float pageWidth = 0;
                float pageHeight = 0;
                using (var reader = new PdfReader(input)) {
                    using (var pdfDoc = new PdfDocument(reader, new StampingProperties())) {
                        var page = pdfDoc.GetPage(1);
                        var pageRect = page.GetPageSize();
                        pageWidth = pageRect.GetWidth();
                        pageHeight = pageRect.GetHeight();
                    }
                }

                using (PdfReader reader = new PdfReader(input)) {
                    using (var fs = new FileStream(temp, FileMode.Create)) {
                        PdfSigner signer = new PdfSigner(reader, fs, new StampingProperties());

                        var rect = GetApperanceImageRect(PdfSignatureLocation.BottomLeft, pageWidth, pageHeight);
                        PdfSignatureAppearance appearance = signer.GetSignatureAppearance();
                        appearance
                            .SetPageRect(rect)
                            .SetPageNumber(1)
                            .SetReason(reason)
                            .SetReasonCaption("Rodzaj: ")
                            .SetLocation(location)
                            .SetLocationCaption("Miejsce: ")
                            .SetImage(iText.IO.Image.ImageDataFactory.CreatePng(Properties.Resources.nes_stamp));
                        appearance.SetRenderingMode(PdfSignatureAppearance.RenderingMode.DESCRIPTION);
                        signer.SetFieldName($"sig-{Guid.NewGuid()}");

                        if (addTimeStamp) { tsa = new TSAClientBouncyCastle(timeStampServerUrl); }
                        var signature = new X509Certificate2Signature(cert, HashAlgorithmName.SHA256.Name);
                        signer.SignDetached(signature, GetChain(cert), null, null, tsa, 0, PdfSigner.CryptoStandard.CADES);

                        //https://kb.itextpdf.com/home/it7kb/faq/how-to-enable-ltv-for-a-timestamp-signature
                    }
                }

                if (addTimeStamp) {
                    using (PdfReader reader = new PdfReader(temp)) {
                        IOcspClient ocsp = new OcspClientBouncyCastle(null);
                        ICrlClient crl = new CrlClientOnline();

                        using (PdfWriter writer = new PdfWriter(output)) {
                            using (PdfDocument pdfDoc = new PdfDocument(reader, writer, new StampingProperties().UseAppendMode())) {
                                LtvVerification v = new LtvVerification(pdfDoc);
                                SignatureUtil signatureUtil = new SignatureUtil(pdfDoc);
                                var names = signatureUtil.GetSignatureNames();
                                var sigName = names[names.Count - 1];
                                PdfPKCS7 pkcs7 = signatureUtil.ReadSignatureData(sigName);
                                if (pkcs7.IsTsp()) {
                                    v.AddVerification(sigName, ocsp, crl, LtvVerification.CertificateOption.WHOLE_CHAIN,
                                        LtvVerification.Level.OCSP_CRL, LtvVerification.CertificateInclusion.NO);
                                }
                                else {
                                    foreach (var name in names) {
                                        v.AddVerification(name, ocsp, crl, LtvVerification.CertificateOption.WHOLE_CHAIN,
                                            LtvVerification.Level.OCSP_CRL, LtvVerification.CertificateInclusion.NO);
                                    }
                                }
                                v.Merge();
                                pdfDoc.Close();
                            }
                            writer.Close();
                        }
                    }
                }

                if (File.Exists(output)) {
                    item.Init(File.ReadAllBytes(output));
                }
                else if (File.Exists(temp)) {
                    item.Init(File.ReadAllBytes(temp));
                }

                try {
                    if (File.Exists(input)) { File.Delete(input); }
                    if (File.Exists(output)) { File.Delete(output); }
                    if (File.Exists(temp)) { File.Delete(temp); }
                }
                catch { }
            }
        }

        /// <summary>
        /// https://stackoverflow.com/questions/50627483/create-iexternalsignature-with-x509certificate2-in-c-sharp-and-itext-7
        ///  https://git.itextsupport.com/projects/I5N/repos/itextsharp/browse/src/core/iTextSharp/text/pdf/security/X509Certificate2Signature.cs?at=refs/heads/develop
        /// </summary>
        class X509Certificate2Signature : IExternalSignature {
            private String hashAlgorithm;
            private String encryptionAlgorithm;
            private X509Certificate2 certificate;

            public X509Certificate2Signature(X509Certificate2 certificate, String hashAlgorithm) {
                if (!certificate.HasPrivateKey)
                    throw new ArgumentException("No private key.");
                this.certificate = certificate;
                this.hashAlgorithm = DigestAlgorithms.GetDigest(DigestAlgorithms.GetAllowedDigest(hashAlgorithm));
                if (certificate.PrivateKey is RSACryptoServiceProvider)
                    encryptionAlgorithm = "RSA";
                else if (certificate.PrivateKey is DSACryptoServiceProvider)
                    encryptionAlgorithm = "DSA";
                else
                    throw new ArgumentException("Unknown encryption algorithm " + certificate.PrivateKey);
            }

            public virtual byte[] Sign(byte[] message) {
                if (certificate.PrivateKey is RSACryptoServiceProvider) {
                    RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)certificate.PrivateKey;
                    return rsa.SignData(message, hashAlgorithm);
                }
                else {
                    DSACryptoServiceProvider dsa = (DSACryptoServiceProvider)certificate.PrivateKey;
                    return dsa.SignData(message);
                }
            }

            public virtual String GetHashAlgorithm() {
                return hashAlgorithm;
            }

            public virtual String GetEncryptionAlgorithm() {
                return encryptionAlgorithm;
            }
        }

        private Org.BouncyCastle.X509.X509Certificate[] GetChain(X509Certificate2 cert) {
            var list = new List<Org.BouncyCastle.X509.X509Certificate>();

            X509Chain ch = new X509Chain();
            ch.Build(cert);

            foreach (var item in ch.ChainElements) {
                list.Add(Org.BouncyCastle.Security.DotNetUtilities.FromX509Certificate(item.Certificate));
            }
            return list.ToArray();
        }

        private bool HasSignatures(string filePath) {
            using (PdfReader reader = new PdfReader(filePath)) {
                using (var doc = new PdfDocument(reader)) {
                    if (iText.Forms.PdfAcroForm.GetAcroForm(doc, false) != null) {
                        var names = new SignatureUtil(doc).GetSignatureNames();
                        return names != null && names.Count > 0;
                    }
                }
            }
            return default;
        }
        /*
        private const string SigTextFormat = "Signed by: {0} \r\nSigned on: {1:MM/dd/yyyy HH:mm:ss}";
        private void SignPdfItem(ItemBase item, X509Certificate2 cert, bool addTimeStamp) {
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
                SetSigCryptoFromX509(cert, chain, appearance, addTimeStamp);

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
        private void SetSigCryptoFromX509(X509Certificate2 cert, Org.BouncyCastle.X509.X509Certificate[] chain, iTextSharp.text.pdf.PdfSignatureAppearance appearance, bool addTimeStamp) {
            appearance.SetCrypto(null, chain, null, iTextSharp.text.pdf.PdfSignatureAppearance.WincerSigned);
            appearance.CryptoDictionary = new iTextSharp.text.pdf.PdfSignature(iTextSharp.text.pdf.PdfName.AdobePpkms, iTextSharp.text.pdf.PdfName.AdbePkcs7Sha1) {
                Date = new iTextSharp.text.pdf.PdfDate(appearance.SignDate),
                Name = iTextSharp.text.pdf.PdfPkcs7.GetSubjectFields(chain[0]).GetField("CN"),
                Reason = appearance.Reason,
                Location = appearance.Location
            };

            const int csize = 4000;
            var appendix = 2;
            var exc = new Dictionary<iTextSharp.text.pdf.PdfName, int> { { iTextSharp.text.pdf.PdfName.Contents, csize * 2 + appendix } };
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
            var pk = SignMsg(sha.Hash, cert, false, addTimeStamp);

            var outc = new byte[csize];

            var dic2 = new iTextSharp.text.pdf.PdfDictionary();

            Array.Copy(pk, 0, outc, 0, pk.Length);

            dic2.Put(iTextSharp.text.pdf.PdfName.Contents, new iTextSharp.text.pdf.PdfString(outc).SetHexWriting(true));

            //appearance.CryptoDictionary.Remove(iTextSharp.text.pdf.PdfName.Contents);

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
        private byte[] SignMsg(Byte[] msg, X509Certificate2 signerCert, bool detached, bool addTimeStamp = true) {
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
            cmsSigner.IncludeOption = X509IncludeOption.WholeChain;

            //if (addTimeStamp) {
            //    using (var client = new TimeStamp.TimeStampClient()) {
            //        var token = client.RequestTimeStampToken("http://time.certum.pl", msg, TimeStamp.Oid.SHA1);
            //        cmsSigner.UnsignedAttributes.Add(new System.Security.Cryptography.Pkcs.Pkcs9SigningTime(token.EncodedToken));
            //    }
            //}

            //  Sign the CMS/PKCS #7 message. The second argument is
            //  needed to ask for the pin.
            signedCms.ComputeSignature(cmsSigner, false);

            //  Encode the CMS/PKCS #7 message.
            return signedCms.Encode();
        }
        */

        /*
        private void SignPdfItem(ItemBase item, X509Certificate2 cert, bool addTimeStamp) {
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
            try {
                File.WriteAllBytes(tempFilePath, (item as DocumentFile).FileData);

                //using (var stream = new FileStream(tempFilePath, FileMode.Open, FileAccess.ReadWrite)) {
                    using (var doc = new Aspose.Pdf.Document(tempFilePath)) {
                        if (doc.Form != null && doc.Form.Fields != null) {
                            foreach (var field in doc.Form.Fields) {
                                var sf = field as Aspose.Pdf.Forms.SignatureField;
                                if (sf != null) { return; } // this document has signature.
                            }
                        }

                        using (var sig = new Aspose.Pdf.Facades.PdfFileSignature(doc)) {
                            var pkcs = new Aspose.Pdf.Forms.ExternalSignature(cert);
                            var timestampSettings = new Aspose.Pdf.TimestampSettings("http://time.certum.pl", string.Empty); // User/Password can be omitted
                            pkcs.TimestampSettings = timestampSettings;
                            sig.Sign(1, true, new System.Drawing.Rectangle(10, 10, 300, 50), pkcs);
                            sig.Save(tempFilePath);
                        }


                        //var field1 = new Aspose.Pdf.Forms.SignatureField(doc.Pages[1], new Aspose.Pdf.Rectangle(10, 10, 300, 50));
                        //var externalSignature = new Aspose.Pdf.Forms.ExternalSignature(cert) {
                        //    //Authority = "Me",
                        //    //Reason = "Reason",
                        //    //ContactInfo = "Contact"
                        //    //TimestampSettings = addTimeStamp ? new Aspose.Pdf.TimestampSettings("http://time.certum.pl", String.Empty) : null
                        //};

                        //if (addTimeStamp) {
                        //    externalSignature.TimestampSettings = new Aspose.Pdf.TimestampSettings("http://time.certum.pl", String.Empty);
                        //}

                        //field1.PartialName = "sig1";
                        //doc.Form.Add(field1, 1);
                        //field1.Sign(externalSignature);

                        //if (externalSignature.Verify()) {
                        //    doc.Save();
                        //}

                    }
                //}
                item.Init(File.ReadAllBytes(tempFilePath));
            }
            finally {
                try {
                    if (File.Exists(tempFilePath)) { File.Delete(tempFilePath); }
                }
                catch { }
            }

        }
        */

        private void SignXmlItem(ItemBase item,
                         XadesManager xadesManager,
                         X509Certificate2 cert,
                         SignatureProductionPlace productionPlace = null,
                         SignerRole signerRole = null,
                         XadesFormat xadesFormat = XadesFormat.XadesBes,
                         string timeStampServerUrl = "http://time.certum.pl") {
            var ms = new MemoryStream((item as DocumentFile).FileData);
            Xades.Upgraders.SignatureFormat? upgradeFormat = null;
            if (xadesFormat == XadesFormat.XadesT) {
                upgradeFormat = Xades.Upgraders.SignatureFormat.XAdES_T;
            }
            else if (xadesFormat == XadesFormat.XadesXL) {
                upgradeFormat = Xades.Upgraders.SignatureFormat.XAdES_XL;
            }

            var result = xadesManager.AppendSignatureToXmlFile(ms, cert, productionPlace, signerRole, upgradeFormat, timeStampServerUrl);
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
                     SignerRole signerRole = null,
                     bool addTimeStamp = false,
                     string timeStampServerUrl = "http://time.certum.pl") {
            var ms = new MemoryStream((item as DocumentFile).FileData);
            Xades.Upgraders.SignatureFormat? xadesFormat = addTimeStamp ? Xades.Upgraders.SignatureFormat.XAdES_T : (Xades.Upgraders.SignatureFormat?)null;
            var result = xadesManager.CreateEnvelopingSignature(ms, cert, productionPlace, signerRole, null, xadesFormat, timeStampServerUrl);
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
                    SignerRole signerRole = null,
                    bool addTimeStamp = false,
                    string timeStampServerUrl = "http://time.certum.pl") {
            var tempFilePath = Path.Combine(Path.GetTempPath(), (item as DocumentFile).FileName);
            File.WriteAllBytes(tempFilePath, (item as DocumentFile).FileData);
            Xades.Upgraders.SignatureFormat? xadesFormat = addTimeStamp ? Xades.Upgraders.SignatureFormat.XAdES_T : (Xades.Upgraders.SignatureFormat?)null;
            var result = xadesManager.CreateDetachedSignature(tempFilePath, cert, productionPlace, signerRole, xadesFormat, timeStampServerUrl);
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

        private SignatureInfo[] GetPadesInfos(byte[] fileData, string fileName = null) {
            using (var pdfDoc = new PdfDocument(new PdfReader(new MemoryStream(fileData)))) {
                var signUtil = new SignatureUtil(pdfDoc);
                var names = signUtil.GetSignatureNames();
                if (names != null && names.Count > 0) {
                    var list = new List<SignatureInfo>();
                    foreach (var name in names) {
                        var pkcs7 = signUtil.ReadSignatureData(name);
                        if (pkcs7 != null) {
                            var cert = new X509Certificate2(pkcs7.GetSigningCertificate().GetEncoded());
                            if (cert != null) {
                                var item = new SignatureInfo() {
                                    CreateDate = pkcs7.GetSignDate(),
                                    Certificate = cert,
                                    Author = new SubjectNameInfo(cert.Subject)["CN"],
                                    Publisher = new SubjectNameInfo(cert.Issuer)["CN"],
                                    SignatureType = SignatureType.Pades,
                                    SignatureNumber = name,
                                    FileName = fileName
                                };
                                if (item.Author != null) { item.Author = item.Author.Replace("\"", String.Empty); }
                                if (item.Publisher != null) { item.Publisher = item.Publisher.Replace("\"", String.Empty); }

                                list.Add(item);
                            }
                        }
                    }
                    return list.ToArray();
                }
            }
            return default;
        }

        private SignatureInfo GetSignatureInfo(XElement e) {
            if (e != null) {
                var result = new SignatureInfo() {
                    SignatureType = SignatureType.Xades,
                    SignatureNumber = e.Attribute("Id").Value
                };

                var keyName = e.Descendants().Where(x => x.Name.LocalName == "KeyName").FirstOrDefault();
                if (keyName != null) {
                    result.Author = keyName.Value;
                }
                else {
                    var xSubjectName = e.Descendants().Where(x => x.Name.LocalName == "X509SubjectName").FirstOrDefault();
                    if (xSubjectName != null) {
                        var subjectName = new SubjectNameInfo(xSubjectName.Value);
                        if (subjectName.Count > 0) {
                            result.Author = subjectName["CN"];
                        }
                    }
                }

                var signingTime = e.Descendants().Where(x => x.Name.LocalName == "SigningTime").FirstOrDefault();
                if (signingTime != null) {
                    result.CreateDate = Convert.ToDateTime(signingTime.Value);
                }

                var xIssuerName = e.Descendants().Where(x => x.Name.LocalName == "X509IssuerName").FirstOrDefault();
                if (xIssuerName != null) {
                    var issuerName = new SubjectNameInfo(xIssuerName.Value);
                    if (issuerName.Count > 0) {
                        result.Publisher = issuerName["CN"];
                    }
                }

                var xmlCert = e.Descendants().Where(x => x.Name.LocalName == "X509Certificate").FirstOrDefault();
                try {
                    var data = Convert.FromBase64String(xmlCert.Value);
                    var cert = new X509Certificate2(data);
                    result.Certificate = cert;
                    if (result.Author == null) {
                        var subjectName = new SubjectNameInfo(cert.Subject);
                        if (subjectName.Count > 0) {
                            result.Author = subjectName["CN"];
                        }
                    }
                    if (result.Publisher == null) {
                        var issuerName = new SubjectNameInfo(cert.Issuer);
                        if (issuerName.Count > 0) {
                            result.Publisher = issuerName["CN"];
                        }
                    }
                }
                catch { }

                if (result.Author != null) { result.Author = result.Author.Replace("\"", String.Empty); }
                if (result.Publisher != null) { result.Publisher = result.Publisher.Replace("\"", String.Empty); }

                return result;
            }
            return default;
        }
    }
}
