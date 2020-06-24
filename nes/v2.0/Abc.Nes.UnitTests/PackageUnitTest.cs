using Abc.Nes.ArchivalPackage;
using Abc.Nes.ArchivalPackage.Model;
using Abc.Nes.Enumerations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

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
    }
}
