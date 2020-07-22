using Abc.Nes.Xades;
using Abc.Nes.Xades.Signature.Parameters;
using Abc.Nes.Xades.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;

namespace Abc.Nes.UnitTests {
    [TestClass]
    public class XadesServiceUnitTest {
        [TestMethod]
        public void XadesService_SignXmlFile() {
            var document = DocumentUnitTest.GetModel();
            var documentXml = new Abc.Nes.Converters.XmlConverter().GetXml(document);

            using (IXadesService service = new XadesService()) {
                var xml = new MemoryStream(Encoding.UTF8.GetBytes(documentXml.ToString()));
                var signatureParams = new SignatureParameters() {
                    SignaturePolicyInfo = new SignaturePolicyInfo(),
                    SignaturePackaging = SignaturePackaging.ENVELOPED,
                    DataFormat = new DataFormat() {
                        MimeType = MimeTypeUtil.GetMimeType("nes.xml")
                    },
                    Signer = new Xades.Crypto.Signer(CertUtil.SelectCertificate()),
                    DigestMethod = Xades.Crypto.DigestMethod.SHA256,
                    SignatureMethod = Xades.Crypto.SignatureMethod.RSAwithSHA256
                };


                var result = service.Sign(xml, signatureParams);

                var filePath = Path.Combine(Path.GetTempPath(), "signature.xml");
                if (File.Exists(filePath)) { File.Delete(filePath); }
                result.Save(filePath);

                Assert.IsTrue(result != null && result.Document != null && result.Document.OuterXml != null && File.Exists(filePath));

                System.Diagnostics.Process.Start(filePath);
            }
        }
    }
}
