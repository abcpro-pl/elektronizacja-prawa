/*=====================================================================================

	ABC NES.ArchivalPackage 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/


using Abc.Nes.ArchivalPackage;
using Abc.Nes.ArchivalPackage.Cryptography;
using Abc.Nes.ArchivalPackage.Model;
using Abc.Nes.Enumerations;
using Abc.Nes.Xades.Signature.Parameters;
using Abc.Nes.Xades.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Abc.Nes.UnitTests {
    [TestClass]
    public class PackageUnitTest {
        [TestMethod]
        public void SignPdf() {
            var path = @"../../../sample/sample_file.pdf";
            var imagePath = @"../../../sample/legislator.png";
            var outputpath = @"../../../sample/sample_file.signed.pdf";
            var mgr = new PackageSignerManager();
            mgr.SignPdfFile(
                new FileInfo(path).FullName,
                CertUtil.SelectCertificate(),
                CommitmentTypeId.ProofOfOrigin,
                "Wołomierz-Łęczna",
                true,
                apperancePngImage: File.ReadAllBytes(new FileInfo(imagePath).FullName),
                apperancePngImageLocation: PdfSignatureLocation.Custom,
                apperanceLocationX: 360F,
                apperanceLocationY: 790F,
                apperanceWidth: 220F,
                apperanceHeight: 50F,
                margin: 10F,
                outputFilePath: new FileInfo(outputpath).FullName,
                addSignatureApperance: false
           );

            Assert.IsTrue(new FileInfo(outputpath).Exists);
            if (new FileInfo(outputpath).Exists) {
                System.Diagnostics.Process.Start(new FileInfo(outputpath).FullName);
            }
        }

        [TestMethod]
        public void Package_LoadPackageEzdPuw() {
            var path = @"../../../sample/EZD_PUW.zip";
            var outpath = @"../../../sample/EZD_PUW_generated.zip";
            var mgr = new PackageManager();
            mgr.LoadPackage(path);
            var isNotEmpty = mgr != null && mgr.Package != null && !mgr.Package.IsEmpty;
            Assert.IsTrue(isNotEmpty);
            mgr.Save(outpath);
        }

        [TestMethod]
        public void Package_LoadPackage() {
            var path = @"../../../sample/InvalidPackage.zip";
            var mgr = new PackageManager();
            mgr.LoadPackage(path);
            var isNotEmpty = mgr != null && mgr.Package != null && !mgr.Package.IsEmpty;
            Assert.IsTrue(isNotEmpty);
        }

        [TestMethod]
        public void Package_LoadValidatedPackage() {
            var path = @"../../../sample/ValidatedPackage.zip";
            var mgr = new PackageManager();
            mgr.LoadPackage(path);
            var isNotEmpty = mgr != null && mgr.Package != null && !mgr.Package.IsEmpty;
            Assert.IsTrue(isNotEmpty);
        }

        [TestMethod]
        public void Package_GetDocumentsCount() {
            var path = @"../../../sample/ValidatedPackage.zip";
            var mgr = new PackageManager();
            mgr.LoadPackage(path);
            var count = mgr.GetDocumentsCount();
            var isNotEmpty = mgr != null && mgr.Package != null && !mgr.Package.IsEmpty;
            Assert.IsTrue(isNotEmpty && count == 3);
        }

        [TestMethod]
        public void Package_Save() {
            var path = @"../../../sample/InvalidPackage.zip";
            var mgr = new PackageManager();
            mgr.LoadPackage(path);

            mgr.AddFile(new DocumentFile() {
                FileData = File.ReadAllBytes(@"../../../sample/sample_file.pdf"),
                FileName = "TabelaWydatkow.pdf"
            }, new Abc.Nes.Document() {
                Identifiers = new List<Elements.IdentifierElement>() {
                    new Elements.IdentifierElement() {
                        Type = "Numer tabeli",
                        Value = "3",
                        Subject = new Elements.SubjectElement() {
                            Person = new Elements.PersonElement() {
                                FirstNames = new List<string> { "Alojzy" },
                                Surname = "Bąbel",
                                Contacts = new List<Elements.ContactElement> {
                                    new Elements.ContactElement() {
                                        Type = ContactType.Email.GetContactType(),
                                        Value = "alojzy.babel@akademia-pana-kleksa.pl"
                                    }
                                }
                            }
                        }
                    }
                },
                Titles = new List<Elements.TitleElement> {
                    new Elements.TitleElement() {
                        Original = new Elements.TitleWithLanguageCodeElement(){
                            Type = LanguageCode.pol,
                            Value = "Tabela wydatków"
                        }
                    }
                },
                Dates = new List<Elements.DateElement> {
                    new Elements.DateElement() {
                        Type = EventDateType.Created,
                        Date = "2020-04-01 12:32:00"
                    }
                },
                Formats = new List<Elements.FormatElement> {
                    new Elements.FormatElement() {
                        Type = "PDF",
                        Specification = "1.7",
                        Uncompleted = BooleanValues.False,
                        Size = new Elements.SizeElement() {
                            Measure = FileSizeType.bajt.GetSizeType(),
                            Value = new FileInfo(@"../../../sample/sample_file.pdf").Length.ToString()
                        }
                    }
                },
                Access = new List<Elements.AccessElement> {
                    new Elements.AccessElement() {
                        Access = AccessType.Public
                    }
                },
                Types = new List<Elements.TypeElement> {
                    new Elements.TypeElement() {
                        Class = DocumentClassType.Text.GetDocumentClassType(),
                        Kinds = new List<string> { DocumentKindType.Regulation.GetDocumentKindType() }
                    }
                },
                Groupings = new List<Elements.GroupingElement> {
                    new Elements.GroupingElement() {
                        Type = "Rejestr wydatków",
                        Code = "KS_RW",
                        Description = "Księgowość: rejestr wydatków"
                    }
                 }
            });

            mgr.Save(@"../../../sample/ValidatedPackage.zip");
        }

        [TestMethod]
        public void Package_GetMetadataFile() {
            var result = false;
            var path = @"../../../sample/ValidatedPackage.zip";
            var mgr = new PackageManager();
            mgr.LoadPackage(path);
            var item = mgr.GetItemByFilePath("dokumenty/Wniosek/Wniosek.xml");
            if (item != null) {
                var metadataFile = mgr.GetMetadataFile(item);
                result = metadataFile != null;
            }

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Package_GetParentFolder() {
            var result = false;
            var path = @"../../../sample/ValidatedPackage.zip";
            var mgr = new PackageManager();
            mgr.LoadPackage(path);
            var item = mgr.GetParentFolder("dokumenty/Wniosek/Wniosek.xml");
            if (item != null) {
                result = item != null && item.GetItems().Any();
            }

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Package_GetAllFiles() {
            var path = @"../../../sample/ValidatedPackage.zip";
            var mgr = new PackageManager();
            mgr.LoadPackage(path);
            var items = mgr.GetAllFiles();
            var isNotEmpty = mgr != null && mgr.Package != null && !mgr.Package.IsEmpty;
            Assert.IsTrue(isNotEmpty && items.Count() == 8);
        }

        [TestMethod]
        public void PackageSignerManager_Sign() {
            var path = @"../../../sample/eNadzorPackage.zip";
            var outputPath = @"../../../sample/eNadzorPackage_SignedPackage.zip";
            using (var mgr = new PackageSignerManager()) {
                mgr.Sign(new FileInfo(path).FullName,
                    CertUtil.SelectCertificate(),
                    new FileInfo(outputPath).FullName,
                    new SignatureProductionPlace() {
                        City = "Warszawa",
                        CountryName = "Polska",
                        PostalCode = "03-825",
                        StateOrProvince = "mazowieckie"
                    },
                    new SignerRole("Wiceprezes Zarządu"),
                    true, // Podpisz pliki w paczce archiwalnej
                    true, // Podpisz paczkę archiwalną
                    true, // w pliku .xades umieść jedynie referencję do pliku paczki (podpis zewnętrzny - detached)
                    true, // dla plików w paczce w pliku .xades umieść jedynie referencję do pliku paczki (podpis zewnętrzny - detached)
                    true, // dodaj znacznik czasu
                    "http://time.certum.pl" // adres serwera znacznika czasu
                    );

            }
            Assert.IsTrue(File.Exists(outputPath));
        }

        [TestMethod]
        public void PackageSignerManager_SignDetached() {
            //var licPath = @"../../../../../../../Aspose.Total.lic";
            //new Aspose.Pdf.License().SetLicense(licPath);

            var path = @"../../../sample/ValidatedPackage.zip";
            var outputPath = @"../../../sample/SignedPackage.zip";
            using (var mgr = new PackageSignerManager()) {
                mgr.Sign(new FileInfo(path).FullName,
                    CertUtil.SelectCertificate(),
                    new FileInfo(outputPath).FullName,
                    new SignatureProductionPlace() {
                        City = "Warszawa",
                        CountryName = "Polska",
                        PostalCode = "03-825",
                        StateOrProvince = "mazowieckie"
                    },
                    new SignerRole("Wiceprezes Zarządu"),
                    true, // Podpisz pliki w paczce archiwalnej
                    true, // Podpisz paczkę archiwalną
                    true, // w pliku .xades umieść jedynie referencję do pliku paczki (podpis zewnętrzny - detached)
                    true  // Podpisz pliki w paczce archiwalnej inne niż XML i PDF podpisem zewnętrznym
                    );

            }

            var outputXadesFilePath = @"../../../sample/SignedPackage.zip.xades";
            Assert.IsTrue(File.Exists(outputXadesFilePath));
        }

        [TestMethod]
        public void Package_Validate() {
            var path = @"../../../sample/ValidatedPackage.zip";
            var mgr = new PackageManager();
            mgr.LoadPackage(path);
            var validateMetdataFiles = true;
            var breakOnFirstError = false;
            var result = mgr.Validate(out var message, validateMetdataFiles, breakOnFirstError);
            if (!result) {
                System.Diagnostics.Debug.WriteLine(message);
            }
            Assert.IsTrue(!result);
        }

        [TestMethod]
        public void Package_GetValidationResult() {
            var path = @"../../../sample/ValidatedPackage.zip";
            var mgr = new PackageManager();
            mgr.LoadPackage(path);
            var validateMetdataFiles = true;
            var breakOnFirstError = false;
            var result = mgr.GetValidationResult(validateMetdataFiles, breakOnFirstError);
            if (!result.IsCorrect) {
                foreach (var item in result) {
                    System.Diagnostics.Debug.WriteLine(item.DefaultMessage);
                }
            }
            Assert.IsTrue(!result.IsCorrect);
        }

        [TestMethod]
        public void PackageSignerManager_SignSpecifiedFile() {
            var path = @"../../../sample/ValidatedPackage.zip";
            var outputPath = @"../../../sample/SignedPackage.zip";
            using (var mgr = new PackageSignerManager()) {
                mgr.SignInternalFile(new FileInfo(path).FullName,
                    "Dokumenty/TabelaWydatkow.pdf",
                    CertUtil.SelectCertificate(),
                    new SignatureProductionPlace() {
                        City = "Warszawa",
                        CountryName = "Polska",
                        PostalCode = "03-825",
                        StateOrProvince = "mazowieckie"
                    },
                    new SignerRole("Wiceprezes Zarządu"),
                    false,
                    new FileInfo(outputPath).FullName
                 );
            }
            Assert.IsTrue(File.Exists(outputPath));
        }

        [TestMethod]
        public void GetSignatureInfo_Xades() {
            var path = @"../../../sample/SignedPackage.zip.xades";
            using (var mgr = new PackageSignerManager()) {
                var result = mgr.GetXadesSignatureInfos(path);
                Assert.IsTrue(result != null);
            }
        }
        [TestMethod]
        public void GetSignatureInfo() {
            var path = @"../../../sample/SignedPackage.zip";
            using (var mgr = new PackageSignerManager()) {
                var result = mgr.GetSignatureInfos(path, "Dokumenty/LegalAct.pdf");
                Assert.IsTrue(result != null);
            }
        }

        [TestMethod]
        public void GetSignatureInfos() {
            var path = @"../../../sample/LegalAct.zip";
            var list = new List<ArchivalPackage.Cryptography.Model.SignatureInfo>();
            using (var mgr = new PackageSignerManager()) {
                list.AddRange(mgr.GetSignatureInfos(path));
            }
            Assert.IsTrue(list.Count > 0);
        }

        [TestMethod]
        public void ValidateSignatures() {
            var path = @"../../../sample/LegalAct.zip";
            var list = new List<ArchivalPackage.Cryptography.Model.SignatureVerifyInfo>();
            using (var mgr = new PackageSignerManager()) {
                list.AddRange(mgr.VerifySignatures(path));
            }
            Assert.IsTrue(list.Count > 0);
        }

        [TestMethod]
        public void ValidateSignaturesInPackageFile() {
            var path = @"../../../sample/SignedPackage.zip";
            var list = new List<ArchivalPackage.Cryptography.Model.SignatureVerifyInfo>();
            using (var mgr = new PackageSignerManager()) {
                list.AddRange(mgr.VerifySignatures(path, "Dokumenty/LegalAct.zip.xades"));
            }
            Assert.IsTrue(list.Count > 0);
        }


        [TestMethod]
        public void ValidateXadesSignature() {
            var path = @"../../../sample/Sprawa_NPII.4131.1.815.2020_20201202_161026+.zip.xades";
            var list = new List<ArchivalPackage.Cryptography.Model.SignatureVerifyInfo>();
            using (var mgr = new PackageSignerManager()) {
                list.AddRange(mgr.VerifyXadesSignature(path));
            }
            Assert.IsTrue(list.Count > 0);
        }

        [TestMethod]
        public void ValidateZipxSignature() {
            var pathToPackage = @"../../../sample/cda64f92-4cd5-4809-9938-024cf96fa99c.zip";
            var pathToFileInPackage = "dokumenty/Zawiadomienie o wszczeciu postepowania nadzorczego.zip";
            using (var mgr = new PackageSignerManager()) {
                var signatures = mgr.GetSignatureInfos(pathToPackage, pathToFileInPackage);
                Assert.IsTrue(signatures.Length > 0);
            }
        }

        [TestMethod]
        public void LoadTarArchive() {
            var path = @"../../../sample/paczka_archiwalna_48.tar";
            var pathZip = @"../../../sample/paczka_archiwalna_48.tar.zip";

            using (var tar = new ArchivalPackage.Formats.Tar.TarFile(path)) {
                if (tar.ConvertToZip(pathZip)) {
                    var mgr = new PackageManager();
                    mgr.LoadPackage(pathZip);
                    var isNotEmpty = mgr != null && mgr.Package != null && !mgr.Package.IsEmpty;
                    Assert.IsTrue(isNotEmpty);
                }
            }
        }
        [TestMethod]
        public void LoadTarArchiveStream() {
            var path = @"../../../sample/paczka_archiwalna_48.tar";
            using (var stream = File.OpenRead(path)) {
                using (var tar = new ArchivalPackage.Formats.Tar.TarFile(stream, ArchivalPackage.Formats.Tar.TarType.Tar)) {
                    using (var zipStream = tar.ConvertToZip()) {
                        if (zipStream != null && zipStream.Length > 0) {
                            var mgr = new PackageManager();
                            mgr.LoadPackage(zipStream);
                            var isNotEmpty = mgr != null && mgr.Package != null && !mgr.Package.IsEmpty;
                            Assert.IsTrue(isNotEmpty);
                        }
                    }
                }
            }
        }
    }
}
