using Abc.Nes.ArchivalPackage.Cryptography;
using Abc.Nes.ArchivalPackage.Cryptography.Model;
using NUnit.Framework;
using System.Collections.Generic;

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
    }
}