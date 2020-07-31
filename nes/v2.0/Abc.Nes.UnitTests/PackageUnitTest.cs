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
            }, new Document() {
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
                                        Type = Elements.ContactElement.GetContactType(ContactType.Email),
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
                            Measure = Elements.SizeElement.GetSizeType(FileSizeType.bajt),
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
                        Class = Elements.TypeElement.GetDocumentClassType(DocumentClassType.Text),
                        Kinds = new List<string> { Elements.TypeElement.GetDocumentKindType(DocumentKindType.Regulation) }
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
            //var licPath = @"../../../../../../../Aspose.Total.lic";
            //new Aspose.Pdf.License().SetLicense(licPath);

            var path = @"../../../sample/ValidatedPackage.zip";
            var outputPath = @"../../../sample/SignedPackage.xades";
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
                    false // w pliku .xades umieść jedynie referencję do pliku paczki (podpis zewnętrzny - detached)
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
    }
}
