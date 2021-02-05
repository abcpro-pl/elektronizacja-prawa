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
        [TestMethod]
        public void TestXmlDsigEnveloped() {
            var mgr = new XmlDsigEnveloped(true, new CertManager());
            var fileName = Path.Combine(Path.GetTempPath(), "Some.xml");
            mgr.CreateSomeXml(fileName);
            mgr.SignXml(fileName);
            var result = mgr.VerifyFile(fileName);
            Assert.IsTrue(result);
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
