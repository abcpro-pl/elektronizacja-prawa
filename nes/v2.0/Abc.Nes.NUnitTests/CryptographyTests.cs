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
        public void GetSignatureInfos_schema_v1_6() {
            var pathToPackage = @"../../../../sample/test paczka z zip.zip";
            var packageSignerManager = new PackageSignerManager();

            var _signatureInfos = packageSignerManager.GetSignatureInfos(pathToPackage);
            Assert.IsTrue(_signatureInfos != null && _signatureInfos.Length == 0);
        }

        [Test]
        public void XXX() {
            var pathToPackage = @"../../../../sample/LegalAct.Duplicate.zip";
            var packageSignerManager = new PackageSignerManager();

            var _signatureInfos = packageSignerManager.GetSignatureInfos(pathToPackage);
            Assert.IsTrue(_signatureInfos != null && _signatureInfos.Length > 0);
        }


        [Test]
        public void ValidatePackage() {
            Exception exception = null;
            var pathToPackage = @"../../../../sample/CorruptedPackage.zip";
            using (var mgr = new PackageManager()) {
                mgr.LoadPackage(pathToPackage, out exception);
                Assert.IsTrue(mgr.Package != null && exception != null);
            }
        }


        [Test]
        public void ValidatePackage1() {
            var pathToPackage = @"../../../../sample/BadPackage.zip";
            using (var mgr = new PackageManager()) {
                Exception exception;
                mgr.LoadPackage(pathToPackage, out exception);
                Assert.IsTrue(mgr.Package != null && exception != null);
            }
        }

        [Test]
        public void ValidatePackage2() {
            var pathToPackage = @"../../../../sample/BadPackage2.zip";
            using (var mgr = new PackageManager()) {
                Exception exception;
                mgr.LoadPackage(pathToPackage, out exception);
                Assert.IsTrue(mgr.Package != null && exception != null);
            }
        }
        [Test]
        public void ValidatePackage2Stream() {
            var pathToPackage = @"../../../../sample/BadPackage2.zip";
            using (var mgr = new PackageManager()) {
                Exception exception;
                using (var stream = File.OpenRead(pathToPackage)) {
                    mgr.LoadPackage(stream, out exception);

                    var file = mgr.Package.GetItemByFilePath("inne/InnePliki/Wniosek przychodzacy z odpowiedzia.pdf");
                    Assert.IsTrue(mgr.Package != null && file != null && exception != null);
                }
            }
        }
        [Test]
        public void ValidatePackage3Stream() {
            var pathToPackage = @"../../../../sample/BadPackage3.zip";
            using (var mgr = new PackageManager()) {
                Exception exception;
                using (var stream = File.OpenRead(pathToPackage)) {
                    mgr.LoadPackage(stream, out exception);

                    var file = mgr.Package.GetItemByFilePath("__inne__/akt.pdf");
                    Assert.IsTrue(mgr.Package != null && file != null && exception != null);
                }
            }
        }

        [Test]
        public void ValidatePackage4() {
            var pathToPackage = @"../../../../sample/paczka16.zip";
            using (var mgr = new PackageManager()) {
                Exception exception;
                mgr.LoadPackage(pathToPackage, out exception);
                Assert.IsTrue(mgr.Package != null && exception == null && mgr.Package.Metadata.Items.Count > 0);
            }
        }

        [Test]
        public void ValidatePackageEZDPUW() {
            var path = @"../../../../sample/paczka0.zip";

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

                            Assert.IsTrue(isNotEmpty && exception == null && result.IsCorrect);
                        }
                    }
                }
            }
        }

        [Test]
        public void ValidatePackageSignaturesEZDPUW() {
            var path = @"../../../../sample/paczka0.zip";
            var result = new List<Signature>();
            using (var stream = File.OpenRead(path)) {
                using (var tar = new ArchivalPackage.Formats.Tar.TarFile(stream, ArchivalPackage.Formats.Tar.TarType.Tar)) {
                    var pathToPackage = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");
                    var zipConvertResult = tar.ConvertToZip(pathToPackage);
                    if (zipConvertResult) {
                        if (File.Exists(pathToPackage)) {
                            using (var packageSignerManager = new PackageSignerManager()) {
                                var pathToFileInPackage = "dokumenty/E161491121.xml";
                                var verifiedSignaturesForDocument = packageSignerManager.VerifySignatures(pathToPackage, pathToFileInPackage);

                                var signatures = packageSignerManager.GetSignatureInfos(pathToPackage, pathToFileInPackage);

                                foreach (var signature in signatures) {
                                    var signatureForDocument = new Signature() {
                                        Author = signature.Author,
                                        CreateDate = signature.CreateDate,
                                        Type = signature.SignatureType == SignatureType.Pades ? "PAdeS" : "XAdES",
                                        Publisher = signature.Publisher,
                                        Number = signature.SignatureNumber,
                                        CommitmentType = signature.CommitmentTypeIndication,
                                        Position = signature.ClaimedRole,
                                        Institution = signature.Organization,
                                        IsCorrect = false,
                                        SignatureValid = false,
                                        CertyficateIsValid = false
                                    };

                                    var verifiedSignature = verifiedSignaturesForDocument
                                        .FirstOrDefault(x => x.SignatureName == signature.SignatureNumber);

                                    if (verifiedSignature != null) {
                                        signatureForDocument.IsCorrect = verifiedSignature.IsValid && verifiedSignature.CertValidationInfo.IsValid;
                                        signatureForDocument.SignatureValid = verifiedSignature.IsValid;
                                        signatureForDocument.CertyficateIsValid = verifiedSignature.CertValidationInfo.IsValid;
                                        signatureForDocument.VerifiedMessage = verifiedSignature.Message;
                                    }

                                    result.Add(signatureForDocument);
                                }
                            }

                            File.Delete(pathToPackage);
                        }
                    }
                }
            }
        }

        public class Signature {
            public Guid Id { get; set; }
            public string Number { get; set; }
            public string Type { get; set; }
            public string CommitmentType { get; set; }
            public string Author { get; set; }
            public string Position { get; set; }
            public string Institution { get; set; }
            public DateTime CreateDate { get; set; }
            public string Publisher { get; set; }
            public bool IsCorrect { get; set; }
            public string VerifiedMessage { get; set; }
            public bool SignatureValid { get; set; }
            public bool CertyficateIsValid { get; set; }
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
            //var path = @"../../../../sample/Sprawa_GN-III.7541.7.2018_20210301_143750.zip";
            

            var signedPackageManager = new SignedPackageManager();
            var info = signedPackageManager.Extract(path);

            // validate package
            using (var mgr = new PackageManager()) {
                Exception exception;
                mgr.LoadPackage(Path.Combine(info.Directory, info.PackageFileName), out exception);
                var validateMetdataFiles = true;
                var breakOnFirstError = false;
                var result = mgr.GetValidationResult(validateMetdataFiles, breakOnFirstError);
                //var res = result as Abc.Nes.ArchivalPackage.Validators.PackageValidationResult;
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

        [Test]
        public void Package_GetValidationResultWithErrors() {
            
            //var path = @"../../../../sample/Sprawa_GN-III.7541.7.2018_20210301_143750.zip";
            //var path = @"../../../../sample/Paczka z bledami 2.zip";
            var path = @"../../../../sample/aPaczka_17.03.21.zip";


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

                Assert.IsTrue(!result.IsCorrect && exception == null);

                //var res = result as Abc.Nes.ArchivalPackage.Validators.PackageValidationResult;
                //Assert.IsTrue(res.DocumentToMetadataErrors.Count == 1);
                //Assert.IsTrue(res.MetadataToDocumentErrors.Count == 1);
                //Assert.IsTrue(res.CaseMetadataErrors.Count == 0);
                //Assert.IsTrue(res.ConfirmationRelationErrors.Count == 2);
                //Assert.IsTrue(res.DocumentToCaseErrors.Count == 0);
                //Assert.IsTrue(res.MetadataErrors.Count == 0);
            }

            //using (var signerMgr = new PackageSignerManager()) {
            //    // get xades signature info
            //    var xadesPath = Path.Combine(info.Directory, info.SignatureFileName);
            //    if (File.Exists(xadesPath)) {
            //        var signatureInfo = signerMgr.GetXadesSignatureInfos(xadesPath);
            //        Assert.IsTrue(signatureInfo.Length > 0);
            //        // validate xades
            //        var verifyInfo = signerMgr.VerifyXadesSignature(Path.Combine(info.Directory, info.SignatureFileName));
            //        Assert.IsTrue(verifyInfo != null && verifyInfo.Length > 0);
            //    }
            //}
        }
    }
}