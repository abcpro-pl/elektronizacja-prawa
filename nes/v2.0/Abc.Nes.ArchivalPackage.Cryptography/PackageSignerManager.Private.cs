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
using Abc.Nes.Cryptography;
using Abc.Nes.Xades;
using Abc.Nes.Xades.Signature.Parameters;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Signatures;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using Abc.Nes.Common;
using Abc.Nes.Common.Clients;
using Abc.Nes.Common.Helpers;
using Abc.Nes.Common.Models;

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
                float margin = 10F,
                int signatureCount = 0) {

            float x = apperanceLocationX,
                y = apperanceLocationY,
                width = apperanceWidth,
                height = apperanceHeight;

            //var result = new iText.Kernel.Geom.Rectangle(apperanceLocationX, apperanceLocationY, apperanceWidth, apperanceHeight);
            if (apperancePngImageLocation != PdfSignatureLocation.Custom) {
                switch (apperancePngImageLocation) {
                    case PdfSignatureLocation.BottomLeft: { 
                            x = margin;
                            y = margin;
                            if (signatureCount > 0)
                                y += height * signatureCount;
                            break;
                        }
                    case PdfSignatureLocation.BottomRight: { 
                            x = pageWidth - (margin + apperanceWidth);
                            y = margin;
                            if (signatureCount > 0)
                                y += height * signatureCount;
                            break;
                        }

                    case PdfSignatureLocation.TopLeft: { 
                            x = margin;
                            y = pageHeight - (margin + apperanceHeight);
                            if (signatureCount > 0)
                                y -= height * signatureCount;
                            break;
                        }
                    case PdfSignatureLocation.TopRight: { 
                            x = pageWidth - (margin + apperanceWidth);
                            y = pageHeight - (margin + apperanceHeight);
                            if (signatureCount > 0)
                                y -= height * signatureCount;
                            break;
                        }
                }
            }

            var result = new iText.Kernel.Geom.Rectangle(x, y, width, height);

            return result;
        }

        private void signPdfFile(string sourceFilePath, PdfSignatureOptions options, string outputFilePath) {
            var temp = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
            TSAClientAbcBouncyCastle tsa = null;

            int signatureCount = SignatureCount(sourceFilePath);

            if (signatureCount == 0 || options.AllowMultipleSignatures) {
                float pageWidth = 0;
                float pageHeight = 0;
                using (var reader = new PdfReader(sourceFilePath)) {
                    using (var pdfDoc = new PdfDocument(reader, new StampingProperties())) {
                        var page = pdfDoc.GetPage(options.PageNumber);
                        var pageRect = page.GetPageSize();
                        pageWidth = pageRect.GetWidth();
                        pageHeight = pageRect.GetHeight();
                    }
                }

                using (var reader = new PdfReader(sourceFilePath)) {
                    using (var fs = new FileStream(temp, FileMode.Create)) {

                        StampingProperties properties = new StampingProperties();
                        if (signatureCount > 0 && options.AllowMultipleSignatures)
                            properties.UseAppendMode();

                        var signer = new PdfSigner(reader, fs, properties);
                        if (options.SignDate.HasValue)
                            signer.SetSignDate(options.SignDate.Value);

                        if (options.AddVisibleSignature) {
                            var rect = GetApperanceImageRect(options.SignatureLocation, pageWidth, pageHeight,
                                options.PositionX, options.PositionY, options.Width, options.Height, options.Margin, signatureCount);
                            var sb = new StringBuilder();
                            sb.AppendLine(options.SignatureTitle);
                            //var signerName = cert.FriendlyName;
                            //if (signerName == null || signerName == string.Empty)
                            string signerName;
                            if (options.SignerName.IsNotNullOrEmpty())
                                signerName = options.SignerName;
                            else
                                signerName = options.Certificate.GetNameInfo(X509NameType.SimpleName, false);

                            sb.AppendLine(signerName);
                            string dateString = DateTimeHelper.FormatDateForSignature(options.SignDate.Value,
                                                                                      options.DatePrefix,
                                                                                      options.DatePostfix,
                                                                                      options.DateCultureInfoName,
                                                                                      options.DateStringFormat);
                            sb.Append(dateString);

                            var appearance = signer.GetSignatureAppearance();
                            appearance.SetReason(options.Reason.GetDescription())
                                .SetReasonCaption(options.ReasonText);

                            if (options.Location != null)
                                appearance.SetLocation(options.Location)
                                    .SetLocationCaption(options.LocationText);


                            appearance
                                .SetLayer2FontSize(options.FontSize)
                                .SetPageRect(rect)
                                .SetPageNumber(options.PageNumber);
                            string signText = sb.ToString();
                            appearance.SetLayer2Text(signText);

                            iText.IO.Image.ImageData image;
                            appearance.SetRenderingMode(PdfSignatureAppearance.RenderingMode.DESCRIPTION);
                            bool emptyImage = options.SignatureImage == null;
                            if (!emptyImage) {
                                image = iText.IO.Image.ImageDataFactory.CreatePng(options.SignatureImage);
                            }
                            else image = ImageDataFactory.CreatePng(Properties.Resources.img_blank);

                            if (options.ImageAsBackground) {
                                if (!emptyImage)
                                    appearance.SetImage(image);
                            }
                            else {
                                //custom widget
                                var pdfDoc = signer.GetDocument();
                                var layer2 = appearance.GetLayer2();
                                var canvas = new PdfCanvas(layer2, pdfDoc);

                                float MARGIN = 1;

                                iText.IO.Font.FontProgram fontProgram = iText.IO.Font.FontProgramFactory.CreateFont();
                                PdfFont font = PdfFontFactory.CreateFont(fontProgram, "cp1250", true);
                                

                                float width = rect.GetWidth();
                                float height = rect.GetHeight();
                                iText.Kernel.Geom.Rectangle imgRect;
                                iText.Kernel.Geom.Rectangle signatureRect;

                                //square image left, signature right
                                if (emptyImage) {
                                    imgRect = new iText.Kernel.Geom.Rectangle(0,0);
                                    signatureRect = new iText.Kernel.Geom.Rectangle(MARGIN, MARGIN, rect.GetWidth() - MARGIN, rect.GetHeight() - MARGIN);
                                }
                                else {
                                    imgRect = new iText.Kernel.Geom.Rectangle(MARGIN, MARGIN, rect.GetHeight() - MARGIN * 2, rect.GetHeight() - MARGIN * 2);
                                    signatureRect = new iText.Kernel.Geom.Rectangle(rect.GetHeight() + MARGIN, MARGIN, width - rect.GetHeight(), rect.GetHeight() - 2 * MARGIN);
                                }

                                //image on top, signature bottom
                                //iText.Kernel.Geom.Rectangle dataRect = new iText.Kernel.Geom.Rectangle(MARGIN, rect.GetHeight() / 2 + MARGIN, rect.GetWidth() - 2 * MARGIN, rect.GetHeight() / 2 - MARGIN);
                                //iText.Kernel.Geom.Rectangle signatureRect = new iText.Kernel.Geom.Rectangle(MARGIN, MARGIN, rect.GetWidth() - 2 * MARGIN, rect.GetHeight() / 2 - MARGIN);

                                try {
                                    //text element
                                    Canvas signLayoutCanvas = new Canvas(canvas, signatureRect);
                                    Paragraph para = new Paragraph(signText)
                                        //.SetMultipliedLeading(.9f)
                                        .SetFont(font)
                                        .SetFontSize(options.FontSize)
                                        //.SetTextRenderingMode(PdfCanvasConstants.TextRenderingMode.FILL_CLIP)
                                        
                                        .AddStyle(new Style { })
                                        .SetMargin(0);
                                    var div = new Div();
                                    div.SetHeight(signatureRect.GetHeight());
                                    div.SetWidth(signatureRect.GetWidth());
                                    div.SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE);
                                    div.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                                    div.Add(para);
                                    signLayoutCanvas.Add(div);

                                    if (!emptyImage) {
                                        //image element
                                        Canvas dataLayoutCanvas = new Canvas(canvas, imgRect);
                                        Image sImage = new Image(image);
                                        sImage.SetAutoScale(true);
                                        Div imgDiv = new Div();
                                        imgDiv.SetHeight(imgRect.GetHeight());
                                        imgDiv.SetWidth(imgRect.GetHeight());
                                        imgDiv.SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE);
                                        imgDiv.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                                        imgDiv.Add(sImage);
                                        dataLayoutCanvas.Add(imgDiv);
                                    }

                                }
                                catch { throw; }

                                //appearance.SetSignatureGraphic(image);

                                //appearance.SetRenderingMode(PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION);

                            }



                        }

                        signer.SetFieldName($"sig-{Guid.NewGuid()}");

                        if (options.AddTimestamp) {
                            if (options.TimestampOptions.Login != null)
                                tsa = new TSAClientAbcBouncyCastle(options.TimestampOptions.TsaUrl, options.TimestampOptions.Login, options.TimestampOptions.Password, 0, HashAlgorithmName.SHA256.Name);
                            else if (options.TimestampOptions.Certificate != null)
                                tsa = new TSAClientAbcBouncyCastle(options.TimestampOptions.TsaUrl, options.TimestampOptions.Certificate, HashAlgorithmName.SHA256.Name);
                            else
                                tsa = new TSAClientAbcBouncyCastle(options.TimestampOptions.TsaUrl);

                            if (tsa != null) {
                                tsa.SetTSAReqPolicy(options.TimestampOptions.TsaPolicy);
                                tsa.SetCertReq(true);
                                //BigInteger nonce = BigInteger.ValueOf(DateTime.Now.Ticks);
                                //tsa.SetNonce(nonce.ToByteArray());
                            }
                        }
                        var signature = new X509Certificate2Signature(options.Certificate, HashAlgorithmName.SHA256.Name);
                        signer.SignDetached(signature, GetChain(options.Certificate), null, null, tsa, 0, PdfSigner.CryptoStandard.CADES);
                    }
                }

                if (options.AddTimestamp) {
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
                else {
                    File.Copy(temp, outputFilePath, true);
                }

                try {
                    if (File.Exists(temp)) { File.Delete(temp); }
                }
                catch { }
            }
        }
        [Obsolete]
        private void signPdfFile(
                string sourceFilePath,
                X509Certificate2 cert,
                CommitmentTypeId reason,
                string location,
                DateTime signDate,
                bool addTimeStamp,
                string timeStampServerUrl,
                string tsaPolicy,
                X509Certificate2 tsaCert,
                string tsaLogin, 
                string tsaPassword,
                byte[] apperancePngImage,
                PdfSignatureLocation apperancePngImageLocation,
                float apperanceLocationX,
                float apperanceLocationY,
                float apperanceWidth,
                float apperanceHeight,
                float margin,
                string outputFilePath,
                bool addSignatureAppearance,
                bool imageAsBackground = true,
                bool allowMultipleSignatures = false) {

            //if (apperancePngImage == null) { apperancePngImage = Properties.Resources.nes_stamp; }

            var temp = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
            TSAClientAbcBouncyCastle tsa = null;

            int signatureCount = SignatureCount(sourceFilePath);

            if (signatureCount == 0 || allowMultipleSignatures) {
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

                        StampingProperties properties = new StampingProperties();
                        if (signatureCount > 0 && allowMultipleSignatures)
                            properties.UseAppendMode();

                        var signer = new PdfSigner(reader, fs, properties);

                        if (signDate > DateTime.MinValue)
                            signer.SetSignDate(signDate);

                        if (addSignatureAppearance) {
                            var rect = GetApperanceImageRect(apperancePngImageLocation, pageWidth, pageHeight,
                                apperanceLocationX, apperanceLocationY, apperanceWidth, apperanceHeight, margin, signatureCount);
                            var sb = new StringBuilder();
                            sb.AppendLine("Podpisany przez:");
                            //var signerName = cert.FriendlyName;
                            //if (signerName == null || signerName == string.Empty)
                            var signerName = cert.GetNameInfo(X509NameType.SimpleName, false);

                            sb.AppendLine(signerName);
                            sb.AppendLine(DateTimeHelper.FormatDateForSignature(signDate));

                            var appearance = signer.GetSignatureAppearance();
                            appearance.SetReason(reason.GetDescription())
                                .SetReasonCaption("Rodzaj: ");

                            if (location != null)
                                appearance.SetLocation(location)
                                    .SetLocationCaption("Miejsce: ");


                            appearance
                                .SetLayer2FontSize(6f)
                                .SetPageRect(rect)
                                .SetPageNumber(1);
                            string signText = sb.ToString();
                            appearance.SetLayer2Text(signText);

                            appearance.SetRenderingMode(PdfSignatureAppearance.RenderingMode.DESCRIPTION);
                            if (apperancePngImage != null) {
                                iText.IO.Image.ImageData image = iText.IO.Image.ImageDataFactory.CreatePng(apperancePngImage);
                                if (imageAsBackground)
                                    appearance.SetImage(image);
                                else {
                                    //custom widget
                                    var pdfDoc = signer.GetDocument();
                                    var layer2 = appearance.GetLayer2();
                                    var canvas = new PdfCanvas(layer2, pdfDoc);

                                    float MARGIN = 1;
                                    PdfFont font = PdfFontFactory.CreateFont();

                                    float width = rect.GetWidth();
                                    float height = rect.GetHeight();
                                    //square image left, signature right
                                    iText.Kernel.Geom.Rectangle imgRect = new iText.Kernel.Geom.Rectangle(MARGIN, MARGIN, rect.GetHeight() - MARGIN *2, rect.GetHeight() - MARGIN*2);
                                    iText.Kernel.Geom.Rectangle signatureRect = new iText.Kernel.Geom.Rectangle(rect.GetHeight() + MARGIN, MARGIN, width - rect.GetHeight(), rect.GetHeight() - 2 * MARGIN);

                                    //image on top, signature bottom
                                    //iText.Kernel.Geom.Rectangle dataRect = new iText.Kernel.Geom.Rectangle(MARGIN, rect.GetHeight() / 2 + MARGIN, rect.GetWidth() - 2 * MARGIN, rect.GetHeight() / 2 - MARGIN);
                                    //iText.Kernel.Geom.Rectangle signatureRect = new iText.Kernel.Geom.Rectangle(MARGIN, MARGIN, rect.GetWidth() - 2 * MARGIN, rect.GetHeight() / 2 - MARGIN);

                                    try {
                                        //text element
                                        Canvas signLayoutCanvas = new Canvas(canvas, signatureRect);
                                        Paragraph para = new Paragraph(signText)
                                            //.SetMultipliedLeading(.9f)
                                            .SetFontSize(6)
                                            .SetMargin(0);
                                        var div = new Div();
                                        div.SetHeight(signatureRect.GetHeight());
                                        div.SetWidth(signatureRect.GetWidth());
                                        div.SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE);
                                        div.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                                        div.Add(para);
                                        signLayoutCanvas.Add(div);

                                        //image element
                                        Canvas dataLayoutCanvas = new Canvas(canvas, imgRect);
                                        Image sImage = new Image(image);
                                        sImage.SetAutoScale(true);
                                        Div imgDiv = new Div();
                                        imgDiv.SetHeight(imgRect.GetHeight());
                                        imgDiv.SetWidth(imgRect.GetHeight());
                                        imgDiv.SetVerticalAlignment(iText.Layout.Properties.VerticalAlignment.MIDDLE);
                                        imgDiv.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                                        imgDiv.Add(sImage);
                                        dataLayoutCanvas.Add(imgDiv);

                                    }
                                    catch { throw; }

                                    //appearance.SetSignatureGraphic(image);

                                    //appearance.SetRenderingMode(PdfSignatureAppearance.RenderingMode.GRAPHIC_AND_DESCRIPTION);

                                }
                            }


                        }

                        signer.SetFieldName($"sig-{Guid.NewGuid()}");

                        if (addTimeStamp) {
                            if (tsaLogin != null)
                                tsa = new TSAClientAbcBouncyCastle(timeStampServerUrl, tsaLogin, tsaPassword, 0, HashAlgorithmName.SHA256.Name);
                            else if (tsaCert != null)
                                tsa = new TSAClientAbcBouncyCastle(timeStampServerUrl, tsaCert, HashAlgorithmName.SHA256.Name);
                            else
                                tsa = new TSAClientAbcBouncyCastle(timeStampServerUrl);

                            if (tsa != null) {
                                tsa.SetTSAReqPolicy(tsaPolicy);
                                tsa.SetCertReq(true);
                                //BigInteger nonce = BigInteger.ValueOf(DateTime.Now.Ticks);
                                //tsa.SetNonce(nonce.ToByteArray());
                            }
                        }
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
                } else {
                    File.Copy(temp, outputFilePath, true);
                }

                try {
                    if (File.Exists(temp)) { File.Delete(temp); }
                }
                catch { }
            }
        }
        private void SignPdfItem(
            ItemBase item,
            X509Certificate2 cert,
            bool addTimeStamp,
            CommitmentTypeId reason = CommitmentTypeId.ProofOfApproval,
            string location = null,
            string timeStampServerUrl = "http://time.certum.pl") {

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
                            .SetReason(reason.GetDescription())
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
                    item.Init(File.ReadAllBytes(output),out var exception);
                }
                else if (File.Exists(temp)) {
                    item.Init(File.ReadAllBytes(temp), out var exception);
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
                if (certificate.PrivateKey is RSACryptoServiceProvider || certificate.PrivateKey is RSA)
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
                else if(certificate.PrivateKey is RSA) {
                    RSA rsa = (RSA)certificate.PrivateKey;
                    var han = new HashAlgorithmName(hashAlgorithm);
                    return rsa.SignData(message, han, RSASignaturePadding.Pkcs1);
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
            return SignatureCount(filePath) > 0;
        }

        private int SignatureCount(string filePath) {
            using (PdfReader reader = new PdfReader(filePath)) {
                using (var doc = new PdfDocument(reader)) {
                    if (iText.Forms.PdfAcroForm.GetAcroForm(doc, false) != null) {
                        var names = new SignatureUtil(doc).GetSignatureNames();
                        if (names != null)
                            return names.Count;
                    }
                }
            }
            return 0;
        }
        private void SignXmlItem(ItemBase item,
                         XadesManager xadesManager,
                         X509Certificate2 cert,
                         SignatureProductionPlace productionPlace = null,
                         SignerRole signerRole = null,
                         DateTime? signDate = null,
                         XadesFormat xadesFormat = XadesFormat.XadesBes,
                         string timeStampServerUrl = "http://time.certum.pl",
                         CommitmentTypeId commitmentTypeId = CommitmentTypeId.ProofOfApproval) {
            var ms = new MemoryStream((item as DocumentFile).FileData);
            Xades.Upgraders.SignatureFormat? upgradeFormat = null;
            if (xadesFormat == XadesFormat.XadesT) {
                upgradeFormat = Xades.Upgraders.SignatureFormat.XAdES_T;
            }
            else if (xadesFormat == XadesFormat.XadesXL) {
                upgradeFormat = Xades.Upgraders.SignatureFormat.XAdES_XL;
            }

            var result = xadesManager.AppendSignatureToXmlFile(ms, cert, productionPlace, signerRole, upgradeFormat, signDate, timeStampServerUrl, commitmentTypeId);
            if (result != null) {
                using (var msOutput = new MemoryStream()) {
                    result.Save(msOutput);
                    item.Init(msOutput.ToArray(), out var exception);
                }
            }
        }

        private void SignOtherItem(ItemBase item,
                     XadesManager xadesManager,
                     X509Certificate2 cert,
                     SignatureProductionPlace productionPlace = null,
                     SignerRole signerRole = null,
                     bool addTimeStamp = false,
                     string timeStampServerUrl = "http://time.certum.pl",
                     CommitmentTypeId commitmentTypeId = CommitmentTypeId.ProofOfApproval) {
            var ms = new MemoryStream((item as DocumentFile).FileData);
            Xades.Upgraders.SignatureFormat? xadesFormat = addTimeStamp ? Xades.Upgraders.SignatureFormat.XAdES_T : (Xades.Upgraders.SignatureFormat?)null;
            var result = xadesManager.CreateEnvelopingSignature(ms, cert, productionPlace, signerRole, null, xadesFormat, timeStampServerUrl, commitmentTypeId);
            if (result != null) {
                using (var msOutput = new MemoryStream()) {
                    result.Save(msOutput);
                    item.Init(msOutput.ToArray(), out var exception);
                }
            }
        }

        private void SignDetachedOtherItem(PackageManager mgr, ItemBase item,
                    XadesManager xadesManager,
                    X509Certificate2 cert,
                    SignatureProductionPlace productionPlace = null,
                    SignerRole signerRole = null,
                    bool addTimeStamp = false,
                    string timeStampServerUrl = "http://time.certum.pl",
                    CommitmentTypeId commitmentTypeId = CommitmentTypeId.ProofOfApproval) {
            var tempFilePath = Path.Combine(Path.GetTempPath(), (item as DocumentFile).FileName);
            File.WriteAllBytes(tempFilePath, (item as DocumentFile).FileData);
            Xades.Upgraders.SignatureFormat? xadesFormat = addTimeStamp ? Xades.Upgraders.SignatureFormat.XAdES_T : (Xades.Upgraders.SignatureFormat?)null;
            var result = xadesManager.CreateDetachedSignature(tempFilePath, cert, productionPlace, signerRole, xadesFormat, timeStampServerUrl, commitmentTypeId);
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
                            MetadataFile xadesMetadataFile = null;
                            using (var converter = new Converters.XmlConverter()) {
                                var xadesMetadataFileDocumentXml = converter.GetXml(metadataFile.Document);
                                IDocument parsedDocument = converter.ParseXml(xadesMetadataFileDocumentXml);
                                Document xadesMetadataFileDocument = parsedDocument as Document;
                                
                                if (xadesMetadataFileDocument.IsNotNull()) {
                                    xadesMetadataFileDocument.Description = $"Digital signature of file {(item as DocumentFile).FileName}.";
                                    xadesMetadataFile = new MetadataFile() {
                                        FileName = $"{xadesFile.FileName}.xml",
                                        Document = xadesMetadataFileDocument
                                    };
                                } else {
                                    Document17 xadesMetadataFileDocument17 = parsedDocument as Document17;
                                    if (xadesMetadataFileDocument17.IsNotNull()) {
                                        xadesMetadataFileDocument17.Description = $"Digital signature of file {(item as DocumentFile).FileName}.";
                                        xadesMetadataFile = new MetadataFile() {
                                            FileName = $"{xadesFile.FileName}.xml",
                                            Document = xadesMetadataFileDocument17
                                        };
                                    }
                                }
                                

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

        private SignatureInfo[] GetZipxInfos(byte[] fileData, string internalPath = null) {
            const string XML = "akt.xml";
            using (var zip = Ionic.Zip.ZipFile.Read(new MemoryStream(fileData))) {
                if (zip.ContainsEntry(XML)) {
                    // musimy rozpakować archiwum w celu weryfikacji 
                    // pliku akt.xml
                    var temp = Path.Combine(Path.GetTempPath(), $"ABCPRO.NES\\{Path.GetFileName(internalPath)}");
                    if (!Directory.Exists(temp)) { Directory.CreateDirectory(temp); }
                    zip.ExtractAll(temp);
                    try {
                        var xml = XElement.Load(Path.Combine(temp, XML));
                        return GetXadesSignatureInfos(xml, internalPath);
                    }
                    catch { }
                    finally {
                        try { Directory.Delete(temp, true); } catch { }
                    }

                }
            }
            return default;
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
                                var subject = new SubjectNameInfo(cert.Subject);
                                var issuer = new SubjectNameInfo(cert.Issuer);
                                var item = new SignatureInfo() {
                                    CreateDate = pkcs7.GetSignDate(),
                                    Certificate = cert,
                                    Author = subject.ContainsKey("CN") ? subject["CN"] : String.Empty,
                                    Publisher = issuer.ContainsKey("CN") ? issuer["CN"] : String.Empty,
                                    SignatureType = SignatureType.Pades,
                                    SignatureNumber = name,
                                    FileName = fileName,
                                    CommitmentTypeIndication = pkcs7.GetReason(),
                                    Organization = subject.ContainsKey("O") ? subject["O"] : String.Empty,
                                    ClaimedRole = ""
                                };
                                if (item.Author != null) { item.Author = item.Author.Replace("\"", String.Empty); }
                                if (item.Publisher != null) { item.Publisher = item.Publisher.Replace("\"", String.Empty); }
                                if (item.CommitmentTypeIndication == null) { item.CommitmentTypeIndication = "Formalne zatwierdzenie (Proof of approval)"; }

                                list.Add(item);
                            }
                        }
                    }
                    return list.ToArray();
                }
            }
            return default;
        }
        private SignatureVerifyInfo[] VerifyPadesSignatures(byte[] fileData, string internalPath) {
            var list = new List<SignatureVerifyInfo>();

            using (var pdfDoc = new PdfDocument(new PdfReader(new MemoryStream(fileData)))) {
                var signUtil = new SignatureUtil(pdfDoc);
                var names = signUtil.GetSignatureNames();
                if (names != null && names.Count > 0) {
                    foreach (var name in names) {
                        var pkcs7 = signUtil.ReadSignatureData(name);
                        if (pkcs7 != null) {

                            var certValidationInfo = ValidateCert(pkcs7.GetSigningCertificate());
                            var message = certValidationInfo.ErrorMessage;
                            var isValid = pkcs7.VerifySignatureIntegrityAndAuthenticity();


                            if (String.IsNullOrEmpty(message)) {
                                if (!isValid && certValidationInfo.IsValid) {
                                    message = "Weryfikacja sygnatury podpisu zakończona niepowodzeniem.";
                                }
                                else if (isValid && !certValidationInfo.IsValid) {
                                    message = "Weryfikacja certyfikatu osoby podpisującej zakończona niepowodzeniem.";
                                }
                                else if (!isValid && !certValidationInfo.IsValid) {
                                    message = "Weryfikacja podpisu i certyfikatu zakończona niepowodzeniem.";
                                }
                                else {
                                    message = "Weryfikacja sygnatury podpisu i certyfikatu przebiegła pomyślnie.";
                                }
                            }
                            else if (!String.IsNullOrEmpty(message) && !isValid) {
                                message = $"Weryfikacja sygnatury podpisu zakończona niepowodzeniem. {message}";
                            }

                            list.Add(new SignatureVerifyInfo() {
                                FileName = internalPath,
                                SignatureName = name,
                                IsValid = isValid,
                                CertValidationInfo = certValidationInfo,
                                Message = message
                            });
                        }
                    }
                }
            }

            return list.ToArray();
        }
        private SignatureVerifyInfo[] VerifyZipxSignatures(byte[] fileData, string internalPath) {
            var list = new List<SignatureVerifyInfo>();
            const string XML = "akt.xml";
            using (var zip = Ionic.Zip.ZipFile.Read(new MemoryStream(fileData))) {
                if (zip.ContainsEntry(XML)) {
                    // musimy rozpakować archiwum w celu weryfikacji 
                    // pliku akt.xml
                    var temp = Path.Combine(Path.GetTempPath(), $"ABCPRO.NES\\{Path.GetFileName(internalPath)}");
                    if (!Directory.Exists(temp)) { Directory.CreateDirectory(temp); }
                    zip.ExtractAll(temp);
                    try {
                        var result = VerifyXadesSignatures(Path.Combine(temp, XML), internalPath);
                        if (result != null) { list.AddRange(result); }
                    }
                    catch { }
                    finally {
                        try { Directory.Delete(temp, true); } catch { }
                    }

                }
            }
            return list.ToArray();
        }
        private SignatureVerifyInfo[] VerifyXadesSignatures(string filePath, string internalPath) {
            var list = new List<SignatureVerifyInfo>();

            using (var mgr = new XadesManager()) {
                var result = mgr.ValidateSignature(filePath);
                if (result != null) {
                    list.Add(new SignatureVerifyInfo() {
                        FileName = internalPath,
                        IsValid = result.IsValid,
                        SignatureName = result.SignatureName,
                        CertValidationInfo = result.CertValidationInfo,
                        Message = result.Message
                    });
                }
            }

            return list.ToArray();
        }
        private string GetCommitmentTypeIndication(XElement e) {
            const string xmlCTIProofOfReceipt = "http://uri.etsi.org/01903/v1.2.2#ProofOfReceipt";
            const string xmlCTIProofOfDelivery = "http://uri.etsi.org/01903/v1.2.2#ProofOfDelivery";
            const string xmlCTIProofOfSender = "http://uri.etsi.org/01903/v1.2.2#ProofOfSender";
            const string xmlCTIProofOfApproval = "http://uri.etsi.org/01903/v1.2.2#ProofOfApproval";
            const string xmlCTIProofOfCreation = "http://uri.etsi.org/01903/v1.2.2#ProofOfCreation";
            const string xmlCTIProofOfOrigin = "http://uri.etsi.org/01903/v1.2.2#ProofOfOrigin";
            const string EDAP_NAMESPACE = "http://www.crd.gov.pl/xml/schematy/edap/2010/01/02";
            const string EDAP_NAMESPACE_INITIALS = EDAP_NAMESPACE + "#Initials";

            var s = "Brak";
            var commitmentTypeIndication = e.Descendants().Where(x => x.Parent != null &&
                    x.Parent.Name.LocalName == "CommitmentTypeId" &&
                    x.Name.LocalName == "Identifier").FirstOrDefault();
            if (commitmentTypeIndication != null) {
                switch (commitmentTypeIndication.Value) {
                    case xmlCTIProofOfApproval: { s = "Formalne zatwierdzenie (Proof of approval)"; break; }
                    case xmlCTIProofOfCreation: { s = "Potwierdzenie utworzenia (Proof of creation)"; break; }
                    case xmlCTIProofOfDelivery: { s = "Dowód dostawy (Proof of delivery)"; break; }
                    case xmlCTIProofOfOrigin: { s = "Dowód pochodzenia (Proof of origin)"; break; }
                    case xmlCTIProofOfReceipt: { s = "Potwierdzenie odbioru (Proof of receipt)"; break; }
                    case xmlCTIProofOfSender: { s = "Dowód nadawcy (Proof of sender)"; break; }
                    case EDAP_NAMESPACE_INITIALS: { s = "Parafka"; break; }
                    default: { s = commitmentTypeIndication.Value; break; }
                }

            }
            return s;
        }
        private SignatureInfo GetSignatureInfo(XElement e) {
            if (e != null) {
                var result = new SignatureInfo() {
                    SignatureType = SignatureType.Xades,
                    SignatureNumber = e.Attribute("Id").Value(),
                    CommitmentTypeIndication = GetCommitmentTypeIndication(e),
                    Author = String.Empty,
                    ClaimedRole = String.Empty,
                    Organization = String.Empty,
                    Publisher = String.Empty,
                };

                var xSubjectName = e.Descendants().Where(x => x.Name.LocalName == "X509SubjectName").FirstOrDefault();
                var keyName = e.Descendants().Where(x => x.Name.LocalName == "KeyName").FirstOrDefault();
                if (keyName != null) {
                    result.Author = keyName.Value;
                }
                else if (xSubjectName != null) {
                    var subjectName = new SubjectNameInfo(xSubjectName.Value);
                    if (subjectName.Count > 0 && subjectName.ContainsKey("CN")) {
                        result.Author = subjectName["CN"];
                    }
                }

                if (xSubjectName != null) {
                    var subjectName = new SubjectNameInfo(xSubjectName.Value);
                    if (subjectName.Count > 0 && subjectName.ContainsKey("O")) {
                        result.Organization = subjectName["O"];
                    }
                    else if (subjectName.Count > 0 && subjectName.ContainsKey("OU")) {
                        result.Organization = subjectName["OU"];
                    }
                }

                var signingTime = e.Descendants().Where(x => x.Name.LocalName == "SigningTime").FirstOrDefault();
                if (signingTime != null) {
                    result.CreateDate = Convert.ToDateTime(signingTime.Value);
                }

                var claimedRole = e.Descendants().Where(x => x.Name.LocalName == "ClaimedRole").FirstOrDefault();
                if (claimedRole != null) {
                    result.ClaimedRole = claimedRole.Value;
                }

                var xIssuerName = e.Descendants().Where(x => x.Name.LocalName == "X509IssuerName").FirstOrDefault();
                if (xIssuerName != null) {
                    var issuerName = new SubjectNameInfo(xIssuerName.Value);
                    if (issuerName.Count > 0 && issuerName.ContainsKey("CN")) {
                        result.Publisher = issuerName["CN"];
                    }
                }

                var xmlCert = e.Descendants().Where(x => x.Name.LocalName == "X509Certificate").FirstOrDefault();
                try {
                    var data = Convert.FromBase64String(xmlCert.Value);
                    var cert = new X509Certificate2(data);
                    result.Certificate = cert;
                    if (result.Author.IsNullOrEmpty()) {
                        var subjectName = new SubjectNameInfo(cert.Subject);
                        if (subjectName.Count > 0 && subjectName.ContainsKey("CN")) {
                            result.Author = subjectName["CN"];
                        }
                    }
                    if (result.Publisher.IsNullOrEmpty()) {
                        var issuerName = new SubjectNameInfo(cert.Issuer);
                        if (issuerName.Count > 0 && issuerName.ContainsKey("CN")) {
                            result.Publisher = issuerName["CN"];
                        }
                    }
                    if (result.Organization.IsNullOrEmpty()) {
                        var subjectName = new SubjectNameInfo(cert.Subject);
                        if (subjectName.Count > 0 && subjectName.ContainsKey("O")) {
                            result.Organization = subjectName["O"];
                        }
                        else if (subjectName.Count > 0 && subjectName.ContainsKey("OU")) {
                            result.Organization = subjectName["OU"];
                        }
                    }
                }
                catch { }

                if (result.Author.IsNotNullOrEmpty()) { result.Author = result.Author.Replace("\"", String.Empty); }
                if (result.Publisher.IsNotNullOrEmpty()) { result.Publisher = result.Publisher.Replace("\"", String.Empty); }
                if (result.CommitmentTypeIndication.IsNullOrEmpty()) { result.CommitmentTypeIndication = "Formalne zatwierdzenie (Proof of approval)"; }

                return result;
            }
            return default;
        }

        private CertificateValidationInfo ValidateCert(Org.BouncyCastle.X509.X509Certificate e) {
            if (e != null) {
                var cert = new X509Certificate2(e.GetEncoded());
                return ValidateCert(cert);
            }
            return default;
        }
        private CertificateValidationInfo ValidateCert(X509Certificate2 e) {
            if (e != null) {
                var ch = new X509Chain();
                ch.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                if (ch.Build(e)) { return new CertificateValidationInfo(true); }
                else {

                    foreach (X509ChainStatus chainStatus in ch.ChainStatus) {
                        var userName = GetSubjectCommonName(e);
                        return new CertificateValidationInfo(chainStatus, userName);
                    }
                }
            }
            return default;
        }
        private string GetSubjectCommonName(X509Certificate2 e) {
            var sc = e.Subject.Split(',');
            if (sc.Length > 0) {
                foreach (string t in sc) {
                    string[] sci = t.Split('=');
                    if (sci.Length > 1) {
                        if (sci[0].Trim() == "CN") {
                            return sci[1].Trim();
                        }
                    }
                }
            }
            return String.Empty;
        }

        private SignAndVerifyInfo GetZipSignAndVerifyInfo(byte[] fileData, string temp, string internalPath) {
            const string XML = "akt.xml";
            var result = new SignAndVerifyInfo();
            using (var zip = Ionic.Zip.ZipFile.Read(new MemoryStream(fileData))) {
                if (zip.ContainsEntry(XML)) {
                    // musimy rozpakować archiwum w celu weryfikacji 
                    // pliku akt.xml
                    zip.ExtractAll(temp);
                    try {
                        var xml = XElement.Load(Path.Combine(temp, XML));
                        result.SignInfo = GetXadesSignatureInfos(xml, internalPath);
                        result.VerifyInfo = VerifyXadesSignatures(Path.Combine(temp, XML), internalPath);
                    }
                    catch { }
                    finally {
                        try { Directory.Delete(temp, true); } catch { }
                    }

                }
            }
            return result;
        }
        private SignAndVerifyInfo GetXmlSignAndVerifyInfo(PackageManager mgr, DocumentFile item, string temp, string internalPath) {
            var result = new SignAndVerifyInfo();
            var sigInfo = GetXadesSignatureInfos(item.FileData.ToXElement(), internalPath);
            if (sigInfo != null && sigInfo.Length > 0) {
                result.SignInfo = sigInfo;
            }

            if (internalPath.EndsWith(".xml")) {
                File.WriteAllBytes(Path.Combine(temp, item.FileName), item.FileData);
                var verifyInfo = VerifyXadesSignatures(Path.Combine(temp, item.FileName), internalPath);
                if (verifyInfo != null && verifyInfo.Length > 0) {
                    result.VerifyInfo = verifyInfo;
                }
            }
            else if (internalPath.EndsWith(".xades")) {
                var _internalPath = internalPath.Replace(".xades", "");
                var _item = mgr.GetItemByFilePath(_internalPath) as DocumentFile;
                if (_item != null) {
                    File.WriteAllBytes(Path.Combine(temp, item.FileName), item.FileData);
                    File.WriteAllBytes(Path.Combine(temp, _item.FileName), _item.FileData);

                    var verifyInfo = VerifyXadesSignatures(Path.Combine(temp, item.FileName), internalPath);
                    if (verifyInfo != null && verifyInfo.Length > 0) {
                        result.VerifyInfo = verifyInfo;
                    }
                }
            }
            return result;
        }
        private SignAndVerifyInfo GetPadesSignAndVerifyInfo(byte[] fileData, string internalPath) {
            using (var pdfDoc = new PdfDocument(new PdfReader(new MemoryStream(fileData)))) {
                var signUtil = new SignatureUtil(pdfDoc);
                var names = signUtil.GetSignatureNames();
                if (names != null && names.Count > 0) {
                    var verifyInfo = new List<SignatureVerifyInfo>();
                    var sigInfo = new List<SignatureInfo>();
                    foreach (var name in names) {
                        var pkcs7 = signUtil.ReadSignatureData(name);
                        if (pkcs7 != null) {
                            var cert = new X509Certificate2(pkcs7.GetSigningCertificate().GetEncoded());
                            if (cert != null) {
                                var subject = new SubjectNameInfo(cert.Subject);
                                var issuer = new SubjectNameInfo(cert.Issuer);
                                var item = new SignatureInfo() {
                                    CreateDate = pkcs7.GetSignDate(),
                                    Certificate = cert,
                                    Author = subject.ContainsKey("CN") ? subject["CN"] : String.Empty,
                                    Publisher = issuer.ContainsKey("CN") ? issuer["CN"] : String.Empty,
                                    SignatureType = SignatureType.Pades,
                                    SignatureNumber = name,
                                    FileName = internalPath,
                                    CommitmentTypeIndication = pkcs7.GetReason(),
                                    Organization = subject.ContainsKey("O") ? subject["O"] : String.Empty,
                                    ClaimedRole = ""
                                };
                                if (item.Author != null) { item.Author = item.Author.Replace("\"", String.Empty); }
                                if (item.Publisher != null) { item.Publisher = item.Publisher.Replace("\"", String.Empty); }
                                if (item.CommitmentTypeIndication == null) { item.CommitmentTypeIndication = "Formalne zatwierdzenie (Proof of approval)"; }

                                sigInfo.Add(item);

                                var certValidationInfo = ValidateCert(cert);
                                var message = certValidationInfo.ErrorMessage;
                                var isValid = pkcs7.VerifySignatureIntegrityAndAuthenticity();

                                if (String.IsNullOrEmpty(message)) {
                                    if (!isValid && certValidationInfo.IsValid) {
                                        message = "Weryfikacja sygnatury podpisu zakończona niepowodzeniem";
                                    }
                                    else if (isValid && !certValidationInfo.IsValid) {
                                        message = "Weryfikacja certyfikatu osoby podpisującej zakończona niepowodzeniem";
                                    }
                                    else if (!isValid && !certValidationInfo.IsValid) {
                                        message = "Weryfikacja podpisu i certyfikatu zakończona niepowodzeniem";
                                    }
                                    else {
                                        message = "Weryfikacja sygnatury podpisu i certyfikatu przebiegła pomyślnie";
                                    }
                                }
                                else if (!certValidationInfo.IsValid && !isValid) {
                                    message = $"Weryfikacja sygnatury podpisu zakończona niepowodzeniem. {message}";
                                }

                                verifyInfo.Add(new SignatureVerifyInfo() {
                                    FileName = internalPath,
                                    SignatureName = name,
                                    IsValid = isValid,
                                    CertValidationInfo = certValidationInfo,
                                    Message = message
                                });
                            }
                        }
                    }

                    return new SignAndVerifyInfo() {
                        SignInfo = sigInfo.ToArray(),
                        VerifyInfo = verifyInfo.ToArray()
                    };
                }
            }
            return default;
        }

        private void VerifySignatures(List<SignatureVerifyInfo> list, PackageManager mgr, DocumentFile item, string internalPath) {
            try {
                var temp = Path.Combine(Path.GetTempPath(), "ABCPRO.NES");
                if (!Directory.Exists(temp)) { Directory.CreateDirectory(temp); }

                if (internalPath.EndsWith(".xml")) {
                    try {
                        File.WriteAllBytes(Path.Combine(temp, item.FileName), item.FileData);

                        var result = VerifyXadesSignatures(Path.Combine(temp, item.FileName), internalPath);
                        if (result != null && result.Length > 0) { list.AddRange(result); }
                    }
                    finally {
                        var _files = Directory.GetFiles(temp);
                        foreach (var _file in _files) {
                            try { File.Delete(_file); } catch { }
                        }
                    }
                }
                else if (internalPath.EndsWith(".pdf")) {
                    var result = VerifyPadesSignatures(item.FileData, internalPath);
                    if (result != null && result.Length > 0) { list.AddRange(result); }
                }
                else if (internalPath.EndsWith(".zip")) {
                    var result = VerifyZipxSignatures(item.FileData, internalPath);
                    if (result != null && result.Length > 0) { list.AddRange(result); }
                }
                else if (internalPath.EndsWith(".xades")) {
                    var _internalPath = internalPath.Replace(".xades", "");
                    var _item = mgr.GetItemByFilePath(_internalPath) as DocumentFile;
                    if (_item != null) {
                        try {
                            File.WriteAllBytes(Path.Combine(temp, item.FileName), item.FileData);
                            File.WriteAllBytes(Path.Combine(temp, _item.FileName), _item.FileData);

                            var result = VerifyXadesSignatures(Path.Combine(temp, item.FileName), internalPath);
                            if (result != null && result.Length > 0) { list.AddRange(result); }
                        }
                        finally {
                            try { Directory.Delete(temp, true); } catch { }
                            //var _files = Directory.GetFiles(temp);
                            //foreach (var _file in _files) {
                            //    try { File.Delete(_file); } catch { }
                            //}
                        }
                    }
                }
            }
            catch {
                list.Add(new SignatureVerifyInfo() {
                    FileName = internalPath,
                    IsValid = false,
                    SignatureName = String.Empty
                });
            }
        }
    }
}
