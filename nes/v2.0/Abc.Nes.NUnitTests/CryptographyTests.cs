using Abc.Nes.ArchivalPackage;
using Abc.Nes.ArchivalPackage.Cryptography;
using Abc.Nes.ArchivalPackage.Cryptography.Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Abc.Nes.NUnitTests {
    public class CryptographyTests {
        [SetUp]
        public void Setup() { }

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
        public void ValidatePackageSignatures() {
            var pathToPackage = @"../../../../sample/paczka_eADM_do_podpisu.podpisana.zip";
            var smgr = new SignedPackageManager();
            var info = smgr.Extract(pathToPackage);
            using (var mgr = new PackageSignerManager()) {
                var signatures = mgr.GetSignAndVerifyInfo(Path.Combine(info.Directory, info.PackageFileName));
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
        public void GetSignAndVerifyInfos() {
            var pathToPackage = @"../../../../sample/LegalActRIOzielonagora.zip";
            using (var mgr = new PackageSignerManager()) {
                var result = mgr.GetSignAndVerifyInfo(pathToPackage);
                Assert.IsTrue(result.Length > 0);
            }
        }

        [Test]
        public void XXX() {
            var pathToPackage = @"../../../../sample/LegalAct.Duplicate.zip";
            var packageSignerManager = new PackageSignerManager();

            var _signatureInfos = packageSignerManager.GetSignatureInfos(pathToPackage);
            if (_signatureInfos != null) { }
        }


        [Test]
        public void ValidatePackage() {
            Exception exception = null;
            var pathToPackage = @"../../../../sample/CorruptedPackage.zip";
            using (var mgr = new PackageManager()) {
                mgr.LoadPackage(pathToPackage, out exception);
                Assert.IsTrue(mgr.Package != null && exception == null);
            }
        }


        [Test]
        public void ValidatePackage2() {
            var pathToPackage = @"../../../../sample/BadPackage.zip";
            using (var mgr = new PackageManager()) {
                Exception exception;
                mgr.LoadPackage(pathToPackage, out exception);
                Assert.IsTrue(mgr.Package != null && exception == null);
            }
        }

        [Test]
        public void ValidatePackageEZDPUW() {
            var path = @"../../../../sample/_2016-10-28_18_33.tar";
            //var path = @"../../../../sample/_2006-12-15_11_45.tar";
            //var path = @"../../../../sample/Sprawy_PUW_BIA_20211109_090742.tar";
            using (var stream = File.OpenRead(path)) {
                using (var tar = new ArchivalPackage.Formats.Tar.TarFile(stream, ArchivalPackage.Formats.Tar.TarType.Tar)) {
                    using (var zipStream = tar.ConvertToZip()) {
                        if (zipStream != null && zipStream.Length > 0) {
                            var mgr = new PackageManager();
                            Exception exception;
                            mgr.LoadPackage(zipStream, out exception);
                            var isNotEmpty = mgr != null && mgr.Package != null && !mgr.Package.IsEmpty;
                            var validator = new ArchivalPackage.Validators.PackageValidator();
                            var result = validator.GetValidationResult(mgr.Package, true);
                            //Assert.IsTrue(isNotEmpty && exception == null && result.IsCorrect);

                            //mgr.Save("../../../../sample/_2006-12-15_11_45.zip");

                            //var mgr2 = new PackageManager();
                            //mgr2.AddFiles(mgr.Package.Documents.Items, "Wniosek", mgr.Package.Metadata.Items.Select(x=>x.Document));
                            //mgr2.Save("../../../../sample/_2006-12-15_11_33_test.zip");

                            Assert.IsTrue(isNotEmpty && exception == null && result.IsCorrect);
                        }
                    }
                }
            }
        }


        [Test]
        public void ValidateTarPackage() {
            var path = @"../../../../sample/Sprawy_PUW_BIA_20211109_090742.tar";
            using (var stream = File.OpenRead(path)) {
                using (var tar = new ArchivalPackage.Formats.Tar.TarFile(stream, ArchivalPackage.Formats.Tar.TarType.Tar)) {
                    using (var zipStream = tar.ConvertToZip()) {
                        if (zipStream != null && zipStream.Length > 0) {
                            var mgr = new PackageManager();
                            Exception exception;
                            mgr.LoadPackage(zipStream, out exception);
                            var isNotEmpty = mgr != null && mgr.Package != null && !mgr.Package.IsEmpty;
                            Assert.IsTrue(isNotEmpty && exception == null);
                        }
                    }
                }
            }
        }

        [Test]
        public void Package_GetValidationResult() {
            var path = @"../../../../sample/LegalAct26.03.2.podpisana.zip";

            var signedPackageManager = new SignedPackageManager();
            var info = signedPackageManager.Extract(path);

            // validate package
            using (var mgr = new PackageManager()) {
                Exception exception;
                mgr.LoadPackage(Path.Combine(info.Directory, info.PackageFileName), out exception);
                var validateMetdataFiles = true;
                var breakOnFirstError = false;
                var result = mgr.GetValidationResult(validateMetdataFiles, breakOnFirstError);
                if (!result.IsCorrect) {
                    foreach (var item in result) {
                        System.Diagnostics.Debug.WriteLine(item.DefaultMessage);
                    }
                }

                Assert.IsTrue(result.IsCorrect && exception == null);
            }

            using (var signerMgr = new PackageSignerManager()) {
                // get xades signature info
                var xadesPath = Path.Combine(info.Directory, info.SignatureFileName);
                if (File.Exists(xadesPath)) {
                    var signatureInfo = signerMgr.GetXadesSignatureInfos(xadesPath);
                    Assert.IsTrue(signatureInfo.Length > 0);
                    // validate xades
                    var verifyInfo = signerMgr.VerifyXadesSignature(Path.Combine(info.Directory, info.SignatureFileName));
                    Assert.IsTrue(verifyInfo != null && verifyInfo.Length > 0);
                }
            }
        }
    }
}