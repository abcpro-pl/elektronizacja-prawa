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
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

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
        private const string TEST_PREFIX = "TEST ";
        private const string SIGILLUM_PREFIX = "SIG ";
        private const string KIR_PREFIX = "KIR ";
        private const string CENCERT_PREFIX = "CEN ";
        private const string CERTUM_PREFIX = "CER ";
        private const string CERTUM_PREFIX_CLIDE = "CER2 ";
        private const string EUROCERT_PREFIX = "EUR ";

        //timestamp urls
        private const string TSA_KIR = "http://www.ts.kir.com.pl/HttpTspServer";
        private const string TSA_CERTUM_BASIC = "https://qts.certum.pl/default/basic";
        private const string TSA_CERTUM = "http://time.certum.pl";
        private const string TSA_SIGILLUM = "http://tsa.sigillum.pl";
        private const string TSA_EUROCERT = ""; //todo
        private const string TSA_CENCERT1 = "http://tsp.cencert.pl";
        private const string TSA_CENCERT2 = "http://tsp2.cencert.pl";



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
        string cencert_cert;
        string certum_cert;
        string certum_clide_cert;
        string eurocert_cert;

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
                HashAlgorithmName = "SHA256",

                TimestampOptions = null
            };

            pdfSignOptions = new PdfSignatureOptions {
                SignDate = signDate,
                Reason = CommitmentTypeId.ProofOfOrigin,
                Location = "Test location",
                HashAlgorithmName = "SHA256",

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

        //[Test]
        //public void SignPdf_TestCert_TsCertum_ClidePfx() {
        //    testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
        //    using (var mgr = new PackageSignerManager()) {

        //        PreparePdfPaths(out string filePath, out string destPath);

        //        X509Certificate2 cert = CertUtil.GetCertByName(test_cert);

        //        var cert2 = CertUtil.GetCertByName(certum_clide_cert);

        //        pdfSignOptions.Certificate = cert;
        //        pdfSignOptions.Location = "Wawa";
        //        pdfSignOptions.AddVisibleSignature = true;
        //        pdfSignOptions.TimestampOptions = new TimestampOptions {
        //            TsaUrl = TSA_CERTUM_BASIC,
        //            Certificate = cert2
        //        };

        //        mgr.SignPdfFile(filePath, pdfSignOptions, destPath);


        //        Assert.IsTrue(File.Exists(destPath));
        //        bool isValid = ValidatePdfSignature(mgr, destPath);
        //        Assert.IsTrue(isValid);
        //    }
        //}


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
        public void SignPdf_KIRCert_NoTs_DateBeforeCertValid() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {
                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(kir_cert),
                    tsaCert = cert;

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.SignDate = new DateTime(2020, 01,01, 10,10,10); //2020-01-01 10:10:10
                //pdfSignOptions.TimestampOptions = new TimestampOptions {
                //    Certificate = cert,
                //    TsaUrl = TSA_KIR
                //};

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);
                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }
        [Test]
        public void SignPdf_TestCert_TsKIR_wrongTsCert() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {
                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(test_cert),
                    tsaCert = cert;

                pdfSignOptions.Certificate = cert;
                pdfSignOptions.TimestampOptions = new TimestampOptions {
                    Certificate = cert,
                    TsaUrl = TSA_KIR
                };
                try {
                    mgr.SignPdfFile(filePath, pdfSignOptions, destPath);
                }
                catch (Exception ex) {
                    Assert.IsTrue(ex is iText.Kernel.PdfException);
                    Assert.IsTrue(null != ex.InnerException);
                    string errMsg = "Invalid TSA http://www.ts.kir.com.pl/HttpTspServer response code 32.";
                    Assert.IsTrue(ex.InnerException.Message == errMsg);
                }
            }
        }


        [Test]
        public void SignPdf_CenCert_TsCencert1() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(cencert_cert);


                pdfSignOptions.Certificate = cert;
                pdfSignOptions.Location = "Wawa";
                pdfSignOptions.AddVisibleSignature = true;
                pdfSignOptions.TimestampOptions = new TimestampOptions {
                    Certificate = cert,
                    TsaUrl = TSA_CENCERT1
                };

                mgr.SignPdfFile(filePath, pdfSignOptions, destPath);


                Assert.IsTrue(File.Exists(destPath));
                bool isValid = ValidatePdfSignature(mgr, destPath);
                Assert.IsTrue(isValid);
            }
        }

        [Test]
        public void SignPdf_CenCert_TsCencert2() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {

                PreparePdfPaths(out string filePath, out string destPath);

                X509Certificate2 cert = CertUtil.GetCertByName(cencert_cert);


                pdfSignOptions.Certificate = cert;
                pdfSignOptions.Location = "Wawa";
                pdfSignOptions.AddVisibleSignature = true;
                pdfSignOptions.TimestampOptions = new TimestampOptions {
                    Certificate = cert,
                    TsaUrl = TSA_CENCERT2
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
        public void SignPdf_SigillumCert_TsSigillum_Signer() {
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

        [Test]
        public void SignPdf_TestCert_LegislatorTest_PastDate() {
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
                pdfSignOptions.SignDate = new DateTime(2022, 01, 02, 11, 07, 45, DateTimeKind.Local);
                //pdfSignOptions.Loc
                var tmpFile = Path.GetTempFileName();
                

                mgr.SignPdfFile(filePath,
                    cert,
                    CommitmentTypeId.ProofOfApproval,
                    "Polska",
                    pdfSignOptions.SignDate,
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
                    destPath,
                    true,
                    false,
                    true);

                
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
        public void SignXml_KIRCert_noTs_multiple() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                PrepareXmlPaths(out string filePath, out string destPath);
                X509Certificate2 cert = CertUtil.GetCertByName(kir_cert);

                xmlSignOptions.Certificate = cert;

                var d1 = destPath + "1.xml";
                var d2 = destPath + "2.xml";
                var d3 = destPath + "3.xml";
                var d4 = destPath + "4.xml";
                manager.SignXmlFile(filePath, xmlSignOptions, d1);
                manager.SignXmlFile(filePath, xmlSignOptions, d2);
                manager.SignXmlFile(filePath, xmlSignOptions, d3);
                manager.SignXmlFile(filePath, xmlSignOptions, d4);

                Assert.IsTrue(File.Exists(d1));
                var result = manager.ValidateSignature(d1);
                Assert.IsTrue(result.IsValid);
                
                Assert.IsTrue(File.Exists(d2));
                result = manager.ValidateSignature(d2);
                Assert.IsTrue(result.IsValid);
                
                Assert.IsTrue(File.Exists(d3));
                result = manager.ValidateSignature(d3);
                Assert.IsTrue(result.IsValid);

                Assert.IsTrue(File.Exists(d4));
                result = manager.ValidateSignature(d4);
                Assert.IsTrue(result.IsValid);
            }
        }

        [Test]
        public void SignXml_KIRCert_noTs_szafirSDK() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var manager = new XadesManager()) {
                PrepareXmlPaths(out string filePath, out string destPath);
                X509Certificate2 cert = CertUtil.GetCertByName(kir_cert);

                xmlSignOptions.Certificate = cert;

                var d1 = destPath + "1.xml";
                var d2 = destPath + "2.xml";
                var d3 = destPath + "3.xml";
                var d4 = destPath + "4.xml";
                manager.SignXmlFile(filePath, xmlSignOptions, d1);
                manager.SignXmlFile(filePath, xmlSignOptions, d2);
                manager.SignXmlFile(filePath, xmlSignOptions, d3);
                manager.SignXmlFile(filePath, xmlSignOptions, d4);

                Assert.IsTrue(File.Exists(d1));
                var result = manager.ValidateSignature(d1);
                Assert.IsTrue(result.IsValid);

                Assert.IsTrue(File.Exists(d2));
                result = manager.ValidateSignature(d2);
                Assert.IsTrue(result.IsValid);

                Assert.IsTrue(File.Exists(d3));
                result = manager.ValidateSignature(d3);
                Assert.IsTrue(result.IsValid);

                Assert.IsTrue(File.Exists(d4));
                result = manager.ValidateSignature(d4);
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
        public void SignXml_TestCert_noTs_NoDateNoNumber() {
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

        [Test]
        public void SignPackageDetached() {
            var path = @"../../../../sample/paczka0real.zip";
            var outPath = @"../../../../sample/paczka0.podpisana.zip";
            var mgr = new PackageSignerManager();
            var cert = CertUtil.GetCertByName(test_cert);
            mgr.Sign(path, cert, outPath, null, null, false, true, true, false, DateTime.Now, false, null);
            Assert.IsTrue(true);
        }
        [Test]
        public void SignPackageAttached() {
            Assert.IsTrue(true);
        }

        [Test]
        public void VerifyXML() {
            var xmlTxt = @"<?xml version=""1.0"" encoding=""utf-8""?>
<uchwala xmlns=""http://www.crd.gov.pl/xml/schematy/edap/2010/01/02"" widoczny=""tak"" status=""uchwalony"" id=""744d07f2-d34f-4a1d-95e6-1cd85350e522"">
  <!--eNadzor 6.00-->
  <metryka id=""4701bde0-10b1-4f3a-91a8-9b3c1f34606b"" status-aktu=""uchwalony"" nazwa=""uchwala"" numer=""NA.I-4213-P-4-2/2023"" opis-typu=""Uchwała"" data=""2023-10-05T13:10:54"" widoczny=""tak"" organ-wydajacy-m=""Skład Orzekający Regionalnej Izby Obrachunkowej w Opolu"" organ-wydajacy=""Składu Orzekającego Regionalnej Izby Obrachunkowej w Opolu"">
    <organ-wydajacy id=""6668a677-8818-445d-a709-db20a19cfd08"" glowny=""tak"" w-mianowniku=""Skład Orzekający Regionalnej Izby Obrachunkowej w Opolu"">Składu Orzekającego Regionalnej Izby Obrachunkowej w Opolu</organ-wydajacy>
    <tytul id=""160c9b3b-ce92-4e3d-a7a6-c411dfe6786d"">w sprawie opinii o możliwości spłaty pożyczki przez Gminę Baborów</tytul>
  </metryka>
  <podstawa-prawna id=""87ae910e-ed41-4a6e-ab3f-6cc468d147ab"" />
  <zalaczniki id=""9c3950a2-3ac8-4f21-975a-81a70372dd7f"">
    <zalacznik_bin id=""5db749e7-2a21-4605-b75d-996cba5958ab"" nazwa=""akt.pdf"" nr=""1"" hiperlacze=""nie"" tytul=""nie"" naglowek=""nie"" tresc-dokumentu=""tak"" />
  </zalaczniki>
<ds:Signature Id=""Signature-3b48f08d-6382-49f7-ad53-e1a03851b8f0"" xmlns:ds=""http://www.w3.org/2000/09/xmldsig#""><ds:SignedInfo><ds:CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" /><ds:SignatureMethod Algorithm=""http://www.w3.org/2001/04/xmldsig-more#rsa-sha256"" /><ds:Reference Id=""Reference-1a628b02-3f0e-4a61-a56f-918eaa035d16"" URI=""#744d07f2-d34f-4a1d-95e6-1cd85350e522""><ds:Transforms><ds:Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" /></ds:Transforms><ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256"" /><ds:DigestValue>dFsbzjj5b+DqM/uBxvW+WDNNnuttOXvbJkOU1TVWhx8=</ds:DigestValue></ds:Reference><ds:Reference Id=""ReferenceKeyInfo"" URI=""#KeyInfoId-Signature-3b48f08d-6382-49f7-ad53-e1a03851b8f0""><ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256"" /><ds:DigestValue>4RRo4QctlNcbX8fiz8h6a7SqwQ+9kx7ocys+UpyTth0=</ds:DigestValue></ds:Reference><ds:Reference URI=""#SignedProperties-Signature-3b48f08d-6382-49f7-ad53-e1a03851b8f0"" Type=""http://uri.etsi.org/01903#SignedProperties""><ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256"" /><ds:DigestValue>VAzqbluJ+FAFwgs+HHIrkl2DUmS3NQWd4jPivtfpuQQ=</ds:DigestValue></ds:Reference></ds:SignedInfo><ds:SignatureValue Id=""SignatureValue-3b48f08d-6382-49f7-ad53-e1a03851b8f0"">AAgAAAAAAAA+1rXhAJsAjHd1AACSAQAAPda24QCcAIwOgAAAAAAAwDzWt+EAnQCMdnUAAAAAAEA71rDhAJ4AjAUAAAAAAAAAOtax4QCfAIxpdQAABAAAADnWsuEAoACMZnUAAAAAAAA41rPhAKEAjEJ1AAAAABAAN9a84QCiAIyYdQAAAAAAADbWveEAowCMxHUAAJIBAAA11r7hAKQAjgAAAAAAAAAANNa/4QClAIwKgAAAAAAAADPWuOEApgCMBwAAAAAAAAAy1rnhAKcAjAAIAAAFBwICMda64QCoAIiwP19/EdUKOjDWu+EAqQCMCoAAAJIBAAAP1qThAKoAjA==</ds:SignatureValue><ds:KeyInfo Id=""KeyInfoId-Signature-3b48f08d-6382-49f7-ad53-e1a03851b8f0""><ds:X509Data><ds:X509Certificate>MIIG3zCCBMegAwIBAgIUXBT07YCIHRIVy7YTQEA/ZIbzrg4wDQYJKoZIhvcNAQELBQAweDELMAkGA1UEBhMCUEwxKDAmBgNVBAoMH0tyYWpvd2EgSXpiYSBSb3psaWN6ZW5pb3dhIFMuQS4xJDAiBgNVBAMMG0NPUEUgU1pBRklSIC0gS3dhbGlmaWtvd2FueTEZMBcGA1UEYQwQVkFUUEwtNTI2MDMwMDUxNzAeFw0yMjA2MDMwNjAwMDBaFw0yNDA2MDMwNjAwMDBaMGcxCzAJBgNVBAYTAlBMMRowGAYDVQQFExFQTk9QTC03MjEyMTgwMTA5MTEYMBYGA1UEAwwPQXJrYWRpdXN6IFRhbGlrMRIwEAYDVQQqDAlBcmthZGl1c3oxDjAMBgNVBAQMBVRhbGlrMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAlRBl+sifK1WbvF+0coYLZeeI82dq+jmFeyGObHFMCpfSs7ASNLf7fAK3Fa3plrxPsC/8sb7HQFu/SEINlOm9q8CHWqsJvCplPSv6ZYy423F+B/7ZuLwt6WHDp7VrEBm72B6AKLIhaqv0cSpAAvXycTyg+BSUCD6ENl9I9YTVmhWk/3krUrDetUu7h1ndIYFl5YCJlXr9xlXFC9JsKb8GLgdQHPwc9Cffbhwy3P2azvHUco/2myQTa2D1PNdq/sFbivjcDN//g1KyOr7hUKuiJ4FEmUlkDyncyCjax7uwaQ5ToG2qtDcRo/V375UNnVwoRHJL8/N7rGVz3UsYSEdZrwIDAQABo4ICcDCCAmwwDAYDVR0TAQH/BAIwADCBxwYDVR0gAQH/BIG8MIG5MIG2BgkqhGgBhvcjAQEwgagwWgYIKwYBBQUHAgIwTgxMQ2VydHlmaWthdCB3eWRhbnkgemdvZG5pZSB6IFphxYLEhWN6bmlraWVtIEkgZG8gUm96cG9yesSFZHplbmlhIG5yIDkxMC8yMDE0LjBKBggrBgEFBQcCARY+aHR0cDovL3d3dy5lbGVrdHJvbmljem55cG9kcGlzLnBsL2luZm9ybWFjamUvZG9rdW1lbnR5LWktdW1vd3kwfwYIKwYBBQUHAQMEczBxMAgGBgQAjkYBATAIBgYEAI5GAQQwRgYGBACORgEFMDwwOhY0aHR0cHM6Ly93d3cuZWxla3Ryb25pY3pueXBvZHBpcy5wbC9QS0lEaXNjbG9zdXJlLnBkZhMCcGwwEwYGBACORgEGMAkGBwQAjkYBBgEwfwYIKwYBBQUHAQEEczBxMC4GCCsGAQUFBzABhiJodHRwOi8vb2NzcC5lbGVrdHJvbmljem55cG9kcGlzLnBsMD8GCCsGAQUFBzAChjNodHRwOi8vZWxla3Ryb25pY3pueXBvZHBpcy5wbC9jZXJ0eWZpa2F0eS9vems2Mi5kZXIwDgYDVR0PAQH/BAQDAgZAMB8GA1UdIwQYMBaAFOaxtBLmR6eEPubDxLiJZ0OjY9F9MEAGA1UdHwQ5MDcwNaAzoDGGL2h0dHA6Ly9lbGVrdHJvbmljem55cG9kcGlzLnBsL2NybC9jcmxfb3prNjIuY3JsMB0GA1UdDgQWBBRZyrtNi4v0Xw2rle+w6eJ0K/JARjANBgkqhkiG9w0BAQsFAAOCAgEACSBomtnVxb5OUYkijMjAQjbC4y08Ny8Hn//4el5tSF8T2Q8ibMxbjd4oGUj1YAlw6+ayoSFJ8e9gortQQvcHrevf5vbmv/+YdcSFWaOMczjg6xAo7M2DdISmtYQcnT8ci40vbQLjmpbvd5SV9fT4fRmCcJCg6F1A9QCrVnjAEiEhCArnQazhtQ56cZzp2R2Y6lv9s7EOx/IDcfZ/3X1DhvurAWnAuuOZd+ac7i9Tm00M9Tr68RUb4zZKTxOkXItqsYrqqP83wdFZvuzzeI/zZsmUM+BD94psITzVQdQEua+3vua2CRKpnKo5lD5iWk51Rqv9N80DV0tnS7K54zuv8D5F02qx+HRCWKdsaAfQ7ufxX8ov85UwP5YB+D3Jd3+0ST87KN7XLjx+OdaDKcdQLjq7BWLVgt9QQzIASeawJ6cbWQ/zBMChOQ1z8V1Ro9GO354by9HTvkT0TPa/rFD7npJpxz5uv7sEB0o7NJiBhcP7o7Hhs00nzctHwiC7YtUyOlk01EYwDp7sk2gIA86WgUE3DDVKbjBUTWgc8DD6dWo3sAHdENB54HdUkSuMR43x+l2z3fimt9TeXpQFbAoXRWrVEExx9lmrPW2vM7FmH4pd4qMFAyCy6Z4DXsFkSsPOiSL3L0xhK9BhUMfCAmhz5CFcZoWZRY3M+gZ+2+lOvFI=</ds:X509Certificate></ds:X509Data><ds:KeyValue><ds:RSAKeyValue><ds:Modulus>lRBl+sifK1WbvF+0coYLZeeI82dq+jmFeyGObHFMCpfSs7ASNLf7fAK3Fa3plrxPsC/8sb7HQFu/SEINlOm9q8CHWqsJvCplPSv6ZYy423F+B/7ZuLwt6WHDp7VrEBm72B6AKLIhaqv0cSpAAvXycTyg+BSUCD6ENl9I9YTVmhWk/3krUrDetUu7h1ndIYFl5YCJlXr9xlXFC9JsKb8GLgdQHPwc9Cffbhwy3P2azvHUco/2myQTa2D1PNdq/sFbivjcDN//g1KyOr7hUKuiJ4FEmUlkDyncyCjax7uwaQ5ToG2qtDcRo/V375UNnVwoRHJL8/N7rGVz3UsYSEdZrw==</ds:Modulus><ds:Exponent>AQAB</ds:Exponent></ds:RSAKeyValue></ds:KeyValue></ds:KeyInfo><ds:Object Id=""XadesObjectId-af8b7d0e-70c6-429a-8776-4538dc31998b""><xades:QualifyingProperties Id=""QualifyingProperties-4baadb07-c3b2-4c03-bfdc-062634cfbe68"" Target=""#Signature-3b48f08d-6382-49f7-ad53-e1a03851b8f0"" xmlns:xades=""http://uri.etsi.org/01903/v1.3.2#""><xades:SignedProperties Id=""SignedProperties-Signature-3b48f08d-6382-49f7-ad53-e1a03851b8f0""><xades:SignedSignatureProperties><xades:SigningTime>2023-10-05T14:00:55+02:00</xades:SigningTime><xades:SigningCertificate><xades:Cert xmlns:ds=""http://www.w3.org/2000/09/xmldsig#""><xades:CertDigest xmlns:ds=""http://www.w3.org/2000/09/xmldsig#""><ds:DigestMethod Algorithm=""http://www.w3.org/2001/04/xmlenc#sha256"" /><ds:DigestValue xmlns:xades=""http://uri.etsi.org/01903/v1.3.2#"">qYhqbFZa/R4Vnew/UyDVpcY78/76PDoGnhx2+1uFRKo=</ds:DigestValue></xades:CertDigest><xades:IssuerSerial xmlns:ds=""http://www.w3.org/2000/09/xmldsig#""><ds:X509IssuerName xmlns:xades=""http://uri.etsi.org/01903/v1.3.2#"">OID.2.5.4.97=VATPL-5260300517, CN=COPE SZAFIR - Kwalifikowany, O=Krajowa Izba Rozliczeniowa S.A., C=PL</ds:X509IssuerName><ds:X509SerialNumber xmlns:xades=""http://uri.etsi.org/01903/v1.3.2#"">525694502035300083910838424591943385047859703310</ds:X509SerialNumber></xades:IssuerSerial></xades:Cert></xades:SigningCertificate></xades:SignedSignatureProperties><xades:SignedDataObjectProperties><xades:DataObjectFormat ObjectReference=""#Reference-1a628b02-3f0e-4a61-a56f-918eaa035d16""><xades:Description>
MIME-Version: 1.0
Content-Type: text/xml
Content-Transfer-Encoding: UTF-8</xades:Description><xades:MimeType>text/xml</xades:MimeType><xades:Encoding>UTF-8</xades:Encoding></xades:DataObjectFormat><xades:CommitmentTypeIndication><xades:CommitmentTypeId><xades:Identifier>http://uri.etsi.org/01903/v1.2.2#ProofOfApproval</xades:Identifier><xades:Description></xades:Description></xades:CommitmentTypeId><xades:AllSignedDataObjects /></xades:CommitmentTypeIndication></xades:SignedDataObjectProperties></xades:SignedProperties></xades:QualifyingProperties></ds:Object></ds:Signature></uchwala>";

            var tmpFile = Path.GetTempFileName();
            File.WriteAllText(tmpFile, xmlTxt);

            using (var manager = new XadesManager()) {
                manager.ValidateSignature(tmpFile);
            }

            File.Delete(tmpFile);
            Assert.IsTrue(true);
            //var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            //xmlDoc.LoadXml(xmlTxt);

            //var signedXml = new SignedXml(xmlDoc);
            //XmlNodeList nodeList = xmlDoc.GetElementsByTagName("ds:Signature");
            //XmlNodeList certificates = xmlDoc.GetElementsByTagName("ds:X509Certificate");
            //X509Certificate2 dcert2 = new X509Certificate2(Convert.FromBase64String(certificates[0].InnerText));
            //bool passes = true;
            //foreach (XmlElement element in nodeList) {
            //    signedXml.LoadXml(element);
            //    passes &= signedXml.CheckSignature(dcert2, true);
            //}
            //Assert.IsTrue(passes);
        }

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
                    else if (line.Contains(CENCERT_PREFIX)) {
                        var split = line.Split(CENCERT_PREFIX, StringSplitOptions.RemoveEmptyEntries);
                        cencert_cert = split[0].Trim();
                    }
                    else if (line.Contains(EUROCERT_PREFIX)) {
                        var split = line.Split(EUROCERT_PREFIX, StringSplitOptions.RemoveEmptyEntries);
                        eurocert_cert = split[0].Trim();
                    }
                    else if (line.Contains(CERTUM_PREFIX)) {
                        var split = line.Split(CERTUM_PREFIX, StringSplitOptions.RemoveEmptyEntries);
                        certum_cert = split[0].Trim();
                    }
                    else if (line.Contains(CERTUM_PREFIX_CLIDE)) {
                        var split = line.Split(CERTUM_PREFIX_CLIDE, StringSplitOptions.RemoveEmptyEntries);
                        certum_clide_cert = split[0].Trim();
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

            StringBuilder sb = new StringBuilder();
            if (null == kir_cert || kir_cert == string.Empty)
                sb.AppendLine($"Brak nazwy certyfikatu KIR w pliku {certNamesPath}");
            if (null == sigillum_cert || sigillum_cert == string.Empty)
                sb.AppendLine($"Brak nazwy certyfikatu Sigillum w pliku {certNamesPath}");
            if (null == cencert_cert || cencert_cert == string.Empty)
                sb.AppendLine($"Brak nazwy certyfikatu Cencert w pliku {certNamesPath}");
            if (null == certum_cert || certum_cert == string.Empty)
                sb.AppendLine($"Brak nazwy certyfikatu Certum w pliku {certNamesPath}");
            if (null == eurocert_cert || eurocert_cert == string.Empty)
                sb.AppendLine($"Brak nazwy certyfikatu EuroCert w pliku {certNamesPath}");
            if (null == test_cert || test_cert == string.Empty)
                sb.AppendLine($"Brak nazwy certyfikatu Test w pliku {certNamesPath}");

            if (sb.Length > 0)
                throw new ArgumentException(sb.ToString());
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
