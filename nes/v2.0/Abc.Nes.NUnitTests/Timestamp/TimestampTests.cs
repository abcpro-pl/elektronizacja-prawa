using Abc.Nes.ArchivalPackage.Cryptography;
using Abc.Nes.Common;
using Abc.Nes.Common.Models;
using Abc.Nes.Xades;
using Abc.Nes.Xades.Signature.Parameters;
using Abc.Nes.Xades.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Abc.Nes.NUnitTests {
    public class TimestampTests {

        //test files
        const string PDF_FILE = "testFile.pdf";
        const string XML_FILE = "testFile.xml";
        const string LOGO_FILE = "TestLogo1.png";

        //test certificates names
        private const string TEST_CERT = "localhost";
        private const string KIR_CERT = "Marcin I";
        private const string SIGILLUM_CERT = "Beata W";

        //timestamp urls
        private const string TSA_KIR = "http://www.ts.kir.com.pl/HttpTspServer";
        private const string TSA_CERTUM_BASIC = "https://qts.certum.pl/default/basic";
        private const string TSA_CERTUM = "http://time.certum.pl";
        private const string TSA_SIGILLUM = "http://tsa.sigillum.pl";

        //auth data file names
        private const string CERTUM_TSA_AUTH_FILE = "certum.tsa.pass.txt";

        //tsa policies
        private const string TSA_POLICY_SIGILLUM = "1.2.616.1.113725.0.0.5";

        //pdf signature params
        int WIDTH = 150;
        int HEIGHT = 40;


        string certum_tsa_login;
        string certum_tsa_password;
        string pwd;
        string testFilesDirPath;
        string signedPath;
        string testName;
        byte[] img;
        DateTime signDate;
        PdfSignatureOptions pdfSignOptions;
        SignatureOptions xmlSignOptions;

        [OneTimeSetUp]
        public void Init() {
            pwd = Directory.GetCurrentDirectory();
            testFilesDirPath = Path.Combine(pwd, @"../../../../sample/");
            signedPath = Path.Combine(testFilesDirPath, "TimestampTests");

            var certumFilePath = Path.Combine(testFilesDirPath, CERTUM_TSA_AUTH_FILE);

            var authTxt = File.ReadAllLines(certumFilePath);
            certum_tsa_login = authTxt[0];
            certum_tsa_password = authTxt[1];

            //if (Directory.Exists(signedPath))
            //    Directory.Delete(signedPath, true);
            if (!Directory.Exists(signedPath))
                Directory.CreateDirectory(signedPath);

            img = GetImage(LOGO_FILE);

            
        }
        [SetUp]
        public void Setup() {
            signDate = DateTime.Now;

            xmlSignOptions = new SignatureOptions {
                SignDate = signDate,
                Location = "Test location",
                Reason = CommitmentTypeId.ProofOfOrigin,

                TimestampOptions = null
            };

            pdfSignOptions = new PdfSignatureOptions {
                SignDate = signDate,
                Reason = CommitmentTypeId.ProofOfOrigin,
                Location = "Test location",

                AllowMultipleSignatures = false,

                AddVisibleSignature = false,
                ImageAsBackground = false,
                SignatureImage = img,
                Height = HEIGHT,
                Width = WIDTH,
                Margin = 10,
                SignatureLocation = PdfSignatureLocation.TopRight,
                PositionX = 1,
                PositionY = 1,

                TimestampOptions = null
            };
        }

        #region sign pdf test
        [Test]
        public void SignPdf_TestCert_TsCertumBasicAuth() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {
                
                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(TEST_CERT);

                
                pdfSignOptions.Certificate = cert;
                pdfSignOptions.Location = "Wawa";
                pdfSignOptions.AddVisibleSignature = true;
                pdfSignOptions.TimestampOptions = new TimestampOptions {
                    TsaUrl = TSA_CERTUM_BASIC,
                    Login = certum_tsa_login,
                    Password=certum_tsa_password
                };

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                //mgr.SignPdfFile(filePath, cert,
                //                CommitmentTypeId.ProofOfOrigin, "Wawa", signDate,
                //                true, TSA_CERTUM_BASIC, reqPolicy, login, pass, null,
                //                img,
                //                outputFilePath: destPath,
                //                addSignatureApperance: true);
                Assert.IsTrue(File.Exists(destPath));
            }
        }


        [Test]
        public void SignPdf_TestCert_noTS_double() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                var tmpFile = Path.GetTempFileName();
                var tmpPath = Path.Combine(testFilesDirPath, tmpFile);

                X509Certificate2 cert = CertUtil.GetCertByName(TEST_CERT);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.Location = "Location A";
                pdfSignOptions.AddVisibleSignature = true;

                mgr.SignPdfFile(filePath, pdfSignOptions, tmpFile);
                //mgr.SignPdfFile(filePath, cert,
                //                CommitmentTypeId.ProofOfOrigin, "location A", signDate,
                //                false, null, null, null, null, null,
                //                img, PdfSignatureLocation.TopRight, 0, 0, WIDTH, HEIGHT, 0,
                //                tmpFile,
                //                true, false);

                pdfSignOptions.Reason = CommitmentTypeId.ProofOfApproval;
                pdfSignOptions.Location = "Location B";
                pdfSignOptions.SignDate = DateTime.Now;
                pdfSignOptions.Width = WIDTH + 15;
                pdfSignOptions.AllowMultipleSignatures = true;
                mgr.SignPdfFile(tmpFile, pdfSignOptions, destPath);
                //mgr.SignPdfFile(tmpFile, cert,
                //                CommitmentTypeId.ProofOfApproval, "location B", DateTime.Now,
                //                false, null, null, null, null, null,
                //                null, PdfSignatureLocation.TopRight, 0, 0, WIDTH+20, HEIGHT, 0,
                //                destPath,
                //                true, false,
                //                true);

                File.Delete(tmpPath);
                
                Assert.IsTrue(File.Exists(destPath));
            }
        }

        [Test]
        public void SignPdf_TestCert_noTS_background() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(TEST_CERT);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.ImageAsBackground = true;
                pdfSignOptions.AddVisibleSignature = true;

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                //mgr.SignPdfFile(filePath, cert,
                //                CommitmentTypeId.ProofOfOrigin, null, signDate,
                                
                //                apperancePngImage: img,
                //                outputFilePath: destPath,
                //                addSignatureApperance: true);
                Assert.IsTrue(File.Exists(destPath));
            }
        }
        [Test]
        public void SignPdf_TestCert_noTS_graphic() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(TEST_CERT);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.AddVisibleSignature = true;

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                //mgr.SignPdfFile(filePath, cert,
                //                CommitmentTypeId.ProofOfOrigin, null, signDate,
                //                false, null, null, null, null, null,
                //                img, PdfSignatureLocation.TopRight, 0, 0, WIDTH, HEIGHT, 0,
                //                destPath,
                //                true, false
                //               );
                Assert.IsTrue(File.Exists(destPath));
            }
        }

        //[Test]
        //public void SignPdf_TestCert_noTS_customWidget() {
        //    testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

        //    using (var mgr = new PackageSignerManager()) {

        //        PreparePdfPaths(out string filePath, out string destPath);

        //        X509Certificate2 cert = CertUtil.GetCertByName(TEST_CERT);
        //        pdfSignOptions.Certificate = cert;
        //        pdfSignOptions.AddVisibleSignature = true;

        //        mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

        //        //mgr.SignPdfFile(filePath, cert,
        //        //                CommitmentTypeId.ProofOfOrigin, null, signDate,
        //        //                false, null, null, null, null, null,
        //        //                img, PdfSignatureLocation.TopRight, 0, 0, WIDTH, HEIGHT, 0,
        //        //                destPath,
        //        //                true, false
        //        //               );
        //        Assert.IsTrue(File.Exists(destPath));
        //    }
        //}
        [Test]
        public void SignPdf_TestCert_noTS_noLogo() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(TEST_CERT);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.AddVisibleSignature = true;
                pdfSignOptions.SignatureImage = null;

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                //mgr.SignPdfFile(filePath, cert,
                //                CommitmentTypeId.ProofOfOrigin, null, signDate,
                                
                //                outputFilePath: destPath,
                //                addSignatureApperance: true);

                Assert.IsTrue(File.Exists(destPath));
            }
        }

        [Test]
        public void SignPdf_TestCert_TsCertum() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(TEST_CERT);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.AddVisibleSignature = true;
                pdfSignOptions.TimestampOptions = new TimestampOptions {
                    TsaUrl = TSA_CERTUM
                };

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                //mgr.SignPdfFile(filePath, cert,
                //                CommitmentTypeId.ProofOfOrigin, null, signDate,
                //                true, TSA_CERTUM,
                //                apperancePngImage: img,
                //                outputFilePath: destPath,
                //                addSignatureApperance: true);
                Assert.IsTrue(File.Exists(destPath));
            }
        }

        [Test]
        public void SignPdf_TestCert_TsCertum_NoWidget() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(TEST_CERT);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.TimestampOptions = new TimestampOptions { 
                    TsaUrl=TSA_CERTUM
                };

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                //mgr.SignPdfFile(filePath, cert,
                //                CommitmentTypeId.ProofOfOrigin, null, signDate,
                //                true, TSA_CERTUM,
                                
                //                outputFilePath: destPath,
                //                addSignatureApperance: false);
                Assert.IsTrue(File.Exists(destPath));
            }
        }

        [Test]
        public void SignPdf_TestCert_noTS_noWidget() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            using (var mgr = new PackageSignerManager()) {
                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(TEST_CERT);

                pdfSignOptions.Certificate = cert;

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                //mgr.SignPdfFile(filePath, cert,
                //                CommitmentTypeId.ProofOfOrigin, null,signDate,
                                
                //                outputFilePath: destPath,
                //                addSignatureApperance: false);
                Assert.IsTrue(File.Exists(destPath));
            }
        }
        //[Test]
        //public void SignPdf_TestCert_TsCertumBasicAuth_NoWidget() {
        //    testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

        //    using (var mgr = new PackageSignerManager()) {
        //        PreparePdfPaths(out string filePath, out string destPath);

        //        X509Certificate2 cert = CertUtil.GetCertByName(TEST_CERT);

        //        string tsaUrl = CERTUM_TSA_BASIC,
        //            reqPolicy = null,
        //            login = "admin@abcpro.pl",
        //            pass = "WOmehMw5skAAvxoRFKaG";

        //        mgr.SignPdfFile(filePath,
        //                        cert,
        //                        Xades.Signature.Parameters.CommitmentTypeId.ProofOfOrigin,
        //                        null,
        //                        true,
        //                        tsaUrl,
        //                        reqPolicy,
        //                        login, pass,
        //                        null,
        //                        outputFilePath: destPath,
        //                        addSignatureApperance: false);
        //    }
        //}

        //[Test]
        //public void SignPdf_KIRCert_noTs_NoWidget() {
        //    testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
        //    using (var mgr = new PackageSignerManager()) {
        //        PreparePdfPaths(out string filePath, out string destPath);

        //        X509Certificate2 cert = CertUtil.GetCertByName(KIR_CERT);

        //        mgr.SignPdfFile(filePath,
        //                        cert,
        //                        Xades.Signature.Parameters.CommitmentTypeId.ProofOfOrigin,
        //                        null,
        //                        false,
        //                        outputFilePath: destPath,
        //                        addSignatureApperance: false);
        //    }
        //}

        [Test]
        public void SignPdf_KIRCert_SigillumCert_TsKir_TsSigillum() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                var tmpFile = Path.GetTempFileName();
                var tmpPath = Path.Combine(testFilesDirPath, tmpFile);

                X509Certificate2 kirCert = CertUtil.GetCertByName(KIR_CERT),
                    sigillumCert = CertUtil.GetCertByName(SIGILLUM_CERT);

                pdfSignOptions.Certificate = kirCert;
                pdfSignOptions.Reason = CommitmentTypeId.ProofOfOrigin;
                pdfSignOptions.Location = "Location A";
                pdfSignOptions.TimestampOptions = new TimestampOptions {
                    Certificate = kirCert,
                    TsaUrl = TSA_KIR
                };
                pdfSignOptions.SignatureLocation = PdfSignatureLocation.TopLeft;
                pdfSignOptions.AddVisibleSignature = true;

                mgr.SignPdfFile(filePath, pdfSignOptions, tmpFile);
                //mgr.SignPdfFile(filePath, kirCert,
                //                CommitmentTypeId.ProofOfOrigin, "location A", signDate,
                //                true, TSA_KIR, null, null, null, kirCert,
                //                img, PdfSignatureLocation.TopLeft,
                //                outputFilePath: tmpFile,
                //                addSignatureApperance: true);


                pdfSignOptions.Certificate = sigillumCert;
                pdfSignOptions.Location = "Location B";
                pdfSignOptions.SignDate = DateTime.Now;
                pdfSignOptions.Reason = CommitmentTypeId.ProofOfApproval;
                pdfSignOptions.TimestampOptions = new TimestampOptions {
                    Certificate = sigillumCert,
                    TsaUrl = TSA_SIGILLUM,
                    TsaPolicy = TSA_POLICY_SIGILLUM
                };
                pdfSignOptions.SignatureLocation = PdfSignatureLocation.TopRight;
                pdfSignOptions.AddVisibleSignature = true;
                pdfSignOptions.AllowMultipleSignatures = true;

                mgr.SignPdfFile(tmpFile, pdfSignOptions, destPath);
                //mgr.SignPdfFile(tmpFile, sigillumCert,
                //                CommitmentTypeId.ProofOfApproval, "location B", DateTime.Now,
                //                true, TSA_SIGILLUM, TSA_POLICY_SIGILLUM, null, null, sigillumCert,
                //                null, PdfSignatureLocation.TopRight, 0,0,WIDTH,HEIGHT,
                //                outputFilePath: destPath,
                //                addSignatureApperance: true,
                //                allowMultipleSignatures: true);

                File.Delete(tmpPath);
                Assert.IsTrue(File.Exists(destPath));
            }
        }
        [Test]
        public void SignPdf_KIRCert_noTs() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(KIR_CERT);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.AddVisibleSignature = true;

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                //mgr.SignPdfFile(filePath, cert,
                //                CommitmentTypeId.ProofOfOrigin,null,signDate,
                                
                //                apperancePngImage: img, 
                //                outputFilePath: destPath,
                //                addSignatureApperance: true);
                Assert.IsTrue(File.Exists(destPath));
            }
        }
        [Test]
        public void SignPdf_KIRCert_TsKIR() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {
                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(KIR_CERT),
                    tsaCert = cert;

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.TimestampOptions = new TimestampOptions {
                    Certificate = cert,
                    TsaUrl = TSA_KIR
                };

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);
                Assert.IsTrue(File.Exists(destPath));
            }
        }

        [Test]
        public void SignPdf_SigillumCert_noTs() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(SIGILLUM_CERT);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.AddVisibleSignature = true;

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                //mgr.SignPdfFile(filePath, cert,
                //                CommitmentTypeId.ProofOfOrigin,null,signDate,

                //                apperancePngImage:img,
                //                outputFilePath: destPath,
                //                addSignatureApperance: true);
                Assert.IsTrue(File.Exists(destPath));
            }
        }
        [Test]
        public void SignPdf_SigillumCert_TsSigillum() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(SIGILLUM_CERT);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.TimestampOptions = new TimestampOptions {
                    Certificate = cert,
                    TsaPolicy = TSA_POLICY_SIGILLUM,
                    TsaUrl = TSA_SIGILLUM
                };
                pdfSignOptions.AddVisibleSignature = true;

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);
                //mgr.SignPdfFile(filePath,cert,
                //                CommitmentTypeId.ProofOfOrigin,null,signDate,
                //                true, TSA_SIGILLUM, TSA_POLICY_SIGILLUM, null, null, cert,
                //                img,
                //                outputFilePath: destPath,
                //                addSignatureApperance: true);
                Assert.IsTrue(File.Exists(destPath));
            }
        }
        #endregion sign pdf test

        #region sign xml test

        [Test]
        public void SignXml_KIRCert_Ts() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                
                PrepareXmlPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(KIR_CERT);
                
                xmlSignOptions.Certificate = cert;
                xmlSignOptions.TimestampOptions = new TimestampOptions {
                    Certificate = cert,
                    TsaUrl = TSA_KIR
                };
                xmlSignOptions.Reason = CommitmentTypeId.ProofOfApproval;

                manager.SignXmlFile(filePath, xmlSignOptions, destPath);

                Assert.IsTrue(File.Exists(destPath));
            }
        }

        

        [Test]
        public void SignXml_KIRCert_noTs() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                PrepareXmlPaths(out string filePath, out string destPath);
                X509Certificate2 cert = CertUtil.GetCertByName(KIR_CERT);

                xmlSignOptions.Certificate = cert;

                manager.SignXmlFile(filePath, xmlSignOptions, destPath);

                //byte[] fileBytes = File.ReadAllBytes(filePath);
                //var fileStream = new MemoryStream(fileBytes);

                //Xades.Signature.SignatureDocument result = manager.AppendSignatureToXmlFile(
                //    fileStream, cert,
                //    timeStampServerUrl: null
                //);

                //if (result != null) {
                //    result.Save(destPath);
                //}
                Assert.IsTrue(File.Exists(destPath));
            }
        }
        [Test]
        public void SignXml_TestCert_noTs() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                PrepareXmlPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(TEST_CERT);

                xmlSignOptions.Certificate = cert;

                manager.SignXmlFile(filePath, xmlSignOptions, destPath);


                Assert.IsTrue(File.Exists(destPath));
            }
        }
        [Test]
        public void SignXml_TestCert_TsCertum() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                PrepareXmlPaths(out string filePath, out string destPath);
                X509Certificate2 cert = CertUtil.GetCertByName(TEST_CERT);

                xmlSignOptions.Certificate = cert;
                xmlSignOptions.TimestampOptions = new TimestampOptions {
                    TsaUrl = TSA_CERTUM
                };

                manager.SignXmlFile(filePath, xmlSignOptions, destPath);

                Assert.IsTrue(File.Exists(destPath));
            }
        }
        [Test]
        public void SignXml_TestCert_TsCertumBasicAuth() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                PrepareXmlPaths(out string filePath, out string destPath);
                X509Certificate2 cert = CertUtil.GetCertByName(TEST_CERT);

                xmlSignOptions.Certificate = cert;
                xmlSignOptions.TimestampOptions = new TimestampOptions {
                    TsaUrl = TSA_CERTUM_BASIC,
                    Login = certum_tsa_login,
                    Password = certum_tsa_password
                };
                xmlSignOptions.Reason = CommitmentTypeId.ProofOfApproval;

                manager.SignXmlFile(filePath, xmlSignOptions, destPath);

                Assert.IsTrue(File.Exists(destPath));
            }
        }
        [Test]
        public void SignXml_SigillumCert_Ts() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                PrepareXmlPaths(out string filePath, out string destPath);
                X509Certificate2 cert = CertUtil.GetCertByName(SIGILLUM_CERT);

                xmlSignOptions.Certificate = cert;
                xmlSignOptions.TimestampOptions = new TimestampOptions {
                    Certificate = cert,
                    TsaUrl = TSA_SIGILLUM,
                    TsaPolicy = TSA_POLICY_SIGILLUM
                };

                manager.SignXmlFile(filePath, xmlSignOptions, destPath);

                Assert.IsTrue(File.Exists(destPath));
            }
        }

        [Test]
        public void SignXml_SigillumCert_noTs() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                PrepareXmlPaths(out string filePath, out string destPath);
                X509Certificate2 cert = CertUtil.GetCertByName(SIGILLUM_CERT);

                xmlSignOptions.Certificate = cert;

                manager.SignXmlFile(filePath, xmlSignOptions, destPath);

                Assert.IsTrue(File.Exists(destPath));
            }
        }
        #endregion sign xml test

        private void PreparePdfPaths(out string filePath, out string destPath) {
            filePath = Path.Combine(testFilesDirPath, PDF_FILE);
            destPath = Path.Combine(signedPath, $"{testName}.pdf");
            if (File.Exists(destPath))
                File.Delete(destPath);
        }

        private void PrepareXmlPaths(out string filePath, out string destPath) {
            filePath = Path.Combine(testFilesDirPath, XML_FILE);
            destPath = Path.Combine(signedPath, $"{testName}.xml");
            if (File.Exists(destPath))
                File.Delete(destPath);
        }

        private byte[] GetImage(string fileName) {
            var imgPath = Path.Combine(testFilesDirPath, fileName);
            var img = File.ReadAllBytes(imgPath);
            return img;
        }
    }
}
