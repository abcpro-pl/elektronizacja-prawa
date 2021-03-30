using Abc.Nes.ArchivalPackage;
using Abc.Nes.ArchivalPackage.Cryptography;
using Abc.Nes.ArchivalPackage.Cryptography.Model;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace Abc.Nes.NUnitTests {
    public class CryptographyTests {
        [SetUp]
        public void Setup() {
        }

        [Test]
        public void ValidateXadesSignature() {
            var path = @"../../../../sample/Sprawa_NPII.4131.1.815.2020_20201202_161026+.zip.xades";
            var list = new List<SignatureVerifyInfo>();
            using (var mgr = new PackageSignerManager()) {
                list.AddRange(mgr.VerifyXadesSignature(path));
            }
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void ValidateZipxSignature() {
            var pathToPackage = @"../../../../sample/cda64f92-4cd5-4809-9938-024cf96fa99c.zip";
            var pathToFileInPackage = "dokumenty/Zawiadomienie o wszczeciu postepowania nadzorczego.zip";
            using (var mgr = new PackageSignerManager()) {
                var signatures = mgr.GetSignatureInfos(pathToPackage, pathToFileInPackage);
                Assert.IsTrue(signatures.Length > 0);
            }
        }

        [Test]
        public void GetSignAndVerifyInfo() {
            var pathToPackage = @"../../../../sample/cda64f92-4cd5-4809-9938-024cf96fa99c.zip";
            var pathToFileInPackage = "dokumenty/Zawiadomienie o wszczeciu postepowania nadzorczego.zip";
            using (var mgr = new PackageSignerManager()) {
                var signatures = mgr.GetSignAndVerifyInfo(pathToPackage, pathToFileInPackage);
                Assert.IsTrue(signatures.SignInfo != null && signatures.SignInfo.Length > 0);
            }
        }


        [Test]
        public void ValidatePackage() {
            var pathToPackage = @"../../../../sample/CorruptedPackage.zip";
            using (var mgr = new PackageManager()) {
                mgr.LoadPackage(pathToPackage);
                Assert.IsTrue(mgr.Package != null);
            }
        }

        [Test]
        public void Package_GetValidationResult() {
            var path = @"../../../../sample/Paczka.testowa.30.03.21.podpisana.zip";

            var signedPackageManager = new SignedPackageManager();
            var info = signedPackageManager.Extract(path);

            // validate package
            using (var mgr = new PackageManager()) {
                mgr.LoadPackage(Path.Combine(info.Directory, info.PackageFileName));
                var validateMetdataFiles = true;
                var breakOnFirstError = false;
                var result = mgr.GetValidationResult(validateMetdataFiles, breakOnFirstError);
                if (!result.IsCorrect) {
                    foreach (var item in result) {
                        System.Diagnostics.Debug.WriteLine(item.DefaultMessage);
                    }
                }

                Assert.IsTrue(result.IsCorrect);
            }

            using (var signerMgr = new PackageSignerManager()) {
                // get xades signature info
                var signatureInfo = signerMgr.GetXadesSignatureInfos(Path.Combine(info.Directory, info.SignatureFileName));
                Assert.IsTrue(signatureInfo.Length > 0);
                // validate xades
                var verifyInfo = signerMgr.VerifyXadesSignature(Path.Combine(info.Directory, info.SignatureFileName));
                Assert.IsTrue(verifyInfo != null && verifyInfo.Length > 0);
            }
        }       
    }
}