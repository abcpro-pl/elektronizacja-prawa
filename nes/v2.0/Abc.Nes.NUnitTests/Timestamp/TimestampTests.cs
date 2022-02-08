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
        const string PDF_LEGISLATOR_FILE = "legislatorTest.pdf";
        const string XML_FILE = "testFile.xml";
        const string LOGO_FILE = "TestLogo1.png";

        //test certificates names
        //private const string TEST_CERT = "localhost";
        //private const string KIR_CERT = "X";
        //private const string SIGILLUM_CERT = "Y";
        
        //test certificate prefixes
        private const string TEST_PREFIX = "TEST";
        private const string SIGILLUM_PREFIX = "SIG";
        private const string KIR_PREFIX = "KIR";

        //timestamp urls
        private const string TSA_KIR = "http://www.ts.kir.com.pl/HttpTspServer";
        private const string TSA_CERTUM_BASIC = "https://qts.certum.pl/default/basic";
        private const string TSA_CERTUM = "http://time.certum.pl";
        private const string TSA_SIGILLUM = "http://tsa.sigillum.pl";

        //auth data file names
        /// <summary>
        /// File with login and password for certum timestamp server basic auth.<br/>
        /// Login in 1st line, password in 2nd example:<br/>
        /// myLogin<br/>
        /// secretPasword
        /// </summary>
        private const string CERTUM_TSA_AUTH_FILE = "certum.tsa.pass.txt";
        /// <summary>
        /// File with certificate names. Format:<br/>
        /// PREFIX Certificate name<br/>
        /// example:<br/>
        /// KIR John Doe<br/>
        /// SIG Jane Smith
        /// </summary>
        private const string CERTIFICATE_NAMES_FILE = "cert.names.txt";

        //tsa policies
        private const string TSA_POLICY_SIGILLUM = "1.2.616.1.113725.0.0.5";

        //pdf signature params
        int WIDTH = 150;
        int HEIGHT = 40;
        int MARGIN = 2;

        string test_cert;
        string kir_cert;
        string sigillum_cert;

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
            testFilesDirPath = Path.GetFullPath(Path.Combine(pwd, @"../../../../sample/"));
            signedPath = Path.Combine(testFilesDirPath, "TimestampTests");

            
            LoadCertificateNamesData();
            LoadCertumBasicAuthData();

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
                Margin = MARGIN,
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

                X509Certificate2 cert = CertUtil.GetCertByName(test_cert);

                
                pdfSignOptions.Certificate = cert;
                pdfSignOptions.Location = "Wawa";
                pdfSignOptions.AddVisibleSignature = true;
                pdfSignOptions.TimestampOptions = new TimestampOptions {
                    TsaUrl = TSA_CERTUM_BASIC,
                    Login = certum_tsa_login,
                    Password=certum_tsa_password
                };

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                
                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }


        [Test]
        public void SignPdf_TestCert_noTS_double() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                var tmpFile = Path.GetTempFileName();
                var tmpPath = Path.Combine(testFilesDirPath, tmpFile);

                X509Certificate2 cert = CertUtil.GetCertByName(test_cert);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.Location = "1st signature location";
                pdfSignOptions.AddVisibleSignature = true;

                mgr.SignPdfFile(filePath, pdfSignOptions, tmpFile);
                

                pdfSignOptions.Reason = CommitmentTypeId.ProofOfApproval;
                pdfSignOptions.Location = "2nd signature location";
                pdfSignOptions.SignDate = DateTime.Now;
                pdfSignOptions.Width = WIDTH + 15;
                pdfSignOptions.AllowMultipleSignatures = true;
                mgr.SignPdfFile(tmpFile, pdfSignOptions, destPath);
                

                File.Delete(tmpPath);
                
                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }

        [Test]
        public void SignPdf_TestCert_noTS_background() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(test_cert);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.ImageAsBackground = true;
                pdfSignOptions.AddVisibleSignature = true;

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }
        [Test]
        public void SignPdf_TestCert_noTS_pastDate() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(test_cert);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.AddVisibleSignature = true;
                pdfSignOptions.SignDate = new DateTime(2022, 1, 3, 10, 11, 25, DateTimeKind.Local);

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);
                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }

        [Test]
        public void SignPdf_TestCert_noTS_graphic() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(test_cert);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.AddVisibleSignature = true;

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
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

                X509Certificate2 cert = CertUtil.GetCertByName(test_cert);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.AddVisibleSignature = true;
                pdfSignOptions.SignatureImage = null;

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                
                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }

        [Test]
        public void SignPdf_TestCert_TsCertum() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(test_cert);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.AddVisibleSignature = true;
                pdfSignOptions.TimestampOptions = new TimestampOptions {
                    TsaUrl = TSA_CERTUM
                };

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                
                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }

        [Test]
        public void SignPdf_TestCert_TsCertum_NoWidget() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(test_cert);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.TimestampOptions = new TimestampOptions { 
                    TsaUrl=TSA_CERTUM
                };

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                
                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }

        [Test]
        public void SignPdf_TestCert_noTS_noWidget() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            using (var mgr = new PackageSignerManager()) {
                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(test_cert);

                pdfSignOptions.Certificate = cert;

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                
                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
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

                X509Certificate2 kirCert = CertUtil.GetCertByName(kir_cert),
                    sigillumCert = CertUtil.GetCertByName(sigillum_cert);

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
                

                File.Delete(tmpPath);
                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }
        [Test]
        public void SignPdf_KIRCert_noTs() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(kir_cert);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.AddVisibleSignature = true;

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

               
                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }

        [Test]
        public void SignPdf_KIRCert_noTs_double () {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(kir_cert);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.Location = "1st signature location";
                pdfSignOptions.AddVisibleSignature = true;
                var tmpFile = Path.GetTempFileName();
                var tmpPath = Path.Combine(testFilesDirPath, tmpFile);

                mgr.SignPdfFile(filePath, pdfSignOptions, tmpFile);

                pdfSignOptions.Reason = CommitmentTypeId.ProofOfApproval;
                pdfSignOptions.Location = "2nd signature location";
                pdfSignOptions.SignDate = DateTime.Now;
                pdfSignOptions.Width = WIDTH + 15;
                pdfSignOptions.AllowMultipleSignatures = true;
                mgr.SignPdfFile(tmpFile, pdfSignOptions, destPath);

                File.Delete(tmpPath);
                
                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }

        [Test]
        public void SignPdf_KIRCert_TsKIR() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {
                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(kir_cert),
                    tsaCert = cert;

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.TimestampOptions = new TimestampOptions {
                    Certificate = cert,
                    TsaUrl = TSA_KIR
                };

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);
                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }

        [Test]
        public void SignPdf_SigillumCert_noTs() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(sigillum_cert);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.AddVisibleSignature = true;

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);

                
                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }
        [Test]
        public void SignPdf_SigillumCert_TsSigillum() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(sigillum_cert);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.TimestampOptions = new TimestampOptions {
                    Certificate = cert,
                    TsaPolicy = TSA_POLICY_SIGILLUM,
                    TsaUrl = TSA_SIGILLUM
                };
                pdfSignOptions.AddVisibleSignature = true;

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);
                
                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }

        [Test]
        public void SignPdf_KIRCert_LegislatorTest() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using(var mgr = new PackageSignerManager()) {
                PreparePdfPaths(PDF_LEGISLATOR_FILE, out string filePath, out string destPath);
                X509Certificate2 cert = CertUtil.GetCertByName(kir_cert);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.Reason = CommitmentTypeId.ProofOfApproval;
                pdfSignOptions.Location = "Polska";
                pdfSignOptions.TimestampOptions = new TimestampOptions {
                    TsaUrl = TSA_CERTUM
                };
                pdfSignOptions.SignatureImage = null;
                //pdfSignOptions.Loc

                mgr.SignPdfFile(filePath,
                    cert,
                    CommitmentTypeId.ProofOfApproval,
                    "Polska",
                    DateTime.Now,
                    false,
                    TSA_CERTUM,
                    null,
                    null,
                    null,
                    null,
                    null,
                    PdfSignatureLocation.TopRight,
                    360F,
                    790F,
                    150F,
                    HEIGHT,
                    MARGIN,
                    destPath,
                    true,
                    false, 
                    true);

                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }
        [Test]
        public void SignPdf_KIRCert_LegislatorTest_TsKir() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {
                PreparePdfPaths(PDF_LEGISLATOR_FILE, out string filePath, out string destPath);
                X509Certificate2 cert = CertUtil.GetCertByName(kir_cert);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.Reason = CommitmentTypeId.ProofOfApproval;
                pdfSignOptions.Location = "Polska";
                pdfSignOptions.TimestampOptions = new TimestampOptions {
                    TsaUrl = TSA_CERTUM
                };
                pdfSignOptions.SignatureImage = null;
                //pdfSignOptions.Loc

                mgr.SignPdfFile(filePath,
                    cert,
                    CommitmentTypeId.ProofOfApproval,
                    "Polska",
                    DateTime.Now,
                    true,
                    TSA_KIR,
                    null,
                    null,
                    null,
                    cert,
                    null,
                    PdfSignatureLocation.TopRight,
                    360F,
                    790F,
                    150F,
                    HEIGHT,
                    10F,
                    destPath,
                    true,
                    false,
                    true);

                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }
        [Test]
        public void SignPdf_TestCert_LegislatorTest_double() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {
                PreparePdfPaths(PDF_LEGISLATOR_FILE, out string filePath, out string destPath);
                X509Certificate2 cert = CertUtil.GetCertByName(test_cert);

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.Reason = CommitmentTypeId.ProofOfApproval;
                pdfSignOptions.Location = "Polska";
                pdfSignOptions.TimestampOptions = new TimestampOptions {
                    TsaUrl = TSA_CERTUM
                };
                pdfSignOptions.SignatureImage = null;
                //pdfSignOptions.Loc
                var tmpFile = Path.GetTempFileName();
                var tmpPath = Path.Combine(testFilesDirPath, tmpFile);

                mgr.SignPdfFile(filePath,
                    cert,
                    CommitmentTypeId.ProofOfApproval,
                    "Polska",
                    DateTime.Now,
                    true,
                    TSA_CERTUM,
                    null,
                    null,
                    null,
                    null,
                    null,
                    PdfSignatureLocation.TopRight,
                    360F,
                    790F,
                    WIDTH,
                    HEIGHT,
                    10F,
                    tmpPath,
                    true,
                    false,
                    true);


                mgr.SignPdfFile(tmpPath,
                    cert,
                    CommitmentTypeId.ProofOfApproval,
                    "Polska",
                    DateTime.Now,
                    false,
                    TSA_CERTUM,
                    null,
                    null,
                    null,
                    null,
                    null,
                    PdfSignatureLocation.TopRight,
                    360F,
                    790F,
                    150F,
                    HEIGHT,
                    10F,
                    destPath,
                    true,
                    false,
                    true);

                File.Delete(tmpPath);
                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }
        #endregion sign pdf test

        #region sign xml test

        [Test]
        public void SignXml_KIRCert_Ts() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                
                PrepareXmlPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(kir_cert);
                
                xmlSignOptions.Certificate = cert;
                xmlSignOptions.TimestampOptions = new TimestampOptions {
                    Certificate = cert,
                    TsaUrl = TSA_KIR
                };
                xmlSignOptions.Reason = CommitmentTypeId.ProofOfApproval;

                manager.SignXmlFile(filePath, xmlSignOptions, destPath);

                Assert.IsTrue(File.Exists(destPath));
                var result = manager.ValidateSignature(destPath);
                Assert.IsTrue(result.IsValid);
            }
        }

        

        [Test]
        public void SignXml_KIRCert_noTs() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                PrepareXmlPaths(out string filePath, out string destPath);
                X509Certificate2 cert = CertUtil.GetCertByName(kir_cert);

                xmlSignOptions.Certificate = cert;

                manager.SignXmlFile(filePath, xmlSignOptions, destPath);

                
                Assert.IsTrue(File.Exists(destPath));
                var result = manager.ValidateSignature(destPath);
                Assert.IsTrue(result.IsValid);
            }
        }
        [Test]
        public void SignXml_TestCert_noTs() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                PrepareXmlPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(test_cert);

                xmlSignOptions.Certificate = cert;

                manager.SignXmlFile(filePath, xmlSignOptions, destPath);


                Assert.IsTrue(File.Exists(destPath));
                var result = manager.ValidateSignature(destPath);
                Assert.IsTrue(result.IsValid);
            }
        }
        [Test]
        public void SignXml_TestCert_noTs_pastDate() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                PrepareXmlPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(test_cert);

                xmlSignOptions.Certificate = cert;
                xmlSignOptions.SignDate = new DateTime(2022, 01, 02, 11, 07, 45, DateTimeKind.Local);

                manager.SignXmlFile(filePath, xmlSignOptions, destPath);

                Assert.IsTrue(File.Exists(destPath));
                var result = manager.ValidateSignature(destPath);
                Assert.IsTrue(result.IsValid);
            }
        }
        [Test]
        public void SignXml_TestCert_TsCertum() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                PrepareXmlPaths(out string filePath, out string destPath);
                X509Certificate2 cert = CertUtil.GetCertByName(test_cert);

                xmlSignOptions.Certificate = cert;
                xmlSignOptions.TimestampOptions = new TimestampOptions {
                    TsaUrl = TSA_CERTUM
                };

                manager.SignXmlFile(filePath, xmlSignOptions, destPath);

                Assert.IsTrue(File.Exists(destPath));
                var result = manager.ValidateSignature(destPath);
                Assert.IsTrue(result.IsValid);
            }
        }
        [Test]
        public void SignXml_TestCert_TsCertumBasicAuth() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                PrepareXmlPaths(out string filePath, out string destPath);
                X509Certificate2 cert = CertUtil.GetCertByName(test_cert);

                xmlSignOptions.Certificate = cert;
                xmlSignOptions.TimestampOptions = new TimestampOptions {
                    TsaUrl = TSA_CERTUM_BASIC,
                    Login = certum_tsa_login,
                    Password = certum_tsa_password
                };
                xmlSignOptions.Reason = CommitmentTypeId.ProofOfApproval;

                manager.SignXmlFile(filePath, xmlSignOptions, destPath);

                Assert.IsTrue(File.Exists(destPath));
                var result = manager.ValidateSignature(destPath);
                Assert.IsTrue(result.IsValid);
            }
        }
        [Test]
        public void SignXml_SigillumCert_Ts() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                PrepareXmlPaths(out string filePath, out string destPath);
                X509Certificate2 cert = CertUtil.GetCertByName(sigillum_cert);

                xmlSignOptions.Certificate = cert;
                xmlSignOptions.TimestampOptions = new TimestampOptions {
                    Certificate = cert,
                    TsaUrl = TSA_SIGILLUM,
                    TsaPolicy = TSA_POLICY_SIGILLUM
                };

                manager.SignXmlFile(filePath, xmlSignOptions, destPath);

                Assert.IsTrue(File.Exists(destPath));
                var result = manager.ValidateSignature(destPath);
                Assert.IsTrue(result.IsValid);
            }
        }

        [Test]
        public void SignXml_SigillumCert_noTs() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                PrepareXmlPaths(out string filePath, out string destPath);
                X509Certificate2 cert = CertUtil.GetCertByName(sigillum_cert);

                xmlSignOptions.Certificate = cert;

                manager.SignXmlFile(filePath, xmlSignOptions, destPath);

                Assert.IsTrue(File.Exists(destPath));
                var result = manager.ValidateSignature(destPath);
                Assert.IsTrue(result.IsValid);
            }
        }
        #endregion sign xml test
        private void PreparePdfPaths(string inputFileName, out string filePath, out string destPath) {
            filePath = Path.Combine(testFilesDirPath, inputFileName);
            destPath = Path.Combine(signedPath, $"{testName}.pdf");
            if (File.Exists(destPath))
                File.Delete(destPath);
        }
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

        private void LoadCertificateNamesData() {
            var certNamesPath = Path.Combine(testFilesDirPath, CERTIFICATE_NAMES_FILE);
            if (File.Exists(certNamesPath)) {
                var certTxt = File.ReadAllLines(certNamesPath);
                foreach (string line in certTxt) {
                    if (line.Contains(KIR_PREFIX)) {
                        var split = line.Split(KIR_PREFIX, StringSplitOptions.RemoveEmptyEntries);
                        kir_cert = split[0].Trim();
                    }
                    else if (line.Contains(SIGILLUM_PREFIX)) {
                        var split = line.Split(SIGILLUM_PREFIX, StringSplitOptions.RemoveEmptyEntries);
                        sigillum_cert = split[0].Trim();
                    }
                    else if (line.Contains(TEST_PREFIX)) {
                        var split = line.Split(TEST_PREFIX, StringSplitOptions.RemoveEmptyEntries);
                        test_cert = split[0].Trim();
                    }
                }
            }
            else {
                throw new FileNotFoundException(certNamesPath);
            }

            if (null == kir_cert || kir_cert == string.Empty)
                throw new ArgumentException($"Brak nazwy certyfikatu KIR w pliku {certNamesPath}");
            if (null == sigillum_cert || sigillum_cert == string.Empty)
                throw new ArgumentException($"Brak nazwy certyfikatu Sigillum w pliku {certNamesPath}");
            if (null == test_cert || test_cert == string.Empty)
                throw new ArgumentException($"Brak nazwy certyfikatu Test w pliku {certNamesPath}");
        }

        private void LoadCertumBasicAuthData() {
            var certumFilePath = Path.Combine(testFilesDirPath, CERTUM_TSA_AUTH_FILE);
            if (File.Exists(certumFilePath)) {
                var authTxt = File.ReadAllLines(certumFilePath);
                certum_tsa_login = authTxt[0];
                certum_tsa_password = authTxt[1];
            }
            else {
                throw new FileNotFoundException(certumFilePath);
            }
        }

        private static bool ValidatePdfSignature(PackageSignerManager mgr, string destPath) {
            var result = mgr.GetPdfFileSignAndVerifyInfo(destPath);
            bool isValid = true;
            foreach (var item in result.VerifyInfo) {
                isValid = isValid && item.IsValid;
            }

            return isValid;
        }
    }
}
