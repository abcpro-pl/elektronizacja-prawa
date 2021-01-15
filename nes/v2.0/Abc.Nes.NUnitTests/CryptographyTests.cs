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
    }
}