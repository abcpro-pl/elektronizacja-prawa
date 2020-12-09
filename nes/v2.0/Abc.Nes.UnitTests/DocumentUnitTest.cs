/*=====================================================================================

	ABC NES 
	(C)2002 - 2020 ABC PRO sp. z o.o.
	http://abcpro.pl
	
	Author: (C)2009 - 2020 ITORG Krzysztof Radzimski
	http://itorg.pl

    License: GPL-3.0-or-later
    https://licenses.nuget.org/GPL-3.0-or-later

  ===================================================================================*/

using Abc.Nes.Generators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Abc.Nes.UnitTests {
    [TestClass]
    public class DocumentUnitTest {

        [TestMethod]
        public void Document_XsdGenerator_GetSchema() {
            XElement schema;
            using (var xsdGenerator = new XsdGenerator()) {
                schema = xsdGenerator.GetSchema();
            }            
            var filePath = @"..\..\..\Metadane-2.0.xsd";
            schema.Save(filePath);
            Assert.IsTrue(schema != null);
        }
        [TestMethod]
        public void Document_XsdGenerator17_GetSchema() {
            XElement schema;
            using (var xsdGenerator = new XsdGenerator()) {
                schema = xsdGenerator.GetSchema(typeof(Document17));
            }            
            var filePath = @"..\..\..\Metadane-1.7.xsd";
            schema.Save(filePath);
            Assert.IsTrue(schema != null);
        }

        [TestMethod]
        public void Document_XmlConverter_WriteXml() {
            Abc.Nes.Document document = GetModel();
            var filePath = Path.Combine(Path.GetTempPath(), "nes.xml");
            new Abc.Nes.Converters.XmlConverter().WriteXml(document, filePath);
            Assert.IsTrue(File.Exists(filePath));
        }

        [TestMethod]
        public void Document_XmlConverter_LoadXml() {
            var filePath = Path.Combine(Path.GetTempPath(), "nes.xml");
            if (!File.Exists(filePath)) { Document_XmlConverter_WriteXml(); }
            var document = new Abc.Nes.Converters.XmlConverter().LoadXml(filePath) as Document;
            Assert.IsTrue(document != null && document.Groupings != null && document.Groupings.Count > 0);
        }


        [TestMethod]
        public void Document_XmlConverter_Validate() {
            var filePath = Path.Combine(Path.GetTempPath(), "nes.xml");
            if (!File.Exists(filePath)) { Document_XmlConverter_WriteXml(); }
            var c = new Abc.Nes.Converters.XmlConverter();
            var valid = c.Validate(filePath);
            Assert.IsTrue(valid && c.ValidationErrors.Count == 0);
        }


        [TestMethod]
        public void Document_XmlConverter_GetValidationResult() {
            var c = new Abc.Nes.Validators.DocumentValidator();
            var result = c.Validate(GetModel(true));
            foreach (var item in result) {
                System.Diagnostics.Debug.WriteLine(item.DefaultMessage);
            }
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public void Document_XmlConverter_WriteJson() {
            Abc.Nes.Document document = GetModel();
            var filePath = Path.Combine(Path.GetTempPath(), "nes.json");
            new Abc.Nes.Converters.JsonConverter().WriteJson(document, filePath);
            Assert.IsTrue(File.Exists(filePath));
        }

        public static Abc.Nes.Document GetModel(bool incorrect = false) {
            var document = new Abc.Nes.Document() {
                Identifiers = new List<Abc.Nes.Elements.IdentifierElement> {
                    new Abc.Nes.Elements.IdentifierElement() {
                        Type = Abc.Nes.Elements.IdentifierElement.GetIdTypes(Enumerations.IdTypes.ObjectMark),
                        Value = "ABC-A.123.77.3.2011.JW.",
                        Subject = new Elements.SubjectElement(){
                            Institution = new Elements.InstitutionElement(){
                                Name = "Urząd Miasta Wołomierz"
                            }
                        }
                    }
                },
                Titles = new List<Abc.Nes.Elements.TitleElement> {
                    new Abc.Nes.Elements.TitleElement(){
                        Original = new Abc.Nes.Elements.TitleWithLanguageCodeElement(){
                            Type = Enumerations.LanguageCode.pol,
                            Value = "Tytuł dokumentu"
                        },
                        Alternative = new List<Elements.TitleWithLanguageCodeElement> {
                            new Elements.TitleWithLanguageCodeElement() {
                                Type = Enumerations.LanguageCode.eng,
                                Value = "Document title"
                            }
                        }
                    }
                },
                Dates = new List<Elements.DateElement> {
                    new Elements.DateElement() {
                        Type = Enumerations.EventDateType.Created,
                        Date = "2020-06-22"
                    }
                },
                Formats = new List<Elements.FormatElement> {
                    new Elements.FormatElement() {
                        Type = ".pdf",
                        Specification = "1.7",
                        Uncompleted = Enumerations.BooleanValues.False,
                        Size = new Elements.SizeElement(){
                            Measure = Elements.SizeElement.GetSizeType(Enumerations.FileSizeType.kB),
                            Value = "4712"
                        }
                    }
                },
                Access = new List<Elements.AccessElement> {
                    new Elements.AccessElement() {
                        Access = Enumerations.AccessType.Public,
                        Description = "Uwagi dotyczące dostępności",
                        Date = new Elements.AccessDateElement() {
                            Type = Enumerations.AccessDateType.After,
                            Date = "2020-06-23"
                        }
                    }
                },
                Types = new List<Elements.TypeElement>() {
                    new Elements.TypeElement() {
                        Class = Elements.TypeElement.GetDocumentClassType(Enumerations.DocumentClassType.Text),
                        Kinds = new List<string> { Elements.TypeElement.GetDocumentKindType(Enumerations.DocumentKindType.Document) }
                    }
                },
                Groupings = new List<Elements.GroupingElement> {
                    new Elements.GroupingElement() {
                        Type = "Rejestr korespondencji przychodzącej",
                        Code = "RKP01",
                        Description = "tekstowy opis grupy"
                    }
                },
                Authors = new List<Elements.AuthorElement> {
                    new Elements.AuthorElement() {
                        Functions = new List<string> { Elements.AuthorElement.GetAuthorFunctionType(Enumerations.AuthorFunctionType.Created) },
                        Subject = new Elements.SubjectElement() {
                            Institution = new Elements.InstitutionElement() {
                                Name = "Urząd Miasta Wołomierz"
                            }
                        }
                    }
                },
                Senders = new List<Elements.SenderElement> {
                    new Elements.SenderElement() {
                        Subject = new Elements.SubjectElement() {
                            Institution = new Elements.InstitutionElement() {
                                Name = "Urząd Miasta Wołomierz"
                            }
                        }
                    }
                },
                Recipients = new List<Elements.RecipientElement> {
                    new Elements.RecipientElement() {
                        CC = Enumerations.BooleanValues.False,
                        Subject = new Elements.SubjectElement() {
                            Institution = new Elements.InstitutionElement() {
                                Name = "Urząd Miasta Wołomierz"
                            }
                        }
                    },
                    new Elements.RecipientElement() {
                        CC = Enumerations.BooleanValues.True,
                        Subject = new Elements.SubjectElement() {
                            Institution = new Elements.InstitutionElement() {
                                Name = "Regionalna Izba Obrachunkowa w Łodzi"
                            }
                        }
                    }
                },
                Relations = new List<Elements.RelationElement> {
                    new Elements.RelationElement {
                        Identifiers = new List<Elements.IdentifierElement> {
                            new Elements.IdentifierElement() {
                                Type = "SystemID",
                                Value = "P00112233.pdf.xades"
                            }
                        },
                        Type = Elements.RelationElement.GetRelationType(Enumerations.RelationType.HasReference)
                    },
                    new Elements.RelationElement {
                        Identifiers = new List<Elements.IdentifierElement> {
                            new Elements.IdentifierElement() {
                                Type = "SystemID",
                                Value = "dek2010123.txt"
                            }
                        },
                        Type = Elements.RelationElement.GetRelationType(Enumerations.RelationType.HasAttribution)
                    },
                    new Elements.RelationElement {
                        Identifiers = new List<Elements.IdentifierElement> {
                            new Elements.IdentifierElement() {
                                Type = "SystemID",
                                Value = "P00112233.docx"
                            }
                        },
                        Type = Elements.RelationElement.GetRelationType(Enumerations.RelationType.IsVersion)
                    },
                    new Elements.RelationElement {
                        Identifiers = new List<Elements.IdentifierElement> {
                            new Elements.IdentifierElement() {
                                Type = "SystemID",
                                Value = "UPD12345.xml"
                            }
                        },
                        Type = Elements.RelationElement.GetRelationType(Enumerations.RelationType.HasReference)
                    }
                },
                Qualifications = new List<Elements.QualificationElement> {
                    new Elements.QualificationElement() {
                        Type = Elements.QualificationElement.GetArchivalCategoryType(Enumerations.ArchivalCategoryType.BE10),
                        Date = "2005-03-05",
                        Subject = new Elements.SubjectElement() {
                            Institution = new Elements.InstitutionElement() {
                                Name = "Urząd Miasta Wołomierz",
                                Unit = new Elements.InstitutionUnitElement() {
                                    Name = "Departament Usług i Rozwoju",
                                    Unit = new Elements.InstitutionUnitElement() {
                                        Name = "Wydział Rozwoju Systemów",
                                        Employee = new Elements.EmployeeElement() {
                                            FirstNames = new List<string> { "Jan" },
                                            Surname = "Kowalski",
                                            Position = "Specjalista",
                                            Contacts = new List<Elements.ContactElement> {
                                                new Elements.ContactElement() {
                                                    Type = Elements.ContactElement.GetContactType(Enumerations.ContactType.Email),
                                                    Value = "jkowalski@mc.gov.pl"
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                Languages = new List<Elements.LanguageElement>() {
                    new Elements.LanguageElement() {
                        Type = Enumerations.LanguageCode.pol,
                        Value = "polski"
                    }
                },
                Description = "projekt dokumentu \"Requirements for elaboration and implementation of information system of General department of Archives\", przekazny przez Przewodniczącą Departamentu Generalnego Archiwów przy Radzie Ministrów Republiki Bułgarii",
                Keywords = new List<Elements.KeywordElement> {
                    new Elements.KeywordElement() {
                        Matters = new List<string> { "handel" },
                        Places = new List<string> { "Polska" },
                        Dates = new List<Elements.DateElement> {
                            new Elements.DateElement() {
                                Range = Enumerations.DateRangeType.DateFromTo,
                                DateFrom = "2008",
                                DateTo = "2012"
                            }
                        },
                        Others = new List<Elements.KeywordDataElement> {
                            new Elements.KeywordDataElement() {
                                Key = "placówki handlowe",
                                Value = "Anna i Jan"
                            }
                        }
                    }
                },
                Rights = new List<string> { "© Unesco 2003 do polskiego tłumaczenia Naczelna Dyrekcja Archiwów Państwowych" },
                Locations = new List<string> { "Archiwum zakładowe Urzędu Miasta w Wołomierzu" },
                Statuses = new List<Elements.StatusElement> {
                    new Elements.StatusElement() {
                        Kind = "status dokumentu",
                        Version = "numer wersji",
                        Description = "opis"
                    }
                }
            };

            if (incorrect) {
                document.Identifiers.Clear();
                document.Groupings.First().Description = null;
            }

            return document;
        }
    }
}
