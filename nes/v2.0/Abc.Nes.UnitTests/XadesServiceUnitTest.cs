using Abc.Nes.ArchivalPackage;
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
        public void ResignPackage() {
            var path = @"../../../sample/Paczka.testowa.30.03.21.podpisana.zip";

            var signedPackageManager = new SignedPackageManager();
            var info = signedPackageManager.Extract(path);

            try { File.Delete(Path.Combine(info.Directory, info.SignatureFileName)); } catch { }

            using (var manager = new XadesManager()) {
                var result = manager.CreateEnvelopingSignature(
                    new MemoryStream(File.ReadAllBytes(Path.Combine(info.Directory, info.PackageFileName))), 
                    CertUtil.SelectCertificate(),
                    new SignatureProductionPlace() {
                        City = "Warszawa",
                        CountryName = "Polska",
                        PostalCode = "03-825",
                        StateOrProvince = "mazowieckie"
                    },
                    new SignerRole("Wiceprezes Zarządu"));

                var filePath = Path.Combine(info.Directory, info.SignatureFileName);
                if (File.Exists(filePath)) { File.Delete(filePath); }
                result.Save(filePath);
            }

            signedPackageManager.Compress(info);
        }

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

                //var mgr = new XmlDsigEnveloped(false, null);
                //var vr = mgr.VerifyFile(filePath);
                //Assert.IsTrue(vr);
                var validationResult = manager.ValidateSignature(filePath);
                Assert.IsTrue(validationResult.IsValid && result != null && result.Document != null && result.Document.OuterXml != null && File.Exists(filePath));
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
                     new SignerRole("Wiceprezes Zarządu"),
                     "nes_20_generated.pdf");

                var filePath = Path.Combine(Path.GetTempPath(), "nes_20_generated.pdf.xades");
                if (File.Exists(filePath)) { File.Delete(filePath); }
                result.Save(filePath);

                Assert.IsTrue(result != null && result.Document != null && result.Document.OuterXml != null && File.Exists(filePath));

                System.Diagnostics.Process.Start(filePath);
            }
        }

        [TestMethod]
        public void XadesManager_CreateDetachedSignature() {
            var path = "../../../doc/nes_20_generated.pdf";
            using (var manager = new XadesManager()) {
                var result = manager.CreateDetachedSignature(new FileInfo(path).FullName, CertUtil.SelectCertificate(),
                     new SignatureProductionPlace() {
                         City = "Warszawa",
                         CountryName = "Polska",
                         PostalCode = "03-825",
                         StateOrProvince = "mazowieckie"
                     },
                     new SignerRole("Wiceprezes Zarządu"));

                File.Copy(path, Path.Combine(Path.GetTempPath(), "nes_20_generated.pdf"), true);
                var filePath = Path.Combine(Path.GetTempPath(), "nes_20_generated.pdf.xades");
                if (File.Exists(filePath)) { File.Delete(filePath); }
                result.Save(filePath);

                Assert.IsTrue(result != null && result.Document != null && result.Document.OuterXml != null && File.Exists(filePath));

                System.Diagnostics.Process.Start(filePath);
            }
        }
    }
}
