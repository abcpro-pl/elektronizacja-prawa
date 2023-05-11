using Abc.Nes.ArchivalPackage.Cryptography;
using Abc.Nes.Common;
using Abc.Nes.Common.Models;
using Abc.Nes.Xades.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Abc.Nes.UnitTests {
    [TestClass]
    public class SignAndVerifyUnitTest {
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

        
        [TestInitialize]
        public void TestInit() {
            pwd = Directory.GetCurrentDirectory();
            testFilesDirPath = Path.GetFullPath(Path.Combine(pwd, @"../../../sample/"));
            signedPath = Path.Combine(testFilesDirPath, "TimestampTests");


            //LoadCertificateNamesData();
            //LoadCertumBasicAuthData();

            if (!Directory.Exists(signedPath))
                Directory.CreateDirectory(signedPath);

            img = GetImage(LOGO_FILE);
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

        [TestMethod]
        public void TestXmlDsigEnveloped() {
            var mgr = new XmlDsigEnveloped(true, new CertManager());
            var fileName = Path.Combine(Path.GetTempPath(), "Some.xml");
            mgr.CreateSomeXml(fileName);
            mgr.SignXml(fileName);
            var result = mgr.VerifyFile(fileName);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SignPdf_KIRCert_TsKIR() {
            testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            using (var mgr = new PackageSignerManager()) {
                PreparePdfPaths(out string filePath, out string destPath);
                kir_cert = "Marcin I";

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


        private void PreparePdfPaths(string inputFileName, out string filePath, out string destPath) {
            filePath = Path.Combine(testFilesDirPath, inputFileName);
            destPath = Path.Combine(signedPath, $"{testName}_48.pdf");
            if (File.Exists(destPath))
                File.Delete(destPath);
        }
        private void PreparePdfPaths(out string filePath, out string destPath) {
            filePath = Path.Combine(testFilesDirPath, PDF_FILE);
            destPath = Path.Combine(signedPath, $"{testName}_48.pdf");
            if (File.Exists(destPath))
                File.Delete(destPath);
        }

        private void PrepareXmlPaths(out string filePath, out string destPath) {
            filePath = Path.Combine(testFilesDirPath, XML_FILE);
            destPath = Path.Combine(signedPath, $"{testName}_48.xml");
            if (File.Exists(destPath))
                File.Delete(destPath);
        }
        private static bool ValidatePdfSignature(PackageSignerManager mgr, string destPath) {
            var result = mgr.GetPdfFileSignAndVerifyInfo(destPath);
            bool isValid = true;
            foreach (var item in result.VerifyInfo) {
                isValid = isValid && item.IsValid;
            }

            return isValid;
        }
        private byte[] GetImage(string fileName) {
            var imgPath = Path.Combine(testFilesDirPath, fileName);
            var img = File.ReadAllBytes(imgPath);
            return img;
        }
    }

    public class CertManager {
        // note that both *.pfx location and the password are hardcoded!
        // please customize it in a production code
        private X509Certificate2 _certificate;
        public X509Certificate2 Certificate {
            get {
                if (_certificate == null) {
                    _certificate = Xades.Utils.CertUtil.SelectCertificate();

                    //using (FileStream fs =
                    //   File.Open("demo.pfx", FileMode.Open))
                    //using (BinaryReader br = new BinaryReader(fs)) {
                    //    _certificate =
                    //        new X509Certificate2(
                    //           br.ReadBytes((int)br.BaseStream.Length), "demo");
                    //}
                }

                return _certificate;
            }
        }
    }


    public interface IDemo {
        string Name { get; }
        void SignXml(string fileName);
        string SignXml(XmlDocument Document);
        bool VerifyFile(string fileName);
        bool VerifyXml(string SignedXmlDocumentString);
        bool VerifyXmlFromStream(Stream SignedXmlDocumentStream);
    }

    public abstract class BaseXmlDsig : IDemo {
        public abstract string Name { get; }

        public abstract string SignXml(XmlDocument Document);
        public abstract void SignXml(string fileName);

        public bool VerifyXml(string SignedXmlDocumentString) {
            byte[] stringData = Encoding.UTF8.GetBytes(SignedXmlDocumentString);
            using (MemoryStream ms = new MemoryStream(stringData))
                return VerifyXmlFromStream(ms);
        }

        public bool VerifyFile(string fileName) {
            byte[] stringData = File.ReadAllBytes(fileName);
            using (MemoryStream ms = new MemoryStream(stringData))
                return VerifyXmlFromStream(ms);
        }

        public bool VerifyXmlFromStream(Stream SignedXmlDocumentStream) {
            // load the document to be verified
            XmlDocument xd = new XmlDocument();
            xd.PreserveWhitespace = true;
            xd.Load(SignedXmlDocumentStream);

            var signedXml = new SignedXml(xd);

            // load the first <signature> node and load the signature  
            XmlNode MessageSignatureNode =
               xd.GetElementsByTagName("Signature", SignedXml.XmlDsigNamespaceUrl)[0];

            signedXml.LoadXml((XmlElement)MessageSignatureNode);

            // get the cert from the signature
            X509Certificate2 certificate = null;
            foreach (KeyInfoClause clause in signedXml.KeyInfo) {
                if (clause is KeyInfoX509Data) {
                    if (((KeyInfoX509Data)clause).Certificates.Count > 0) {
                        certificate =
                        (X509Certificate2)((KeyInfoX509Data)clause).Certificates[0];
                    }
                }
            }
                        
            // check the signature and return the result.
            return signedXml.CheckSignature(certificate, true);
        }     
    }

    public class XmlDsigEnveloped : BaseXmlDsig, IDemo {
        private CertManager manager { get; set; }
        private bool c14 { get; set; }

        public XmlDsigEnveloped(bool c14, CertManager manager) {
            this.manager = manager;
            this.c14 = c14;
        }

        public override string Name {
            get {
                return string.Format("XmlDsigEnveloped {0} c14", c14 ? "with" : "without");
            }
        }

        public override void SignXml(string fileName) {
            var doc = new XmlDocument();
            doc.Load(fileName);

            var xml = SignXml(doc);
            File.WriteAllText(fileName, xml, Encoding.UTF8);
        }

        public override string SignXml(XmlDocument Document) {
            SignedXml signedXml = new SignedXml(Document);
            signedXml.SigningKey = manager.Certificate.PrivateKey;

            // Create a reference to be signed.
            Reference reference = new Reference();
            reference.Uri = "";

            // Add an enveloped transformation to the reference.            
            XmlDsigEnvelopedSignatureTransform env =
               new XmlDsigEnvelopedSignatureTransform(true);
            reference.AddTransform(env);

            if (c14) {
                XmlDsigC14NTransform c14t = new XmlDsigC14NTransform();
                reference.AddTransform(c14t);
            }

            KeyInfo keyInfo = new KeyInfo();
            KeyInfoX509Data keyInfoData = new KeyInfoX509Data(manager.Certificate);
            keyInfo.AddClause(keyInfoData);
            signedXml.KeyInfo = keyInfo;

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            // Compute the signature.
            signedXml.ComputeSignature();

            // Get the XML representation of the signature and save 
            // it to an XmlElement object.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            Document.DocumentElement.AppendChild(
                Document.ImportNode(xmlDigitalSignature, true));

            return Document.OuterXml;
        }

        public XmlDocument CreateSomeXml() {
            var xml = getXml();
            var doc = new XmlDocument();
            doc.LoadXml(xml.ToString());
            return doc;
        }
        public void CreateSomeXml(string fileName) {
            var xml = getXml();
            xml.Save(fileName);
            
        }

        private System.Xml.Linq.XElement getXml() { 
        return System.Xml.Linq.XElement.Parse(@"<uchwala xmlns=""samples"">
    <!-- comment -->
    <element>
        Example text to be signed.
    </element>
</uchwala>");
        }
    }
}
