using Abc.Nes.ArchivalPackage.Cryptography;
using Abc.Nes.Xades;
using Abc.Nes.Xades.Signature.Parameters;
using Abc.Nes.Xades.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace Abc.Nes.UnitTests {
    [TestClass]
    public class XadesServiceUnitTest {
        [TestMethod]
        public void XadesManager_AppendSignatureToXmlFile() {
            var document = DocumentUnitTest.GetModel();
            var documentXml = new Abc.Nes.Converters.XmlConverter().GetXml(document);

            using (var manager = new XadesManager()) {
                var xml = new MemoryStream(Encoding.UTF8.GetBytes(documentXml.ToString()));
                var result = manager.AppendSignatureToXmlFile(xml, CertUtil.SelectCertificate(),
                    new SignatureProductionPlace() {
                        City = "Warszawa",
                        CountryName = "Polska",
                        PostalCode = "03-825",
                        StateOrProvince = "mazowieckie"
                    },
                    new SignerRole("Wiceprezes Zarządu"));

                var filePath = Path.Combine(Path.GetTempPath(), "signature.xml");
                if (File.Exists(filePath)) { File.Delete(filePath); }
                result.Save(filePath);

                Assert.IsTrue(result != null && result.Document != null && result.Document.OuterXml != null && File.Exists(filePath));

                System.Diagnostics.Process.Start(filePath);
            }
        }

        [TestMethod]
        public void XadesManager_CreateEnvelopingSignature() {
            var path = "../../../doc/nes_20_generated.pdf";
            using (var manager = new XadesManager()) {
               var result = manager.CreateEnvelopingSignature(new MemoryStream(File.ReadAllBytes(path)), CertUtil.SelectCertificate(),
                    new SignatureProductionPlace() {
                        City = "Warszawa",
                        CountryName = "Polska",
                        PostalCode = "03-825",
                        StateOrProvince = "mazowieckie"
                    },
                    new SignerRole("Wiceprezes Zarządu"));

                var filePath = Path.Combine(Path.GetTempPath(), "signature.xml");
                if (File.Exists(filePath)) { File.Delete(filePath); }
                result.Save(filePath);

                Assert.IsTrue(result != null && result.Document != null && result.Document.OuterXml != null && File.Exists(filePath));

                System.Diagnostics.Process.Start(filePath);
            }
        }
    }
}
